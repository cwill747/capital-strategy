using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input.Touch;
using MySql.Data.MySqlClient;

namespace CapitalStrategy
{
    public class Board
    {
        public int WARRIORWIDTH { get; set; }
        public int WARRIORHEIGHT { get; set; }

        public int rows { get; set; }
        public int cols { get; set; }
        public Rectangle location { get; set; }
        public Warrior[][] warriors { get; set; }
        public Color[][] tileTints { get; set; }
        ImageAtlas tileAtlas;
        


        public Board(int rows, int cols, Rectangle location, Texture2D tileImage)
        {
            this.rows = rows;
            this.cols = cols;
            this.location = location;
            warriors = new Warrior[rows][];
            for (int i = 0; i < rows; i++)
            {
                warriors[i] = new Warrior[cols];
            }
            tileTints = new Color[rows][];
            for (int i = 0; i < rows; i++)
            {
                tileTints[i] = new Color[cols];
            }
            this.WARRIORWIDTH = location.Width * 2 / this.cols;
            this.WARRIORHEIGHT = location.Height * 2 / this.rows;
            this.resetTints();
            int tileWidth = location.Width / warriors[0].Length;
            int tileHeight = location.Height / warriors.Length;
            this.tileAtlas = new ImageAtlas(tileImage, 13, 20, 56);
        }

        public void drawTiles(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Game1.charcoal, new Rectangle(location.X - 5, location.Y - 5, location.Width + 10, location.Height + 10), Color.WhiteSmoke);
            spriteBatch.End();
            int tileWidth = location.Width / warriors[0].Length;
            int tileHeight = location.Height / warriors.Length;
            for (int i = 0; i < warriors.Length; i++)
            {
                for (int j = 0; j < warriors[i].Length; j++)
                {
                    this.tileAtlas.draw(spriteBatch, new Rectangle(j*tileWidth + location.X, i * tileHeight + location.Y, tileWidth, tileHeight), tileTints[i][j]);
                }
            }
        }
        public Vector2 getLocation(double row, double col)
        {
            double width = this.location.Width * col / this.cols + location.X;
            double height = this.location.Height* row / this.rows + location.Y;
            Vector2 retVal = new Vector2((float)width, (float)height);
            return retVal;
        }
        public void resetTints()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    tileTints[i][j] = Color.White;
                }
            }
        }
        public Vector2 clickOverGrid(float x, float y)
        {
            int clickRow = (int)((y - this.location.Y) / (this.location.Height / this.rows));
            int clickCol = (int)((x - this.location.X) / (this.location.Width / this.cols));
            
            return new Vector2(clickRow, clickCol);
        }
        public Boolean isClickOverGrid(float x, float y)
        {
            return x - this.location.X < this.location.Width && y - this.location.Y < this.location.Height && x - this.location.X >= 0 && y - this.location.Y >= 0;
        }

        public List<WarriorWrapper> loadWarriors(Game1 windowManager, Boolean isPlayer1)
        {
            List<WarriorWrapper> retVal = new List<WarriorWrapper>();
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                string query = "SELECT * FROM Warriors NATURAL JOIN users WHERE username=@username";
                MySqlCommand cmd = new MySqlCommand(query, db.connection);
                String username = windowManager.username;
                if (!isPlayer1) {
                    username = windowManager.otherPlayer.username;
                }
                cmd.Parameters.AddWithValue("@username", username);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    int curRow = Int32.Parse(dataReader["row"].ToString());
                    if (isPlayer1)
                    {
                        curRow = this.rows - curRow - 1;
                    }
                    int curCol = Int32.Parse(dataReader["col"].ToString());
                    int id = Int32.Parse(dataReader["warrior_id"].ToString());
                    Warrior w = new Warrior(this, id, curRow, curCol, isPlayer1 ? Direction.N : Direction.S, State.stopped, isPlayer1, windowManager.warriorTypes[Int32.Parse(dataReader["warrior_type_id"].ToString()) - 1]);
                    this.warriors[curRow][curCol] = w;
                    WarriorWrapper ww = new WarriorWrapper(w, Int32.Parse(dataReader["warrior_id"].ToString()));
                    //System.Diagnostics.Debug.WriteLine(dataReader["username"]);
                    //System.Diagnostics.Debug.WriteLine(dataReader["password"]);
                    retVal.Add(ww);
                }

                //close Data Reader
                dataReader.Close();
            }



            return retVal;
        }


    }

}
