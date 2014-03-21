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

        public CustomizeArmy(Game1 windowManager)
        {
            this.windowManager = windowManager;

        }

        public void Initialize()
        {
            int xOffset = (this.windowManager.Window.ClientBounds.Width - 700) / 2;
            board = new Board(5, 10, new Rectangle(xOffset, 100, 700, 350), Game1.tileImage);
            this.oldMouseState = new MouseState();
            this.loadWarriors();
            
            
        }

        public void LoadContent()
        {
            this.backButton = new BackButton();
        }

        public void Update(GameTime gameTime)
        {
            MouseState newMouseState = Mouse.GetState();
            if (!newMouseState.Equals(this.oldMouseState))
            {
                if (newMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton != ButtonState.Pressed)
                {
                    if (backButton.checkClick(newMouseState))
                    {
                    }
                    
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
            if (currentWarrior != null)
            {
                currentWarrior.drawToLocation();
            }
            this.backButton.drawBackButton(this.windowManager.spriteBatch);
        }

        public List<Warrior> loadWarriors()
        {
            List<Warrior> retVal = new List<Warrior>();
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
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
            }
        


            return retVal;
        }
    }
}
