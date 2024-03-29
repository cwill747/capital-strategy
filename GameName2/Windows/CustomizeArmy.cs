﻿using System;
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using MySql.Data.MySqlClient;
using CapitalStrategy.GUI;
using CapitalStrategy.Primitives;


namespace CapitalStrategy.Windows
{
    class CustomizeArmy : Window
    {
        public Game1 windowManager { get; set; }
        public BackButton backButton { get; set; }
        public MouseState oldMouseState { get; set; }
        public Board board { get; set; }
        public Warrior currentWarrior { get; set; }
        public Button save { get; set; }
        public List<Warrior> warriors { get; set; }
        public Texture2D red;
        public Texture2D white;
        public Texture2D heartIcon;
        public Texture2D attackIcon;
        public Texture2D shieldIcon;
        public Texture2D moveIcon;
        public Texture2D welcomeBackground;
        public Texture2D infoBoxBackground;
        public SpriteFont menufont;
        public SpriteFont infofont;
        public SpriteFont smallfont;
        public Texture2D hourglass;
        private Texture2D arrowDown;
        private Texture2D background;
        private Texture2D bars;
        private Rectangle pageContent = new Rectangle();
        private int boardHeight;
        private int boardWidth;
        private int tileWidth;
        private int tileHeight;
        public Boolean recentlySaved { get; set; }
        public int saveButtonHeight { get; set; }
        public int saveButtonWidth { get; set; }
        public Button aRange { get; set; }
        public Button mRange { get; set; }
        private PrimitiveBatch primitiveBatch;
        private bool isMousedOver = false;
        private long nextCheckMouseover = 0L;
        MouseWrapper mouseState;
        Boolean warriorIsPickedUp { get; set; }
        Vector2 offsetPickedUpWarrior { get; set; }

        public SoundEffect click;

        public CustomizeArmy(Game1 windowManager)
        {
            this.windowManager = windowManager;

        }

        public void Initialize()
        {
            this.pageContent.X = 70;
            this.pageContent.Y = 50;
            boardHeight = 450;
            this.pageContent.Height = boardHeight;
            boardWidth = 500;
            this.pageContent.Width = boardWidth;
            board = new Board(10, 10, new Rectangle(this.pageContent.X, this.pageContent.Y, boardWidth, boardHeight), Game1.tileImage);
            this.oldMouseState = new MouseState();
            this.warriors = this.board.loadWarriors(this.windowManager, true);

            saveButtonWidth = 100;
            this.saveButtonHeight = 50;
            int padding = 10;
            this.save = new Button("SAVE", new Rectangle(this.pageContent.X + this.pageContent.Width - padding - saveButtonWidth,
                this.pageContent.Y + this.pageContent.Height + padding, saveButtonWidth, saveButtonHeight), Game1.smallFont, isDisabled: true);
            tileWidth = this.boardWidth / this.board.cols;
            tileHeight = this.boardHeight / this.board.rows;
            this.pageContent.Height = this.pageContent.Height + padding + saveButtonHeight;
            this.recentlySaved = false;
            this.aRange = new Button("Attack", new Rectangle(this.pageContent.X + this.pageContent.Width - padding - (2*saveButtonWidth),
                this.pageContent.Y + this.pageContent.Height + padding, saveButtonWidth, saveButtonHeight), Game1.smallFont, isDisabled: false);
            this.mRange = new Button("Movement", new Rectangle(this.pageContent.X + this.pageContent.Width - padding - (3 * saveButtonWidth),
                this.pageContent.Y + this.pageContent.Height + padding, saveButtonWidth, saveButtonHeight), Game1.smallFont, isDisabled: true);
            this.warriorIsPickedUp = false;
            this.offsetPickedUpWarrior = Vector2.Zero;
        }

