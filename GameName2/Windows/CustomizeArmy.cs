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
using MySql.Data.MySqlClient;
using CapitalStrategy.GUI;

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
        public List<WarriorWrapper> warriorWrappers { get; set; }
        public Texture2D red;
        public Texture2D white;
        public Texture2D heartIcon;
        public Texture2D attackIcon;
        public Texture2D shieldIcon;
        public Texture2D moveIcon;
        public SpriteFont menufont;
        public SpriteFont infofont;
        public SpriteFont smallfont;
        public Texture2D hourglass;
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

        MouseWrapper mouseState;
        public CustomizeArmy(Game1 windowManager)
        {
            this.windowManager = windowManager;

        }

        public void Initialize()
        {
            this.pageContent.X = 20;
            this.pageContent.Y = 150;
            boardHeight = 350 - 50;
            this.pageContent.Height = boardHeight;
            boardWidth = 700 - 100;
            this.pageContent.Width = boardWidth;
            board = new Board(5, 10, new Rectangle(this.pageContent.X, this.pageContent.Y, boardWidth, boardHeight), Game1.tileImage);
            this.oldMouseState = new MouseState();
            this.warriorWrappers = this.board.loadWarriors(this.windowManager, true);

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
        }

        public void Update(GameTime gameTime)
        {
            this.save.update(gameTime);
            MouseState newMouseState = Mouse.GetState();
            if (!newMouseState.Equals(this.oldMouseState))
            {
                if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
                {
                    this.backButton.checkClick(newMouseState);
                    this.save.checkClick(newMouseState);
                    this.aRange.checkClick(newMouseState);
                    this.mRange.checkClick(newMouseState);
                    if (board.isClickOverGrid(newMouseState.X, newMouseState.Y))
                    {
                        Vector2 coord = board.clickOverGrid(newMouseState.X, newMouseState.Y);
                        currentWarrior = board.warriors[(int)coord.X][(int)coord.Y];
                        board.warriors[(int)coord.X][(int)coord.Y] = null;
                        if (currentWarrior != null)
                        {
                            currentWarrior.x = newMouseState.X;
                            currentWarrior.y = newMouseState.Y;
                        }
                    }
                }
                else if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    // update current warrior location
                    if (currentWarrior != null)
                    {
                        currentWarrior.x = newMouseState.X;
                        currentWarrior.y = newMouseState.Y;
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
                        Vector2 rowCol = board.clickOverGrid(currentWarrior.x, currentWarrior.y);
                        int row = (int)rowCol.X;
                        int col = (int)rowCol.Y;
                        if (!board.isClickOverGrid(currentWarrior.x, currentWarrior.y))
                        {
                            board.warriors[(int)currentWarrior.row][(int)currentWarrior.col] = currentWarrior;
                        }
                        else if (board.warriors[row][col] == null)
                        {
                            currentWarrior.row = row;
                            currentWarrior.col = col;
                            board.warriors[row][col] = currentWarrior;
                        }
                        else
                        {
                            board.warriors[(int)currentWarrior.row][(int)currentWarrior.col] = currentWarrior;
                        }
                        currentWarrior = null;
                        this.save.isDisabled = false;
                        this.recentlySaved = false;
                    }
                }
            }
            this.oldMouseState = newMouseState;
        }

        public void Draw()
        {

            this.windowManager.spriteBatch.Begin();
            windowManager.spriteBatch.Draw(Game1.background, new Rectangle(0, 0, this.windowManager.Window.ClientBounds.Width, this.windowManager.Window.ClientBounds.Height), Color.White);
            this.windowManager.spriteBatch.End();
            this.board.drawTiles(this.windowManager.spriteBatch);
            for (int row = 0; row < board.warriors.Length; row++)
            {
                for (int col = 0; col < board.warriors[row].Length; col++)
                {

                    Warrior warrior = board.warriors[row][col];
                    //System.Diagnostics.Debug.WriteLine("draw");
                    if (warrior != null)
                    {
                        //this.windowManager.spriteBatch.Begin();
                        //this.windowManager.spriteBatch.DrawString(Game1.smallFont, "hi", board.getLocation(row, col), Color.White);
                        //this.windowManager.spriteBatch.End();
                        warrior.draw();
                    }
                }
            }
            if (currentWarrior == null)
            {
                board.resetTints();

            }
            if (currentWarrior != null)
            {
                currentWarrior.drawToLocation();
                if (aRange.isDisabled)
                {
                    currentWarrior.drawAttackRange();
                }
                else
                {
                    Boolean[][] discovered = currentWarrior.bredthFirst((int)currentWarrior.row, (int)currentWarrior.col, currentWarrior.maxMove);
                for (int i = 0; i < discovered.Length; i++)
                {
                    for (int j = 0; j < discovered[i].Length; j++)
                    {
                        if (discovered[i][j])
                        {
                            board.tileTints[i][j] = Warrior.yourMoveColor;
                        }
                    }

                }
            }
                
            }
            this.backButton.drawBackButton(this.windowManager.spriteBatch);
            this.save.draw(this.windowManager.spriteBatch);
            this.aRange.draw(this.windowManager.spriteBatch);
            this.mRange.draw(this.windowManager.spriteBatch);
            if (recentlySaved)
            {
                this.windowManager.spriteBatch.Begin();
                this.windowManager.spriteBatch.DrawString(this.infofont, "Successfully Saved!", new Vector2(this.pageContent.X + this.pageContent.Width,
                this.pageContent.Y + this.pageContent.Height - this.saveButtonHeight), Color.Chocolate);
                this.windowManager.spriteBatch.End();
            }
            int padding2 = 10;
            this.windowManager.spriteBatch.Begin();
            String rangeDisplay;
            if (!aRange.isDisabled)
            {
                rangeDisplay = "Movement ";
            }
            else
            {
                rangeDisplay = "Attack ";
            }
            this.windowManager.spriteBatch.DrawString(this.smallfont, rangeDisplay+"Range", new Vector2(this.pageContent.X + this.pageContent.Width - padding2 - (3 * saveButtonWidth),
            this.pageContent.Y + this.pageContent.Height - this.saveButtonHeight + padding2), Color.Chocolate);
            this.windowManager.spriteBatch.End();
          
            if (this.currentWarrior != null)
            {
                Warrior displayWarrior = new Warrior(this.currentWarrior);
                int imgPadding = 20;
                
                int toDrawX = this.pageContent.X + this.boardWidth + 10;
                int toDrawY = this.pageContent.Y;
           
                int iconWidth = tileWidth / 2;
                int iconHeight = iconWidth;
                int padding = 30;
                int barHeight = tileHeight / 10;
                double widthPerPoint = ((double)tileWidth) / 100;

                displayWarrior.drawInArbitraryLocation(toDrawX, toDrawY);
                displayWarrior.drawWarriorType(toDrawX + board.WARRIORWIDTH / 2 + imgPadding, toDrawY - imgPadding);
                toDrawX = toDrawX + board.WARRIORWIDTH / 2 + imgPadding;
                toDrawY += iconHeight + padding;


                this.windowManager.spriteBatch.Begin();

                // Draw the warriors attack strength
                this.windowManager.spriteBatch.Draw(attackIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.windowManager.spriteBatch.Draw(Game1.charcoal, new Rectangle(2 + toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
                this.windowManager.spriteBatch.Draw(white, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
                this.windowManager.spriteBatch.Draw(red, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * currentWarrior.attack), barHeight), Color.WhiteSmoke);
                toDrawY += iconHeight + padding;

                // Draw the warriors defense strength
                // @TODO: Change the 2+ on this to be a resolution-independent value
                this.windowManager.spriteBatch.Draw(shieldIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.windowManager.spriteBatch.Draw(Game1.charcoal, new Rectangle(2 + toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
                this.windowManager.spriteBatch.Draw(white, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
                this.windowManager.spriteBatch.Draw(red, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * currentWarrior.defense), barHeight), Color.WhiteSmoke);
                toDrawY += iconHeight + padding;

                // Draw the warriors cooldown
                string coolDisplay = this.currentWarrior.maxCooldown.ToString();
                this.windowManager.spriteBatch.Draw(hourglass, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.windowManager.spriteBatch.DrawString(this.infofont, coolDisplay, new Vector2(2 + toDrawX + iconWidth + padding, toDrawY), Color.White);

                this.windowManager.spriteBatch.End();
            }
        }

        

        public void saveArmy()
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                foreach (WarriorWrapper ww in this.warriorWrappers)
                {
                    this.updateWarriorInDB(ww, db);
                }
            }
        }
        public void updateWarriorInDB(WarriorWrapper ww, DBConnect db)
        {
            String query = "UPDATE Warriors SET row=@row, col=@col WHERE warrior_id=@warrior_id";
            MySqlCommand cmd = new MySqlCommand(query, db.connection);
            cmd.Parameters.AddWithValue("@row", this.board.rows - ww.warrior.row - 1);
            cmd.Parameters.AddWithValue("@col", ww.warrior.col);
            cmd.Parameters.AddWithValue("@warrior_id", ww.id);
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
