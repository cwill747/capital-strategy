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
        public int SELECTED_WARRIOR_INFO_X = 650;
        public int SELECTED_WARRIOR_INFO_Y = 60;

        public Texture2D red;
        public Texture2D white;
        public Texture2D heartIcon;
        public Texture2D attackIcon;
        public Texture2D shieldIcon;
        public Texture2D moveIcon;
        public SpriteFont menufont;
        public SpriteFont infofont;
        public Texture2D hourglass;
        public bool hasWindowBeenDrawn = false;
        Rectangle backgroundRec;

        public Board board { get; set; }
        public List<Warrior> opponentWarriors { get; set; }
        public List<Warrior> yourWarriors { get; set; }
        Warrior selectedWarrior;
        Warrior currentTurnWarrior;
        public Boolean isYourTurn { get; set; }
        public Stack<int[,]> p1MovementStack;
        public Stack<int[,]> p2MovementStack;
        private bool warriorIsResetting;
        private int previousWarriorDirection;
        private int healthBarFadeDelay = -1;
        private bool hasMoved = false; // so that attackbtn plays nicely with movement button

        private int endOfInfoPaneLocation;
        public int turnProgress { get; set; }
        public int targetCol { get; set; }
        public int targetRow { get; set; }
        public Warrior beingAttacked { get; set; }
        private Warrior justAttacked { get; set; } // for displaying health bar after warrior is attacked
        Warrior displayWarrior;
        MouseWrapper mouseState;
        MouseState oldMouseState;
        private Texture2D background;
        private Texture2D bars;

        // GUI STUFF
        Button movementBtn;
        Button attackBtn;
        Button skipBtn;

        // fading messages
        FadingMessage missFadingMessage;
        FadingMessage btnMisuseFadingMessage;

        public Boolean waitingForTurn { get; set; }
        public Boolean didSkipTurn = false;
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
                this.yourWarriors = this.board.loadWarriors(this.windowManager, true);
                this.opponentWarriors = this.board.loadWarriors(this.windowManager, false);
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




            red = Content.Load<Texture2D>("colors/red");
            white = Content.Load<Texture2D>("colors/white");
            heartIcon = Content.Load<Texture2D>("icons/heartIcon");
            attackIcon = Content.Load<Texture2D>("icons/attackIcon");
            shieldIcon = Content.Load<Texture2D>("icons/shieldIcon");
            moveIcon = Content.Load<Texture2D>("icons/moveIcon");
            hourglass = Content.Load<Texture2D>("icons/hourglass");
            this.bars = this.windowManager.Content.Load<Texture2D>("GUI/bars");

            this.background = this.windowManager.Content.Load<Texture2D>("GUI/gamematch_background");

            backgroundRec = new Rectangle(0, 0, windowManager.Window.ClientBounds.Width, windowManager.Window.ClientBounds.Height);
            
            board = new Board(ROWS, COLS, new Rectangle(0, 25, this.BOARDWIDTH, this.BOARDHEIGHT), Game1.tileImage);
            // Game1 game, int maxHealth, int attack, int defense, int accuracy, int evade, int maxMove, double speed, String type, int[] imageDimensions, int[] stateDurations, Point[] attackPoints, int? attackRange, int attackDelayConst, int attackDelayRate)

            XmlTextReader reader = new XmlTextReader("Configuration/WarriorTypes.xml");


            int btn_Y = 250;
            movementBtn = new Button("MOVEMENT", new Rectangle(SELECTED_WARRIOR_INFO_X, btn_Y, 100, 25), Game1.smallFont);
            attackBtn = new Button("ATTACK", new Rectangle(movementBtn.location.X + movementBtn.location.Width, btn_Y, 100, 25), Game1.smallFont);
            skipBtn = new Button("SKIP", new Rectangle(attackBtn.location.X + attackBtn.location.Width, btn_Y, 100, 25), Game1.smallFont);


            mouseState = new MouseWrapper(board, Mouse.GetState());

            this.missFadingMessage = new FadingMessage(0, 0, "Miss!", Game1.menuFont, 2000, Color.White);
            this.btnMisuseFadingMessage = new FadingMessage(attackBtn.location.X + attackBtn.location.Width / 2, btn_Y - 20, "You must select a warrior first.", Game1.smallFont, 2000, Color.Red);
            
        }
        public void Update(GameTime gameTime)
        {
            this.healthBarFadeDelay -= gameTime.ElapsedGameTime.Milliseconds;
            this.missFadingMessage.update(gameTime);
            this.btnMisuseFadingMessage.update(gameTime);
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
                if (this.isYourTurn)
                {
                    switch (this.turnProgress)
                    {
                        case TurnProgress.beginning:
                            this.hasMoved = false;
                            movementBtn.isDisabled = true;
                            attackBtn.isDisabled = false;
                            skipBtn.isDisabled = false;
                            break;
                        case TurnProgress.moving:
                            this.hasMoved = true;
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
                }
                else
                {
                    movementBtn.isDisabled = true;
                    attackBtn.isDisabled = true;
                    skipBtn.isDisabled = true;
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
           
                    if (!this.isYourTurn)
                    {
                        Message toSend;
                        if (this.didSkipTurn)
                        {
                            toSend = new Message(msgType.Move, this.windowManager.client.UniqueIdentifier, this.windowManager.otherPlayer.uniqueIdentifier,
                            new int[2] { 0,0 },
                            new int[2] { 0,0 },
                            0,
                            0,
                            int.MaxValue,
                            false,
                            0);
                            this.didSkipTurn = false;
                            this.windowManager.msgManager.addToOutgoingQueue(toSend);
                        }
                    }
                    // end of end game
                
            }


            if (this.currentTurnWarrior != null)
            {

                if (this.turnProgress == TurnProgress.moving)
                {
                    Boolean finishedMoving = currentTurnWarrior.move(gameTime);
                    if (finishedMoving && !warriorIsResetting)
                    {
                        this.turnProgress = TurnProgress.moved;
                        
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
                    if (!this.isYourTurn)
                    {
                        if (this.targetRow < 0 || beingAttacked == null)
                        {
                            // todo either make you unable to attack nothing or correct facing
                            this.turnProgress = TurnProgress.turnOver;
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
                    else
                    {
                        this.currentTurnWarrior.drawAttackRange();
                    }
                }
                if (this.turnProgress == TurnProgress.targetAcquired)
                {
                    this.board.tileTints[this.targetRow][this.targetCol] = Warrior.targetAcquiredColor;
                }
                if (this.turnProgress == TurnProgress.attacked)
                {

                    
                    
                    // calculate damage
                    this.justAttacked = this.beingAttacked;
                    this.healthBarFadeDelay = 1500;
                    if (this.beingAttacked != null)
                    {

                        int targetHealthCheck = this.beingAttacked.health;
                        if (this.isYourTurn)
                        {
                            this.opponentDamage = this.currentTurnWarrior.strike(this.beingAttacked);
                            if (this.opponentDamage == 0)
                            {
                                Vector2 warriorLoc = this.board.getLocation(this.currentTurnWarrior.row, this.currentTurnWarrior.col);
                                this.missFadingMessage.moveTo(warriorLoc.X + this.BOARDWIDTH / this.board.cols / 2, warriorLoc.Y - this.BOARDHEIGHT / this.board.rows / 4);
                                this.missFadingMessage.show();
                            }
                        }
                        else
                        {
                            this.beingAttacked.health -= this.opponentDamage;
                            if (this.opponentDamage == 0)
                            {
                                Vector2 warriorLoc = this.board.getLocation(this.currentTurnWarrior.row, this.currentTurnWarrior.col);
                                this.missFadingMessage.moveTo(warriorLoc.X + this.BOARDWIDTH / this.board.cols / 2, warriorLoc.Y - this.BOARDHEIGHT / this.board.rows / 4);
                                this.missFadingMessage.show();
                            }
                            if (this.beingAttacked.health > this.beingAttacked.maxHealth)
                            {
                                this.beingAttacked.health = this.beingAttacked.maxHealth;
                            }
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
                    this.selectedWarrior = null;
                    if (!this.isYourTurn)
                    {
                        Message toSend;
                        if (this.didSkipTurn)
                        {
                            toSend = new Message(msgType.Move, this.windowManager.client.UniqueIdentifier, this.windowManager.otherPlayer.uniqueIdentifier,
                            new int[2] { 0,0 },
                            new int[2] { 0,0 },
                            0,
                            0,
                            this.currentTurnWarrior.id,
                            false,
                            this.currentTurnWarrior.direction);
                            this.didSkipTurn = false;
                        }
                        else{
                            toSend = new Message(msgType.Move, this.windowManager.client.UniqueIdentifier, this.windowManager.otherPlayer.uniqueIdentifier,
                            new int[2] { (int)this.currentTurnWarrior.row, (int)this.currentTurnWarrior.col},
                            new int[2] { this.targetRow, this.targetCol },
                            0,
                            0,
                            this.currentTurnWarrior.id,
                            false,
                            0);
                        }


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
                        /*NetOutgoingMessage om = this.windowManager.client.CreateMessage();
                        toSend.handleMoveMessage(ref om);
                        this.windowManager.client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
                        this.currentTurnWarrior = null;
                        this.beingAttacked = null;*/
                        this.windowManager.msgManager.addToOutgoingQueue(toSend);
                    }
                    this.beingAttacked = null;


                    
                }
            }

            // handle skip


            mouseState.update(Mouse.GetState());

            if (mouseState.wasClicked() && mouseState.isOverGrid)
            {
                this.handleClickOverGrid();
                oldMouseState = mouseState.mouseState;

            }
            if (!mouseState.Equals(oldMouseState))
            {


                if (mouseState.mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (this.movementBtn.checkClick(mouseState.mouseState))
                    {

                    }
                    if (this.attackBtn.checkClick(mouseState.mouseState))
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
                        if (this.isYourTurn && this.hasMoved)
                        {
                            int[,] lastMove = p1MovementStack.Pop();
                            int[] movedFrom = { lastMove[0, 0], lastMove[0, 1] };
                            int[] movedTo = { lastMove[1, 0], lastMove[1, 1] };
                            this.selectedWarrior = this.board.warriors[movedTo[0]][movedTo[1]];
                            if (this.selectedWarrior.moveTo(movedFrom[0], movedFrom[1]))
                            {
                                this.turnProgress = TurnProgress.moving;
                                warriorIsResetting = true;
                            }
                            else
                            {
                                this.turnProgress = TurnProgress.moved;
                                warriorIsResetting = false;
                            }
                            
                            board.resetTints();
                            this.currentTurnWarrior = selectedWarrior;
                            
                            this.previousWarriorDirection = lastMove[2, 0];
                            //this.currentTurnWarrior.state = State.
                        }
                        else
                        {
                            this.board.resetTints();
                            this.currentTurnWarrior.updateUserOptions(true);
                        }
                    }

                    if (this.skipBtn.unClick(mouseState.mouseState))
                    {
                        if (this.isYourTurn)
                        {
                            this.turnProgress = TurnProgress.turnOver;
                            this.beingAttacked = null;
                            board.resetTints();
                        }
                    }
                    if (this.attackBtn.unClick(mouseState.mouseState))
                    {
                        if (this.selectedWarrior == null || !this.selectedWarrior.isYours)
                        {
                            this.btnMisuseFadingMessage.message = "You must select a unit on your team.";
                            this.btnMisuseFadingMessage.show();
                        }
                        else
                        {
                            if (this.selectedWarrior.cooldown > 0)
                            {
                                this.btnMisuseFadingMessage.message = "Selected unit cannot be on cooldown.";
                                this.btnMisuseFadingMessage.show();
                            }
                            else
                            {
                                this.hasMoved = false;
                                this.turnProgress = TurnProgress.moved;
                                this.currentTurnWarrior = this.selectedWarrior;
                                board.resetTints();
                            }
                        }
                    }

                }
            }
                else
                {
                    if (mouseState.mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (this.skipBtn.unClick(mouseState.mouseState))
                        {
                            if (this.isYourTurn)
                            {
                                this.turnProgress = TurnProgress.turnOver;
                                if(this.currentTurnWarrior != null)
                                    this.currentTurnWarrior.updateUserOptions(false);
                                board.resetTints();
                                this.isYourTurn = false;
                                this.didSkipTurn = true;
                            }
                        }
                    }
                }
            oldMouseState = mouseState.mouseState;

            


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

            if (this.hasWindowBeenDrawn == false)
            {
                SoundEffect song;
                song = Content.Load<SoundEffect>("Music/intoBattle");
                SoundEffectInstance instance = song.CreateInstance();
                instance.IsLooped = true;
                instance.Play();
                this.hasWindowBeenDrawn = true;
            }
            foreach (Warrior w in yourWarriors)
            {
                this.board.warriors[(int)w.row][(int)w.col] = w;
            }
            foreach (Warrior w in opponentWarriors)
            {
                this.board.warriors[(int)w.row][(int)w.col] = w;
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
            this.windowManager.spriteBatch.Draw(this.background, Vector2.Zero, Color.White);

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
                                   
                                    //draw the health of the warrior being attacked
                                    if (warrior == beingAttacked && (this.turnProgress == TurnProgress.targetAcquired 
                                        ||this.turnProgress == TurnProgress.attacking|| this.turnProgress == TurnProgress.attacked))
                                    {
                                        int healthBarY = yLoc - tileHeight / 2; // most warriors are taller than the tile
                                        double widthPerHP = ((double)tileWidth) / 100;
                                        this.spriteBatch.Draw(Game1.charcoal, new Rectangle(xLoc - 2, healthBarY - 2, (int)(widthPerHP * warrior.maxHealth) + 4, tileHeight / 10 + 4), Color.WhiteSmoke);
                                        this.spriteBatch.Draw(white, new Rectangle(xLoc, healthBarY, (int)(widthPerHP * warrior.maxHealth), tileHeight / 10), Color.WhiteSmoke);
                                        this.spriteBatch.Draw(red, new Rectangle(xLoc, healthBarY, (int)(widthPerHP * warrior.health), tileHeight / 10), Color.WhiteSmoke);
                                    }
                                }
                            }
                       
                    }
                



            spriteBatch.End();
            board.drawTiles(spriteBatch);
            foreach (Warrior w in this.yourWarriors)
            {
                w.draw();
                this.drawCooldownOnWarrior(w);
            }
            foreach (Warrior w in this.opponentWarriors)
            {
                w.draw();
                this.drawCooldownOnWarrior(w);
            }
            this.missFadingMessage.draw(spriteBatch);
            this.btnMisuseFadingMessage.draw(spriteBatch);
            if (mouseState.isOverGrid && board.warriors[mouseState.row][mouseState.col] != null)
            {
                this.drawHealthBar(mouseState.row, mouseState.col);
            }
            if (this.beingAttacked != null)
            {
                this.drawHealthBar((int)beingAttacked.row, (int)beingAttacked.col);
            }
            else if (this.justAttacked != null && healthBarFadeDelay > 0)
            {
                this.drawHealthBar((int)this.justAttacked.row, (int)this.justAttacked.col);
            }
            this.drawInfoFrame(this.selectedWarrior);
            this.spriteBatch.Begin();
            string turnInfo = (this.isYourTurn) ? "It is your turn." : "Waiting for " + this.windowManager.otherPlayer.username + ".";
            //+ this.windowManager.otherPlayer.username
            this.spriteBatch.DrawString(Game1.smallFont, turnInfo, new Vector2(SELECTED_WARRIOR_INFO_X, SELECTED_WARRIOR_INFO_Y), Color.Brown);


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
                        if (selectedWarrior.moveTo(mouseState.row, mouseState.col))
                        {
                            this.turnProgress = TurnProgress.moving;
                        }
                        else
                        {
                            this.turnProgress = TurnProgress.moved;
                        }
                        
                        if (this.isYourTurn)
                        {
                            p1MovementStack.Push(new int[,] { { (int)selectedWarrior.row, (int)selectedWarrior.col }, { mouseState.row, mouseState.col }, {selectedWarrior.direction, 0}});
                        }
                        else if (!this.isYourTurn)
                        {
                            p2MovementStack.Push(new int[,] { { (int)selectedWarrior.row, (int)selectedWarrior.col }, { mouseState.row, mouseState.col }, { selectedWarrior.direction, 0 } });
                        }
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

                        
                        beingAttacked.takeHit(currentTurnWarrior.getAttackDelay(xDiff, yDiff));

                        string attackSound = currentTurnWarrior.attackSound;
                        SoundEffect effect;
                        effect = Content.Load<SoundEffect>(attackSound);
                        effect.Play();
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

            
            this.spriteBatch.End();
            return healthBarY + tileHeight / 10;
        }
        public void drawCooldownOnWarrior(Warrior warrior)
        {
            if (warrior.cooldown != 0)
            {
                //draw cooldown
                SpriteFont font = Game1.smallFont;
                this.spriteBatch.Begin();
                Vector2 warriorLoc = this.board.getLocation(warrior.row, warrior.col);
                int iconHeight = (int)Game1.smallFont.MeasureString("1").Y;
                int iconWidth = iconHeight;
                int totalWidth = (int)(iconWidth + font.MeasureString(warrior.cooldown.ToString()).X);
                int tileWidth = this.BOARDWIDTH / this.board.cols;
                int offsetX = (tileWidth - totalWidth) / 2;
                this.spriteBatch.Draw(hourglass, new Rectangle((int)warriorLoc.X + offsetX, (int)warriorLoc.Y, iconWidth, iconHeight), Color.White);
                this.spriteBatch.DrawString(font, warrior.cooldown.ToString(), new Vector2(warriorLoc.X + iconWidth + offsetX, warriorLoc.Y), Color.White);
                this.spriteBatch.End();
            }
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
                displayWarrior.drawInArbitraryLocation(645, 318);
                toDrawX = toDrawX + board.WARRIORWIDTH / 2 + imgPadding;
                int xoffset = (int) iconHeight * 2;
                padding = (int) iconHeight * 2;

                this.windowManager.spriteBatch.Begin();

                int barStart = 739;
                int barYStart = 310;

                string selwarrior = this.selectedWarrior.type.ToUpperInvariant();
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, selwarrior,
                    new Vector2(barStart, barYStart - 20),
                    Color.Brown, 0, Vector2.Zero, .9f, SpriteEffects.None, 1f
                    );

                int frameWidth = 9;
                int frameHeight = 18;




                // Draw attack strength bars
                if (this.selectedWarrior.attack >= 10)
                {
                    Rectangle source = new Rectangle(0, 0, frameWidth, frameHeight);
                    this.windowManager.spriteBatch.Draw(bars, new Vector2(barStart, barYStart), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                if (Math.Abs(this.selectedWarrior.attack) > 10 && Math.Abs(this.selectedWarrior.attack) <= 100)
                {
                    int total10Blocks = (int)(this.selectedWarrior.attack / 10);
                    for (int i = 1; i < total10Blocks - 1; i++)
                    {
                        Rectangle source = new Rectangle(frameWidth + 1, 0, frameWidth * 2 + 1, frameHeight);
                        this.windowManager.spriteBatch.Draw(bars, new Vector2(barStart + 10 * i, barYStart), source, Color.White);
                    }
                }

                string warriorAttack = this.selectedWarrior.attack.ToString() + "/100";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, warriorAttack,
                    new Vector2(barStart + 30, barYStart),
                    Color.Yellow, 0, Vector2.Zero, .7f, SpriteEffects.None, 1f
                    );

                string attackExplanation = "ATTACK";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, attackExplanation,
                    new Vector2(880, barYStart),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );

                // Draw defensive bars
                if (this.selectedWarrior.defense >= 10)
                {
                    Rectangle source = new Rectangle(0, 19, frameWidth, frameHeight);
                    this.windowManager.spriteBatch.Draw(bars, new Vector2(barStart, 334), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                if (Math.Abs(this.selectedWarrior.defense) > 10 && Math.Abs(this.selectedWarrior.defense) <= 100)
                {
                    int total10Blocks = (int)(this.selectedWarrior.defense / 10);
                    for (int i = 1; i < total10Blocks - 1; i++)
                    {
                        Rectangle source = new Rectangle(frameWidth + 1, 19, frameWidth * 2 + 1, frameHeight);
                        this.windowManager.spriteBatch.Draw(bars, new Vector2(barStart + 10 * i, 334), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }

                string warriorDefense = this.selectedWarrior.defense.ToString() + "/100";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, warriorDefense,
                    new Vector2(barStart + 30, 334),
                    Color.Yellow, 0, Vector2.Zero, .7f, SpriteEffects.None, 1f
                    );

                string defenseExplanation = "DEFENSE";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, defenseExplanation,
                    new Vector2(880, 334),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );

                // Draw the warriors cooldown
                if (this.selectedWarrior.maxCooldown >= 1)
                {
                    Rectangle source = new Rectangle(0, 19 * 2, frameWidth, frameHeight);
                    this.windowManager.spriteBatch.Draw(bars, new Vector2(barStart, 358), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                if (this.selectedWarrior.maxCooldown > 1)
                {
                    for (int i = 1; i < this.selectedWarrior.maxCooldown; i++)
                    {
                        Rectangle source = new Rectangle(frameWidth, 19 * 2, frameWidth, frameHeight);
                        this.windowManager.spriteBatch.Draw(bars, new Vector2(barStart + 10 * i, 358), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        toDrawX = barStart + 10 * i;
                        toDrawY = 358;
                    }
                }

                string warriorCooldown = this.selectedWarrior.maxCooldown.ToString();
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, warriorCooldown,
                    new Vector2(barStart + 45, 358),
                    Color.Yellow, 0, Vector2.Zero, .7f, SpriteEffects.None, 1f
                    );

                string cooldownExplanation = "COOLDOWN";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, cooldownExplanation,
                    new Vector2(880, 358),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );



                toDrawX += 20;
                toDrawY += 30;
                // Draw the warriors bonus
                string baseDisplay = "Class: " + this.selectedWarrior.warriorClass.warriorClassName;
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, baseDisplay, new Vector2(barStart, toDrawY), Color.Brown);
                toDrawY += (int)((double)iconHeight * 1.5);

                string bonusDisplay = "Bonus against: " + this.windowManager.warriorClasses[this.selectedWarrior.warriorClass.indexOfAdvantageAgainst].warriorClassName;
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, bonusDisplay, new Vector2(barStart, toDrawY), Color.Brown);

                string cooldownExplained = "Cooldown is how many turns a warrior must wait before they can take any action after completing a turn. ";
                List<String> splitCooldownExplained = StringHelper.SplitString(cooldownExplained, 40);
                cooldownExplained = string.Join("\n", splitCooldownExplained);
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, cooldownExplained,
                    new Vector2(640, toDrawY + 30),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );

                this.windowManager.spriteBatch.End();
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

            // they skipped their turn
            if(message.attackerUnitID == int.MaxValue)
            {
                this.turnProgress = TurnProgress.moving;
            }
            else
            {
                Warrior attackingWarrior = this.getWarriorById(message.attackerUnitID);
                Warrior attackedWarrior = this.getWarriorById(message.attackedUnitID);
                if (attackingWarrior.moveTo(message.endLocation[0], message.endLocation[1]))
                {
                    this.turnProgress = TurnProgress.moving;
                }
                else
                {
                    this.turnProgress = TurnProgress.moved;
                }
                
                this.currentTurnWarrior = attackingWarrior;

                this.targetRow = message.attackedLocation[0];
                this.targetCol = message.attackedLocation[1];




                this.beingAttacked = attackedWarrior;
                this.opponentDamage = message.damageDealt;

                if (this.opponentDamage != 0)
                {
                    string attackSound = this.currentTurnWarrior.attackSound;
                    SoundEffect effect;
                    effect = Content.Load<SoundEffect>(attackSound);
                    effect.Play();
                }
            }

            
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