        public void LoadContent()
        {
            this.backButton = new BackButton();
            menufont = this.windowManager.Content.Load<SpriteFont>("fonts/gamefont");
            this.infofont = this.windowManager.Content.Load<SpriteFont>("fonts/menufont");
            heartIcon = this.windowManager.Content.Load<Texture2D>("icons/heartIcon");
            attackIcon = this.windowManager.Content.Load<Texture2D>("icons/attackIcon");
            shieldIcon = this.windowManager.Content.Load<Texture2D>("icons/shieldIcon");
            moveIcon = this.windowManager.Content.Load<Texture2D>("icons/moveIcon");
            red = this.windowManager.Content.Load<Texture2D>("colors/red");
            white = this.windowManager.Content.Load<Texture2D>("colors/white");
            hourglass = this.windowManager.Content.Load<Texture2D>("icons/hourglass");
            mouseState = new MouseWrapper(board, Mouse.GetState());
            smallfont = this.windowManager.Content.Load<SpriteFont>("fonts/smallfont");
            this.arrowDown = this.windowManager.Content.Load<Texture2D>("GUI/customizearrow");
            this.welcomeBackground = this.windowManager.Content.Load<Texture2D>("GUI/welcome_box_background");
            this.infoBoxBackground = this.windowManager.Content.Load<Texture2D>("GUI/info_box_background");
            this.background = this.windowManager.Content.Load<Texture2D>("GUI/customize_army_background");
            this.bars = this.windowManager.Content.Load<Texture2D>("GUI/bars");
            primitiveBatch = new PrimitiveBatch(this.windowManager.GraphicsDevice);

            click = windowManager.Content.Load<SoundEffect>("soundEffects/daClick");

        }

