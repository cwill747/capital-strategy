using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input.Touch;
using CapitalStrategy;
using CapitalStrategyServer.Messaging;
using System.Xml;
using CapitalStrategy.GUI;
using Lidgren.Network;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace CapitalStrategy.Windows
{
    public class GameMatch : Window
    {
        public Game1 windowManager { get; set; }
        public SpriteBatch spriteBatch { get; set; }
        public ContentManager Content { get; set; }
        
        public int ROWS = 10;
        public int COLS = 10;
        public int BOARDWIDTH = 600;
        public int BOARDHEIGHT = 600;
        public int SELECTED_WARRIOR_INFO_X = 610;
        public int SELECTED_WARRIOR_INFO_Y = 40;

        public Texture2D red;
        public Texture2D white;
        public Texture2D heartIcon;
        public Texture2D attackIcon;
        public Texture2D shieldIcon;
        public Texture2D moveIcon;
        public SpriteFont menufont;
        public SpriteFont infofont;
        public Texture2D hourglass;


        Texture2D background;
        Rectangle backgroundRec;
        public Board board { get; set; }
        WarriorType axestanShield;
        WarriorType whiteMage;
        WarriorType firedragon;
        WarriorType blueArcher;
        WarriorType crocy;
        WarriorType magier;
        Warrior selectedWarrior;
        Warrior currentTurnWarrior;
        public Boolean isYourTurn { get; set; }
        public Stack<int[,]> p1MovementStack;
        public Stack<int[,]> p2MovementStack;
        private bool warriorIsResetting;
        private int previousWarriorDirection;

        private int endOfInfoPaneLocation;
        public int turnProgress { get; set; }
        public int targetCol { get; set; }
        public int targetRow { get; set; }
        public Warrior beingAttacked { get; set; }
        Warrior displayWarrior;
        MouseWrapper mouseState;
        MouseState oldMouseState;

        // GUI STUFF
        Button movementBtn;
        Button attackBtn;
        Button skipBtn;

        public Boolean waitingForTurn { get; set; }

        //for after match
        public Boolean armyStillAround = false;

        public int opponentDamage { get; set; }

        public GameMatch(Game1 windowManager)
        {
            this.windowManager = windowManager;
        }
        public void Initialize()
        {
            this.turnProgress = TurnProgress.beginning;
            if (board != null)
            {
                this.board.loadWarriors(this.windowManager, true);
                this.board.loadWarriors(this.windowManager, false);
            }
            this.oldMouseState = new MouseState();

            p1MovementStack = new Stack<int[,]>();
            p2MovementStack = new Stack<int[,]>();
            warriorIsResetting = false;
            this.waitingForTurn = true;
        }
        public void LoadContent()
        {
            this.spriteBatch = windowManager.spriteBatch;
            this.Content = windowManager.Content;
            menufont = Content.Load<SpriteFont>("fonts/gamefont");
            this.infofont = Content.Load<SpriteFont>("fonts/menufont");

            //Song song = Content.Load<Song>("music/intoBattle");  // Put the name of your song in instead of "song_title"
            //MediaPlayer.Play(song);


            background = Content.Load<Texture2D>("stars");
            red = Content.Load<Texture2D>("colors/red");
            white = Content.Load<Texture2D>("colors/white");
            heartIcon = Content.Load<Texture2D>("icons/heartIcon");
            attackIcon = Content.Load<Texture2D>("icons/attackIcon");
            shieldIcon = Content.Load<Texture2D>("icons/shieldIcon");
            moveIcon = Content.Load<Texture2D>("icons/moveIcon");
            hourglass = Content.Load<Texture2D>("icons/hourglass");

            backgroundRec = new Rectangle(0, 0, windowManager.Window.ClientBounds.Width, windowManager.Window.ClientBounds.Height);
            
            board = new Board(ROWS, COLS, new Rectangle(0, 25, this.BOARDWIDTH, this.BOARDHEIGHT), Game1.tileImage);
            // Game1 game, int maxHealth, int attack, int defense, int accuracy, int evade, int maxMove, double speed, String type, int[] imageDimensions, int[] stateDurations, Point[] attackPoints, int? attackRange, int attackDelayConst, int attackDelayRate)

            XmlTextReader reader = new XmlTextReader("Configuration/WarriorTypes.xml");

            axestanShield = new WarriorType(
                this,       // game
                80,         // maxhealth
				2,			// maxCool (number of turns + 1)
                50,         // attack
                40,         // defense
                100,        // accuracy
                30,         // evade
                3,          // maxMove
                3,          // speed
                "axestan shield", // type 
                "melee", // baseType
                "ranged", new int[] { 1, 8, 8, 13, 7, 9, 7 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                null, 1, 500, 0, "swordStrike");
			firedragon = new WarriorType(this, 60, 3, 40, 70,
                80, 60, 4, 5, "firedragon", "magic",
                "melee", new int[] { 1, 7, 7, 9, 1, 11, 7 }, new int[] { 1000, 400, 1000, 1000, 1000, 1000, 1000 },
                null, 2, 500, 0, "dragonRoar");
			blueArcher = new WarriorType(this, 
                70,
                3, 40, 50,
                75, 20, 4, 3, "blue archer", "ranged",
                "magic", new int[] { 1, 8, 8, 13, 9, 13, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                null, 6, 500, 10, "bowShot");
			this.whiteMage = new WarriorType(this, 50, 2, -80, 30,
                100, 0, 4, 3, "white mage", "magic",
                "none", new int[] { 1, 8, 8, 13, 9, 13, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                new Point[] { new Point(-1,-1), new Point(1,1), 
					new Point(1,-1), new Point(-1,1), new Point(-2, 0), 
					new Point(2, 0), new Point(0, 2), new Point(0, -2), 
					new Point(1,0), new Point(-1,0),new Point(0,1),
					new Point(0,-1),new Point(0,0)},
                null, 500, 0, "heal");
			crocy = new WarriorType(this, 90, 2, 60, 50,
                50, 50, 2, 2, "crocy", "melee", 
                "ranged", new int[] { 1, 8, 8, 11, 9, 11, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                null, 1, 500, 0, "swordStrike");
			magier = new WarriorType(this,70, 2, 40, 45,
                75, 25, 3, 3, "magier", "magic",
                "melee", new int[] { 9, 7, 7, 9, 9, 10, 9 }, new int[] { 1000, 500, 1000, 1500, 1000, 1000, 1000 },
                null, 3, 500, 10, "thunder");

            PlayerArmy p1 = new PlayerArmy(Direction.N);
            p1.AddWarrior(axestanShield);
            p1.AddWarrior(whiteMage);
            p1.AddWarrior(firedragon);
            p1.AddWarrior(crocy);
            p1.AddWarrior(magier);
            p1.AddWarrior(blueArcher);

            PlayerArmy p2 = new PlayerArmy(Direction.S);
            p2.AddWarrior(axestanShield);
            p2.AddWarrior(whiteMage);
            p2.AddWarrior(blueArcher);
            p2.AddWarrior(crocy);
            p2.AddWarrior(magier);
            
            /*for (int i = 1; i < 2; i++)
            {
                //board.warriors[i == 0 ? 7 : 9 - 7][3] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 3, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, axestanShield);
                board.warriors[i == 0 ? 7 : 9 - 7][5] = new Warrior(this.board, 100, i == 0 ? 7 : 9 - 7, 5, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, axestanShield);
                board.warriors[i == 0 ? 9 : 9 - 9][5] = new Warrior(this.board, 101, i == 0 ? 9 : 9 - 9, 5, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, whiteMage);
               // board.warriors[i == 0 ? 9 : 9 - 9][2] = new Warrior(this.board, i == 0 ? 9 : 9 - 9, 2, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, whiteMage);
                board.warriors[i == 0 ? 6 : 9 - 6][7] = new Warrior(this.board, 102, i == 0 ? 6 : 9 - 6, 7, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, firedragon);
                //board.warriors[i == 0 ? 7 : 9 - 7][6] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 6, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, blueArcher);
               board.warriors[i == 0 ? 7 : 9 - 7][4] = new Warrior(this.board, 103,  i == 0 ? 7 : 9 - 7, 4, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, crocy);
                //board.warriors[i == 0 ? 8 : 9 - 8][3] = new Warrior(this.board, i == 0 ? 8 : 9 - 8, 3, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, magier);
                board.warriors[i == 0 ? 8 : 9 - 8][5] = new Warrior(this.board, 104, i == 0 ? 8 : 9 - 8, 5, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, magier);
                board.warriors[i == 0 ? 7 : 9 - 7][2] = new Warrior(this.board, 105, i == 0 ? 7 : 9 - 7, 2, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, blueArcher);
            }*/

            movementBtn = new Button("MOVEMENT", new Rectangle(SELECTED_WARRIOR_INFO_X, 300, 100, 25), Game1.smallFont);
            attackBtn = new Button("ATTACK", new Rectangle(movementBtn.location.X + movementBtn.location.Width, 300, 100, 25), Game1.smallFont);
            skipBtn = new Button("SKIP", new Rectangle(attackBtn.location.X + attackBtn.location.Width, 300, 100, 25), Game1.smallFont);


            mouseState = new MouseWrapper(board, Mouse.GetState());
        }
        public void Update(GameTime gameTime)
        {
            if (isYourTurn)
            {
                //for end game
                this.armyStillAround = false;
                Boolean endCheck = true;
                if (!armyStillAround)
                {
                    for (int i = 0; i < ROWS; i++)
                    {
                        if (!armyStillAround)
                        {
                            for (int j = 0; j < COLS; j++)
                            {
                                Warrior unitC = board.warriors[i][j];
                                if (unitC != null && unitC.isYours)
                                {
                                    armyStillAround = true;
                                    endCheck = false;
                                }
                            }
                        }
                    }
                }
                // manages button states
                switch(this.turnProgress)
                {
                    case TurnProgress.beginning:
                    case TurnProgress.moving:
                        movementBtn.isDisabled = true;
                        attackBtn.isDisabled = false;
                        skipBtn.isDisabled = false;
                        break;
                    case TurnProgress.moved:
                    case TurnProgress.attacking:
                        movementBtn.isDisabled = false;
                        attackBtn.isDisabled = true;
                        skipBtn.isDisabled = false;
                        break;
                    case TurnProgress.attacked:
                        movementBtn.isDisabled = true;
                        attackBtn.isDisabled = true;
                        skipBtn.isDisabled = true;
                        break;
                    default:
                        break;
                }
                movementBtn.update(gameTime);
                attackBtn.update(gameTime);
                skipBtn.update(gameTime);


                if (endCheck)
                {
                    int newGameState = Game1.gameStates.Pop();
                    this.windowManager.gameState = newGameState;
                    this.windowManager.windows[newGameState].Initialize();
                }
            }
            else
            {
                
                    this.armyStillAround = false;
                    Boolean endCheck = true;
                    if (!armyStillAround)
                    {
                        for (int i = 0; i < ROWS; i++)
                        {
                            if (!armyStillAround)
                            {
                                for (int j = 0; j < COLS; j++)
                                {
                                    Warrior unitC = board.warriors[i][j];
                                    if (unitC != null && !unitC.isYours)
                                    {
                                        armyStillAround = true;
                                        endCheck = false;
                                    }
                                }
                            }
                        }
                    }

                    if (endCheck)
                    {
                        int newGameState = Game1.gameStates.Pop();
                        this.windowManager.gameState = newGameState;
                        this.windowManager.windows[newGameState].Initialize();
                    }
                    // end of end game
                
            }


            /*
            if (this.turnProgress == TurnProgress.beginning && this.cooldownCounter == 0)
            {


				if (this.isYourTurn) {
					for (int i = 0; i < ROWS; i++) {
						for (int j = 0; j < COLS; j++) {
							Warrior unitC = board.warriors [i] [j];
							if (unitC != null && unitC.isYours && unitC.cooldown > 0) {
								unitC.cooldown -= 1;
							}
						}
					}
                }
                else if (!this.isYourTurn)
                {
                    for (int i = 0; i < ROWS; i++)
                    {
                        for (int j = 0; j < COLS; j++)
                        {
                            Warrior unitC = board.warriors[i][j];
                            if (unitC != null && !unitC.isYours && unitC.cooldown > 0)
                            {
                                unitC.cooldown -= 1;
                            }
                        }
                    }
                }
				cooldownCounter = 1;
			}
            */


            if (this.currentTurnWarrior != null)
            {
                if (this.turnProgress == TurnProgress.moving)
                {
                    Boolean finishedMoving = currentTurnWarrior.move(gameTime);
                    if (finishedMoving && !warriorIsResetting)
                    {
                        this.turnProgress = TurnProgress.moved;
                        if (!this.isYourTurn)
                        {
                            if (this.targetRow < 0)
                            {
                                this.turnProgress = TurnProgress.attacked;
                            }
                            else
                            {
                                this.turnProgress = TurnProgress.attacking;
                                int xDiff = (int)(currentTurnWarrior.col - beingAttacked.col);
                                int yDiff = (int)(currentTurnWarrior.row - beingAttacked.row);
                                beingAttacked.setDirection(xDiff, yDiff);
                                beingAttacked.takeHit(currentTurnWarrior.getAttackDelay(xDiff, yDiff));
                                this.currentTurnWarrior.beginAttack(this.targetRow, this.targetCol);
                            }
                        }
                    }
                    else if (finishedMoving && warriorIsResetting)
                    {
                        this.turnProgress = TurnProgress.beginning;
                        this.currentTurnWarrior.direction = previousWarriorDirection;
                        this.currentTurnWarrior = null;
                        this.movementBtn.isDisabled = true;
                        this.attackBtn.isDisabled = false;
                        this.attackBtn.isDisabled = false;
                        this.board.resetTints();
                        this.warriorIsResetting = false;
                        this.selectedWarrior = null;
                    }
                }
                if (this.turnProgress == TurnProgress.moved)
                {
                    this.currentTurnWarrior.drawAttackRange();
                }
                if (this.turnProgress == TurnProgress.targetAcquired)
                {
                    this.board.tileTints[this.targetRow][this.targetCol] = Warrior.targetAcquiredColor;
                }
                if (this.turnProgress == TurnProgress.attacked)
                {

                    
                    
                    // calculate damage

                    if (this.beingAttacked != null)
                    {

                        int targetHealthCheck = this.beingAttacked.health;
                        if (this.isYourTurn)
                        {
                            this.opponentDamage = this.currentTurnWarrior.strike(this.beingAttacked);
                        }
                        else
                        {
                            this.beingAttacked.health -= this.opponentDamage;
                        }
                        
                        if (targetHealthCheck != this.beingAttacked.health)
                        {
                            // int xDiff = (int)(currentTurnWarrior.col - beingAttacked.col);
                            // int yDiff = (int)(currentTurnWarrior.row - beingAttacked.row);
                            //beingAttacked.setDirection(xDiff, yDiff);
                            // beingAttacked.takeHit(currentTurnWarrior.getAttackDelay(xDiff, yDiff));
                            this.currentTurnWarrior.cooldown = this.currentTurnWarrior.maxCooldown;

                        }



                    }

                    

                    this.turnProgress = TurnProgress.turnOver;
                    
                }
                if (turnProgress == TurnProgress.turnOver)
                {
                    if (this.isYourTurn)
                    {
                        this.waitingForTurn = true;
                    }
                    // add message here
                    this.decrementCooldowns();
                    this.isYourTurn = !this.isYourTurn;
                    this.turnProgress = TurnProgress.beginning;
                    
                    if (!this.isYourTurn)
                    {
                        Message toSend = new Message(msgType.Move, this.windowManager.client.UniqueIdentifier, this.windowManager.otherPlayer.uniqueIdentifier,
                            new int[2] { (int)this.currentTurnWarrior.row, (int)this.currentTurnWarrior.col},
                            new int[2] { this.targetRow, this.targetCol },
                            0,
                            0,
                            this.currentTurnWarrior.id,
                            false);

                        if (this.beingAttacked != null)
                        {
                            toSend.attackedUnitID = this.beingAttacked.id;
                            toSend.attackedLocation = new int[2] { (int) this.beingAttacked.row, (int) this.beingAttacked.col};
                            toSend.damageDealt = this.opponentDamage;
                        }
                        /*
                        Message message = new Message();
                        message.attackedLocation = new int[2] { 2, 2 };
                        message.attackedUnitID = 2;
                        message.attackerUnitID = 105;
                        message.damageDealt = 30;
                        message.attackedLocation = new int[2] { 5, 3 };
                        message.endLocation = new int[2] { 8, 1 };
                         */
                        NetOutgoingMessage om = this.windowManager.client.CreateMessage();
                        toSend.handleMoveMessage(ref om);
                        this.windowManager.client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
                    }
                    
                }
            }
            mouseState.update(Mouse.GetState());

            if (mouseState.wasClicked() && mouseState.isOverGrid)
            {
                this.handleClickOverGrid();
                oldMouseState = mouseState.mouseState;

            }
            if (!mouseState.Equals(oldMouseState))
            {
                if (this.turnProgress == TurnProgress.moved)
                {
                    if (mouseState.mouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (this.movementBtn.checkClick(mouseState.mouseState))
                        {

                        }
						if (this.skipBtn.checkClick(mouseState.mouseState))
						{

						}
                    }
                    if (mouseState.mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (this.movementBtn.unClick(mouseState.mouseState))
                        {
                            this.turnProgress = TurnProgress.beginning;
                            if (this.isYourTurn)
                            {
                                int[,] lastMove = p1MovementStack.Pop();
                                int[] movedFrom = { lastMove[0, 0], lastMove[0, 1] };
                                int[] movedTo = { lastMove[1, 0], lastMove[1, 1] };
                                this.selectedWarrior = this.board.warriors[movedTo[0]][movedTo[1]];
                                this.selectedWarrior.moveTo(movedFrom[0], movedFrom[1]);
                                this.turnProgress = TurnProgress.moving;
                                board.resetTints();
                                this.currentTurnWarrior = selectedWarrior;
                                warriorIsResetting = true;
                                this.previousWarriorDirection = lastMove[2, 0];
                                //this.currentTurnWarrior.state = State.
                            }
                        }

						if (this.skipBtn.unClick(mouseState.mouseState))
                        {
							if(this.isYourTurn)
							{
								this.turnProgress = TurnProgress.turnOver;
							}
						}
                    }
                }
                oldMouseState = mouseState.mouseState;

            }


            // TODO: Add your update logic here
            for (int row = 0; row < board.warriors.Length; row++)
            {
                for (int col = 0; col < board.warriors[row].Length; col++)
                {
                    Warrior warrior = board.warriors[row][col];
                    if (warrior != null)
                    {
                        warrior.update(gameTime, this);
                    }

                }
            }
            if (displayWarrior != null)
            {
                displayWarrior.update(gameTime, this);
            }
        }
        public void decrementCooldowns()
        {
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLS; j++)
                {
                    Warrior unitC = board.warriors[i][j];
                    if (unitC != null && unitC != this.currentTurnWarrior && ((unitC.isYours && this.isYourTurn) || (!unitC.isYours && !this.isYourTurn)) && unitC.cooldown > 0)
                    {
                        unitC.cooldown -= 1;
                    }
                }
            }
        }
        public void Draw()
        {
            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(Game1.background, backgroundRec, Color.White);
            
            
            for (int i = 0; i < ROWS; i++)
                    {  
                           for (int j = 0; j < COLS; j++)
                            {
                                Warrior warrior = board.warriors[i][j];
                                if (warrior != null)
                                {
                                    int tileWidth = this.BOARDWIDTH / this.board.cols;
                                    int tileHeight = this.BOARDHEIGHT / this.board.rows;
                                    int xLoc = (int)(warrior.col * tileWidth + board.location.X);
                                    int yLoc = (int)(warrior.row * tileHeight + board.location.Y);
                                    int iconHeight = tileHeight / 2;
                                    int toDrawY = yLoc + (iconHeight / 2);
                                    int toDrawX = xLoc;
                                    int iconWidth = tileWidth / 2;
                                    if (warrior.cooldown != 0)
                                    {
                                        int cool = warrior.cooldown;
                                        string coolString = cool.ToString();
                                        this.spriteBatch.Draw(hourglass, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                                        this.spriteBatch.DrawString(this.infofont, coolString, new Vector2(xLoc + iconWidth, yLoc + (iconHeight / 3)), Color.White);
                                    }
                                }
                            }
                       
                    }
                



            spriteBatch.End();
            board.drawTiles(spriteBatch);
            for (int row = 0; row < board.warriors.Length; row++)
            {
                for (int col = 0; col < board.warriors[row].Length; col++)
                {
                    Warrior warrior = board.warriors[row][col];
                    if (warrior != null)
                    {
                        warrior.draw();
                    }
                }
            }
            if (mouseState.isOverGrid && board.warriors[mouseState.row][mouseState.col] != null)
            {
                this.drawHealthBar(mouseState.row, mouseState.col);
            }
            this.drawInfoFrame(this.selectedWarrior);
            this.spriteBatch.Begin();
            string turnInfo = (this.isYourTurn) ? "It is your turn" : "Waiting for opponent";
            this.spriteBatch.DrawString(this.infofont, turnInfo, new Vector2(SELECTED_WARRIOR_INFO_X, SELECTED_WARRIOR_INFO_Y), Color.White);


            this.spriteBatch.End();

            if (this.isYourTurn)
            {
                this.movementBtn.draw(windowManager.spriteBatch);
                this.attackBtn.draw(windowManager.spriteBatch);
                this.skipBtn.draw(windowManager.spriteBatch);

            }
        }




        public void handleClickOverGrid()
        {
            this.board.resetTints();
            this.board.tileTints[mouseState.row][mouseState.col] = Color.LightBlue;
            // Draw right side panel information about this warrior

            if (this.turnProgress == TurnProgress.beginning)
            {

                if (selectedWarrior != null && 
					(this.isYourTurn && this.selectedWarrior.isYours)
					&& selectedWarrior.cooldown <= 0)
                {
                    // find if this is a valid move
                    if (selectedWarrior.isValidMove(board, mouseState.row, mouseState.col))
                    {
                        selectedWarrior.moveTo(mouseState.row, mouseState.col);
                        if (this.isYourTurn)
                        {
                            p1MovementStack.Push(new int[,] { { (int)selectedWarrior.row, (int)selectedWarrior.col }, { mouseState.row, mouseState.col }, {selectedWarrior.direction, 0}});
                        }
                        else if (!this.isYourTurn)
                        {
                            p2MovementStack.Push(new int[,] { { (int)selectedWarrior.row, (int)selectedWarrior.col }, { mouseState.row, mouseState.col }, { selectedWarrior.direction, 0 } });
                        }
                        this.turnProgress = TurnProgress.moving;
                        this.currentTurnWarrior = selectedWarrior;
                    }
                }
                this.selectedWarrior = board.warriors[mouseState.row][mouseState.col];
                if (this.selectedWarrior != null && this.selectedWarrior.cooldown == 0)
                {
                    //System.Diagnostics.Debug.WriteLine("here");
                    selectedWarrior.updateUserOptions(this.isYourTurn);
                }
            }
            else if (this.isYourTurn && this.turnProgress == TurnProgress.moved)
            {
                

                // clicking to acquire target
                if (this.currentTurnWarrior.isStrikable((int)this.currentTurnWarrior.row, (int)this.currentTurnWarrior.col, this.mouseState.row, this.mouseState.col))
                {
                    this.turnProgress = TurnProgress.targetAcquired;
                    this.targetRow = this.mouseState.row;
                    this.targetCol = this.mouseState.col;
                }
            }
            else if ( this.isYourTurn && this.turnProgress == TurnProgress.targetAcquired)
            {
                if (this.mouseState.row == this.targetRow && this.mouseState.col == this.targetCol)
                {
                    this.turnProgress = TurnProgress.attacking;
                    this.beingAttacked = this.board.warriors[this.targetRow][this.targetCol];
                    this.currentTurnWarrior.beginAttack(this.targetRow, this.targetCol);
                    
                    if (beingAttacked != null)
                    {
                        int xDiff = (int)(currentTurnWarrior.col - beingAttacked.col);
                        int yDiff = (int)(currentTurnWarrior.row - beingAttacked.row);
                        //beingAttacked.setDirection(xDiff, yDiff);

                        string attackSound = currentTurnWarrior.attackSound;
                        SoundEffect effect;     
                        effect = Content.Load<SoundEffect>(attackSound);
                        effect.Play();
                        
                        beingAttacked.takeHit(currentTurnWarrior.getAttackDelay(xDiff, yDiff));
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    this.turnProgress = TurnProgress.moved;
                    return;
                }
            }



        }

        public int drawHealthBar(int row, int col)
        {
            Warrior warrior = this.board.warriors[row][col]; // aleady checked if it was null
            // want to display health as bar across top -- two rectangles, white and red
            int tileWidth = this.BOARDWIDTH / this.board.cols;
            int tileHeight = this.BOARDHEIGHT / this.board.rows;
            int xLoc = (int)(warrior.col * tileWidth + board.location.X);
            int yLoc = (int)(warrior.row * tileHeight + board.location.Y);
            int healthBarY = yLoc - tileHeight / 2; // most warriors are taller than the tile
            // then attack, defense and move
            double widthPerHP = ((double)tileWidth) / 100;
            this.spriteBatch.Begin();
            this.spriteBatch.Draw(Game1.charcoal, new Rectangle(xLoc - 2, healthBarY - 2, (int)(widthPerHP * warrior.maxHealth) + 4, tileHeight / 10 + 4), Color.WhiteSmoke);
            this.spriteBatch.Draw(white, new Rectangle(xLoc, healthBarY, (int)(widthPerHP * warrior.maxHealth), tileHeight / 10), Color.WhiteSmoke);
            this.spriteBatch.Draw(red, new Rectangle(xLoc, healthBarY, (int)(widthPerHP * warrior.health), tileHeight / 10), Color.WhiteSmoke);

            //draw cooldown
            int iconHeight = tileHeight /2;
            int toDrawY = yLoc + (iconHeight /2);
            int toDrawX = xLoc;
            int iconWidth = tileWidth / 2;
            if (warrior.cooldown != 0)
            {
                int cool = warrior.cooldown;
                string cooldown = cool.ToString();
                this.spriteBatch.Draw(hourglass, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.DrawString(this.infofont, cooldown, new Vector2(xLoc+iconWidth, yLoc+(iconHeight/3)), Color.White);
            }
            this.spriteBatch.End();
            return healthBarY + tileHeight / 10;
        }
        public void drawInfoFrame(Warrior selectedWarrior)
        {
            if (selectedWarrior == null)
            {
                return;
            }
            int row = (int)selectedWarrior.row;
            int col = (int)selectedWarrior.col;
            int tileWidth = this.BOARDWIDTH / this.board.cols;
            int tileHeight = this.BOARDHEIGHT / this.board.rows;
            int xLoc = (int)(selectedWarrior.col * tileWidth);
            int yLoc = (int)(selectedWarrior.row * tileHeight);
            int bottomOfHealthBar = this.drawHealthBar(row, col);
            int totalDistance = yLoc + tileHeight - bottomOfHealthBar;
            // we have 3 things to draw, so split evenly
            int padding = (int)(.1 * totalDistance / 6);
            int iconHeight = (int)(.9 * totalDistance / 3);
            int toDrawY = bottomOfHealthBar + padding;
            int toDrawX = (int)(xLoc + tileWidth);
            int iconWidth = tileWidth / 4;
            double widthPerPoint = ((double)tileWidth) / 100;
            int barHeight = tileHeight / 10;

            /*
			this.spriteBatch.Begin();
			this.spriteBatch.Draw(attackIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
            this.spriteBatch.Draw(charcoal, new Rectangle(toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
			this.spriteBatch.Draw(white, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
			this.spriteBatch.Draw(red, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.attack), barHeight), Color.WhiteSmoke);
			toDrawY += iconHeight + padding;
			this.spriteBatch.Draw(shieldIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
            this.spriteBatch.Draw(charcoal, new Rectangle(toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
			this.spriteBatch.Draw(white, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
			this.spriteBatch.Draw(red, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.defense), barHeight), Color.WhiteSmoke);
			toDrawY += iconHeight + padding;



			//this.spriteBatch.Draw(moveIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
			//this.spriteBatch.Draw(white, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
			//this.spriteBatch.Draw(red, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.maxMove * 10), barHeight), Color.WhiteSmoke);
			this.spriteBatch.End();
            */


            if (this.selectedWarrior != null)
            {
                displayWarrior = new Warrior(this.selectedWarrior);
                int imgPadding = 40;
                toDrawX = SELECTED_WARRIOR_INFO_X;
                toDrawY = SELECTED_WARRIOR_INFO_Y + 80;
                displayWarrior.drawInArbitraryLocation(toDrawX, toDrawY);
                displayWarrior.drawWarriorType(toDrawX + board.WARRIORWIDTH / 2 + imgPadding, toDrawY - imgPadding);
                this.spriteBatch.Begin();
                toDrawX = toDrawX + board.WARRIORWIDTH / 2 + imgPadding;
                int xoffset = (int) iconHeight * 2;
                padding = (int) iconHeight * 2;

                // Draw the warriors health
                this.spriteBatch.Draw(heartIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.Draw(Game1.charcoal, new Rectangle(xoffset + toDrawX - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * selectedWarrior.maxHealth) + 4, barHeight + 4), Color.WhiteSmoke);
                this.spriteBatch.Draw(white, new Rectangle(xoffset + toDrawX, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.maxHealth), barHeight), Color.WhiteSmoke);
                this.spriteBatch.Draw(red, new Rectangle(xoffset + toDrawX, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.health), barHeight), Color.WhiteSmoke);
                toDrawY += (int) ((double) iconHeight * 1.5);

                // Draw the warriors attack strength
                this.spriteBatch.Draw(attackIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.Draw(Game1.charcoal, new Rectangle(xoffset + toDrawX - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
                this.spriteBatch.Draw(white, new Rectangle(xoffset + toDrawX, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
                this.spriteBatch.Draw(red, new Rectangle(xoffset + toDrawX, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.attack), barHeight), Color.WhiteSmoke);
                toDrawY += (int)((double)iconHeight * 1.5);

                // Draw the warriors defense strength
                // @TODO: Change the 2+ on this to be a resolution-independent value
                this.spriteBatch.Draw(shieldIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.Draw(Game1.charcoal, new Rectangle(xoffset + toDrawX - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
                this.spriteBatch.Draw(white, new Rectangle(xoffset + toDrawX, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
                this.spriteBatch.Draw(red, new Rectangle(xoffset + toDrawX, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.defense), barHeight), Color.WhiteSmoke);
                toDrawY += (int)((double)iconHeight * 1.5);

                // Draw the warriors cooldown
                string coolDisplay = this.selectedWarrior.maxCooldown.ToString();
                this.spriteBatch.Draw(hourglass, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.DrawString(this.infofont, coolDisplay, new Vector2(xoffset + toDrawX - 2, toDrawY), Color.White);
                toDrawY += (int)((double)iconHeight * 1.5);

                // Draw the warriors bonus
                string baseDisplay = "Base Type: " + this.selectedWarrior.baseType;
                this.spriteBatch.DrawString(this.infofont, baseDisplay, new Vector2(toDrawX, toDrawY), Color.White);
                toDrawY += (int)((double)iconHeight * 1.5);

                string bonusDisplay = "Bonus against " + this.selectedWarrior.bonus;
                this.spriteBatch.DrawString(this.infofont, bonusDisplay, new Vector2(toDrawX, toDrawY), Color.White);


                this.spriteBatch.End();
            }

        
        }

        // returns true if this is a valid move, false otherwise
        // kicks off opponent animation for move
        public Boolean handleOpponentMove(Message message)
        {
            message = this.flipOverXAxis(message);
            if (this.isYourTurn)
            {
                return false;
            }

            Warrior attackingWarrior = this.getWarriorById(message.attackerUnitID);
            Warrior attackedWarrior = this.getWarriorById(message.attackedUnitID);
            attackingWarrior.moveTo(message.endLocation[0], message.endLocation[1]);
            this.turnProgress = TurnProgress.moving;
            this.currentTurnWarrior = attackingWarrior;

            this.targetRow = message.attackedLocation[0];
            this.targetCol = message.attackedLocation[1];

            this.beingAttacked = attackedWarrior;
            this.opponentDamage = message.damageDealt;
            
            return true;
        }

        // modifies message in place and returns it
        public Message flipOverXAxis(Message message)
        {
            // flip everything over x axis
            int[] attackedLocation = message.attackedLocation;
            attackedLocation[0] = this.board.rows - attackedLocation[0] - 1;
            int[] endLocation = message.endLocation;
            endLocation[0] = this.board.rows - endLocation[0] - 1;
            
            return message;
        }

        // looks through board to find warrior
        public Warrior getWarriorById(int id)
        {
            for (int row = 0; row < this.board.rows; row++)
            {
                for (int col = 0; col < this.board.cols; col++)
                {
                    Warrior atRowCol = this.board.warriors[row][col];
                    if (atRowCol !=null && atRowCol.id == id)
                    {
                        return atRowCol;
                    }
                }
            }
            return null;
        }
    }
}