        public void Update(GameTime gameTime)
        {
            isMousedOver = false;
            this.save.update(gameTime);
            MouseState newMouseState = Mouse.GetState();
            if (!newMouseState.Equals(this.oldMouseState))
            {
                if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
                {
                    if(this.backButton.checkClick(newMouseState))
                        click.Play();
                    if(this.save.checkClick(newMouseState))
                        click.Play();
                    if(this.aRange.checkClick(newMouseState))
                        click.Play();
                    if(this.mRange.checkClick(newMouseState))
                        click.Play();
                    if (board.isClickOverGrid(newMouseState.X, newMouseState.Y))
                    {
                        Vector2 coord = board.clickOverGrid(newMouseState.X, newMouseState.Y);
                        currentWarrior = board.warriors[(int)coord.X][(int)coord.Y];
                        board.warriors[(int)coord.X][(int)coord.Y] = null;
                        if (currentWarrior != null)
                        {
                            Vector2 oldWarriorXY = this.board.getLocation(this.currentWarrior.row, this.currentWarrior.col);
                            this.currentWarrior.x = oldWarriorXY.X;
                            this.currentWarrior.y = oldWarriorXY.Y;
                            this.offsetPickedUpWarrior = new Vector2(newMouseState.X - currentWarrior.x, newMouseState.Y - currentWarrior.y);
                            this.warriorIsPickedUp = true;
                        }
                    }
                }
                else if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    // update current warrior location
                    if (currentWarrior != null)
                    {
                        currentWarrior.x = newMouseState.X - this.offsetPickedUpWarrior.X;
                        currentWarrior.y = newMouseState.Y - this.offsetPickedUpWarrior.Y;
                    }
                }
                if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton != ButtonState.Released)
                {
                    if (backButton.unClick(newMouseState))
                    {
                        // this should be moved into backbutton class soon
                        int newGameState = Game1.gameStates.Pop();
                        this.windowManager.gameState = newGameState;
                        this.windowManager.windows[newGameState].Initialize();
                    }
                    if (this.save.unClick(newMouseState))
                    {
                        this.saveArmy();
                        this.recentlySaved = true;
                    }
                    if (this.aRange.unClick(newMouseState))
                    {
                        if (!this.aRange.isDisabled)
                        {
                            this.aRange.isDisabled = true;
                            this.mRange.isDisabled = false;
                        }
                    }
                    if (this.mRange.unClick(newMouseState))
                    {
                        if (!this.mRange.isDisabled)
                        {
                            this.aRange.isDisabled = false;
                            this.mRange.isDisabled = true;
                        }
                    }
                    if (currentWarrior != null)
                    {
                        this.warriorIsPickedUp = false;
                        Vector2 rowCol = board.clickOverGrid(newMouseState.X, newMouseState.Y);
                        int row = (int)rowCol.X;
                        int col = (int)rowCol.Y;
                        if (board.isClickOverGrid(newMouseState.X, newMouseState.Y) && board.warriors[row][col] == null && row >= 5)
                        {
                            currentWarrior.row = row;
                            currentWarrior.col = col;
                            board.warriors[row][col] = currentWarrior;
                            Vector2 loc = this.board.getLocation(row, col);
                            this.currentWarrior.x = loc.X;
                            this.currentWarrior.y = loc.Y;
                        }
                        else
                        {
                            board.warriors[(int)currentWarrior.row][(int)currentWarrior.col] = currentWarrior;
                            Vector2 loc = this.board.getLocation(currentWarrior.row, (int)currentWarrior.col);
                            this.currentWarrior.x = loc.X;
                            this.currentWarrior.y = loc.Y;
                        }
                        currentWarrior = null;
                        this.save.isDisabled = false;
                        this.recentlySaved = false;
                    }

                }
            }
            else
            {
                isMousedOver = false;

                if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Released)
                {
                    // Lets check if we're moused over a warrior
                    if(board.isClickOverGrid(newMouseState.X, newMouseState.Y))
                    {
                        Vector2 coord = board.clickOverGrid(newMouseState.X, newMouseState.Y);
                        currentWarrior = board.warriors[(int)coord.X][(int)coord.Y];
                        if (currentWarrior != null)
                        {
                            isMousedOver = true;
                        }
                        else if (currentWarrior == null)
                        {
                            isMousedOver = false;
                        }
                    }
                    else
                    {
                        isMousedOver = false;
                    }
                }
            }
            this.oldMouseState = newMouseState;
        }


        public void Draw()
        {
            this.windowManager.spriteBatch.Begin();
            windowManager.spriteBatch.Draw(Game1.background, new Rectangle(0, 0, this.windowManager.Window.ClientBounds.Width, this.windowManager.Window.ClientBounds.Height), Color.White);
            this.windowManager.spriteBatch.Draw(this.background, Vector2.Zero, Color.White);
            this.windowManager.spriteBatch.End();
            this.board.drawTiles(this.windowManager.spriteBatch);

            primitiveBatch.Begin(PrimitiveType.LineList);
            primitiveBatch.AddVertex(
                new Vector2(this.pageContent.X, this.pageContent.Y + boardHeight / 2), Color.Red);
            primitiveBatch.AddVertex(
                new Vector2(this.pageContent.X + boardWidth, this.pageContent.Y + boardHeight / 2), Color.Red);
            primitiveBatch.End();
            this.windowManager.spriteBatch.Begin();
            string label = "The enemies will appear on the top half of the board.\nYour army goes below the line";
            Vector2 labelDim = Game1.smallFont.MeasureString(label);
            // X location + (the width of the location - the width of the string) / 2
            float x = this.pageContent.X + (boardWidth / 10) * 1 - (this.windowManager.GraphicsDevice.Viewport.Width - this.pageContent.Width - labelDim.X) / 4;
            float y = this.pageContent.Y + (boardHeight / 10) * 2 - (20 - labelDim.Y) / 2;
            this.windowManager.spriteBatch.DrawString(Game1.smallFont, label,
                new Vector2(x, y),
                Color.Yellow, 0, Vector2.Zero, .9f, SpriteEffects.None, 1f
                );


            this.windowManager.spriteBatch.Draw(this.arrowDown, 
                new Vector2(this.pageContent.X + (boardWidth / 10) * 2.5f, this.pageContent.Y + (boardHeight / 2) - this.arrowDown.Height), Color.White);
            this.windowManager.spriteBatch.Draw(this.arrowDown,
                new Vector2(this.pageContent.X + (boardWidth / 10) * 5f, this.pageContent.Y + (boardHeight / 2) - this.arrowDown.Height), Color.White);
            this.windowManager.spriteBatch.Draw(this.arrowDown,
                new Vector2(this.pageContent.X + (boardWidth / 10) * 7.5f, this.pageContent.Y + (boardHeight / 2) - this.arrowDown.Height), Color.White);


            //this.windowManager.spriteBatch.Draw(this.welcomeBackground,
            //new Vector2(this.pageContent.X + this.boardWidth + 15, this.pageContent.Y), Color.White);

            //this.windowManager.spriteBatch.Draw(this.infoBoxBackground,
            //new Vector2(this.pageContent.X, this.pageContent.Y), Color.White);

            string welcomeString1 = "Welcome to the custom warrior page. This is " +
            "where you set up your army for battle. Drag " +
            "your warriors to set them up for battle, " +
            "mouse over a warrior for more information " +
            "about that warrior. Click save to save your " +
            "configuration for battle. ";



            this.windowManager.spriteBatch.End();
            for (int row = 0; row < board.warriors.Length; row++)
            {
                for (int col = 0; col < board.warriors[row].Length; col++)
                {

                    Warrior warrior = board.warriors[row][col];
                    //System.Diagnostics.Debug.WriteLine("draw");
                    if (warrior != null)
                    {
                       
                        warrior.draw();
                    }
                }
            }




            board.resetTints();
            

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    board.tileTints[i][j] = Color.DarkGray;
                }
            }

            if (currentWarrior != null)
            {
                if (this.warriorIsPickedUp)
                {
                    currentWarrior.drawToLocation();
                }
               // if (aRange.isDisabled && !this.warriorIsPickedUp)
                if (aRange.isDisabled)
                {
                    //currentWarrior.drawAttackRange(true);

                    Vector2 currentLocation = board.clickOverGrid(this.oldMouseState.X, this.oldMouseState.Y);
                    int attackR = (int)currentWarrior.attackRange;
                    Boolean[][] discovered = currentWarrior.bredthFirst((int)currentLocation.X, (int)currentLocation.Y, attackR, passThroughTeam: true);
                    for (int i = 0; i < discovered.Length; i++)
                    {
                        for (int j = 0; j < discovered[i].Length; j++)
                        {
                            if (discovered[i][j] && this.board.warriors[i][j] == null)
                            {
                                    board.tileTints[i][j] = i >= 5 ? Warrior.attackColor : Color.DarkRed;
                            }
                        }

                    }
                }
                else
                {
                    // get warriors current location
                    Vector2 currentLocation = board.clickOverGrid(this.oldMouseState.X, this.oldMouseState.Y);
                    Boolean[][] discovered = currentWarrior.bredthFirst((int)currentLocation.X, (int)currentLocation.Y, currentWarrior.maxMove, passThroughTeam: true);
                    for (int i = 0; i < discovered.Length; i++)
                    {
                        for (int j = 0; j < discovered[i].Length; j++)
                        {
                            if (discovered[i][j] && this.board.warriors[i][j] == null)
                            {
                                board.tileTints[i][j] = i >= 5 ? Warrior.yourMoveColor : Color.DarkSlateBlue;
                            }
                        }

                    }
                }

                
            }

            this.aRange.draw(this.windowManager.spriteBatch);
            this.mRange.draw(this.windowManager.spriteBatch);
            this.backButton.drawBackButton(this.windowManager.spriteBatch);

            if (recentlySaved)
            {
                /*
                this.windowManager.spriteBatch.Begin();
                this.windowManager.spriteBatch.DrawString(this.infofont, "Successfully Saved!", new Vector2(this.pageContent.X + this.pageContent.Width,
                this.pageContent.Y + this.pageContent.Height - this.saveButtonHeight), Color.Chocolate);
                this.windowManager.spriteBatch.End();
                 */
                this.save.label = "Saved!";
            }
            else{
                this.save.label = "SAVE";
            }
            this.save.draw(this.windowManager.spriteBatch);
            int padding2 = 10;
            int padding3 = 10;
            this.windowManager.spriteBatch.Begin();
            String rangeDisplay;
            if (!aRange.isDisabled)
            {
                rangeDisplay = "Movement ";
                padding2 = 5;
            }
            else
            {
                rangeDisplay = "Attack ";
            }
            this.windowManager.spriteBatch.DrawString(this.smallfont, rangeDisplay+"Range", new Vector2(this.pageContent.X + this.pageContent.Width - padding2 - (3 * saveButtonWidth),
            this.pageContent.Y + this.pageContent.Height - this.saveButtonHeight + padding3), Color.Chocolate);

            this.windowManager.spriteBatch.End();



            if (this.currentWarrior != null)
            {


                Warrior displayWarrior = new Warrior(this.currentWarrior);

                // replace the tutorial string with this warrior's description

                if (this.currentWarrior.description != null)
                {
                    welcomeString1 = this.currentWarrior.description;
                }
                else
                {
                    welcomeString1 = "Description missing.";
                }
                int imgPadding = 20;
                
                int toDrawX = 625;
                int toDrawY = 300;
           
                int iconWidth = tileWidth / 2;
                int iconHeight = iconWidth;
                int padding = 30;
                int barHeight = tileHeight / 10;
                double widthPerPoint = ((double)tileWidth) / 100;

                displayWarrior.drawInArbitraryLocation(toDrawX, toDrawY);

                toDrawX = toDrawX + board.WARRIORWIDTH / 2 + imgPadding;
                toDrawY += iconHeight + padding;


                this.windowManager.spriteBatch.Begin();


                string selwarrior = this.currentWarrior.type.ToUpperInvariant();
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, selwarrior,
                    new Vector2(690, 268),
                    Color.Brown, 0, Vector2.Zero, .9f, SpriteEffects.None, 1f
                    );

                int frameWidth = 9;
                int frameHeight = 18;

                // Draw attack strength bars
                if (this.currentWarrior.attack >= 10)
                {
                    Rectangle source = new Rectangle(0, 0, frameWidth, frameHeight);
                    this.windowManager.spriteBatch.Draw(bars, new Vector2(708, 290), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                if (Math.Abs(this.currentWarrior.attack) > 10 && Math.Abs(this.currentWarrior.attack) <= 100)
                {
                    int total10Blocks = (int) (this.currentWarrior.attack / 10);
                    for(int i = 1; i < total10Blocks - 1; i++)
                    {
                        Rectangle source = new Rectangle(frameWidth + 1, 0, frameWidth * 2 + 1, frameHeight);
                        this.windowManager.spriteBatch.Draw(bars, new Vector2(708 + 10 * i, 290), source, Color.White);
                    }
                }

                string warriorAttack = this.currentWarrior.attack.ToString() + "/100";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, warriorAttack,
                    new Vector2(708 + 30, 290),
                    Color.Yellow, 0, Vector2.Zero, .7f, SpriteEffects.None, 1f
                    );

                string attackExplanation = "ATTACK";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, attackExplanation,
                    new Vector2(850, 290),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );

                // Draw defensive bars
                if (this.currentWarrior.defense >= 10)
                {
                    Rectangle source = new Rectangle(0, 19, frameWidth, frameHeight);
                    this.windowManager.spriteBatch.Draw(bars, new Vector2(708, 312), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                if (Math.Abs(this.currentWarrior.defense) > 10 && Math.Abs(this.currentWarrior.defense) <= 100)
                {
                    int total10Blocks = (int)(this.currentWarrior.defense / 10);
                    for (int i = 1; i < total10Blocks - 1; i++)
                    {
                        Rectangle source = new Rectangle(frameWidth + 1, 19, frameWidth * 2 + 1, frameHeight);
                        this.windowManager.spriteBatch.Draw(bars, new Vector2(708 + 10 * i, 312), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }

                string warriorDefense = this.currentWarrior.defense.ToString() + "/100";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, warriorDefense,
                    new Vector2(708 + 30, 312),
                    Color.Yellow, 0, Vector2.Zero, .7f, SpriteEffects.None, 1f
                    );

                string defenseExplanation = "DEFENSE";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, defenseExplanation,
                    new Vector2(850, 312),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );

                // Draw the warriors cooldown
                if (this.currentWarrior.maxCooldown >= 1)
                {
                    Rectangle source = new Rectangle(0, 19 * 2, frameWidth, frameHeight);
                    this.windowManager.spriteBatch.Draw(bars, new Vector2(708, 336), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                if (this.currentWarrior.maxCooldown > 1)
                {
                    for (int i = 1; i < this.currentWarrior.maxCooldown; i++)
                    {
                        Rectangle source = new Rectangle(frameWidth, 19 * 2, frameWidth, frameHeight);
                        this.windowManager.spriteBatch.Draw(bars, new Vector2(708 + 10 * i, 336), source, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }

                string warriorCooldown = this.currentWarrior.maxCooldown.ToString();
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, warriorCooldown,
                    new Vector2(708 + 45, 336),
                    Color.Yellow, 0, Vector2.Zero, .7f, SpriteEffects.None, 1f
                    );

                string cooldownExplanation = "COOLDOWN";
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, cooldownExplanation,
                    new Vector2(850, 336),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );


                string cooldownExplained = "Cooldown is how many turns a warrior must wait before they can take any action after completing a turn. ";
                List<String> splitCooldownExplained = StringHelper.SplitString(cooldownExplained, 40);
                cooldownExplained = string.Join("\n", splitCooldownExplained);
                this.windowManager.spriteBatch.DrawString(Game1.smallFont, cooldownExplained,
                    new Vector2(this.pageContent.X + this.boardWidth + 42, 410),
                    Color.Brown, 0, Vector2.Zero, .8f, SpriteEffects.None, 1f
                    );

                this.windowManager.spriteBatch.End(); 
            }

            this.windowManager.spriteBatch.Begin();

            // Make sure the string fits on the screen

            int characterWidthOfBox = 40;
            List<String> splitString = StringHelper.SplitString(welcomeString1, characterWidthOfBox);
            welcomeString1 = string.Join("\n", splitString);
            Vector2 welcomeStringDim1 = Game1.smallFont.MeasureString(welcomeString1);
            this.windowManager.spriteBatch.DrawString(Game1.smallFont, welcomeString1,
                new Vector2(this.pageContent.X + this.boardWidth + 42, this.pageContent.Y + 40),
                Color.Brown, 0, Vector2.Zero, .9f, SpriteEffects.None, 1f
                );
            this.windowManager.spriteBatch.End();
        }

        

        public void saveArmy()
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                foreach (Warrior w in this.warriors)
                {
                    this.updateWarriorInDB(w, db);
                }
            }
        }
        public void updateWarriorInDB(Warrior w, DBConnect db)
        {
            String query = "UPDATE Warriors SET row=@row, col=@col WHERE warrior_id=@warrior_id";
            MySqlCommand cmd = new MySqlCommand(query, db.connection);
            cmd.Parameters.AddWithValue("@row", this.board.rows - w.row - 1);
            cmd.Parameters.AddWithValue("@col", w.col);
            cmd.Parameters.AddWithValue("@warrior_id", w.id);
            int result = cmd.ExecuteNonQuery();
            if (result == 1)
            {
                this.save.isDisabled = true;
                this.recentlySaved = false;
            }
            else
            {
                this.save.isDisabled = false;
                this.recentlySaved = true;
            }
           
            /*
                string query = "SELECT * FROM Warriors NATURAL JOIN users WHERE username=@username and password=@password";
                MySqlCommand cmd = new MySqlCommand(query, db.connection);
                String username = this.windowManager.username;
                String password = this.windowManager.password;
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    int curRow = board.rows - Int32.Parse(dataReader["row"].ToString());
                    int curCol = Int32.Parse(dataReader["col"].ToString());
                    board.warriors[curRow][curCol] = new Warrior(board, curRow, curCol, Direction.N, State.stopped, true, this.windowManager.getWarriorType(dataReader["warriorType"].ToString()));
                    System.Diagnostics.Debug.WriteLine(dataReader["warriorType"]);
                    //System.Diagnostics.Debug.WriteLine(dataReader["username"]);
                    //System.Diagnostics.Debug.WriteLine(dataReader["password"]);
                }

                //close Data Reader
                dataReader.Close();
                 */
        }

    }

}
