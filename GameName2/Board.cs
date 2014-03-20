using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CapitalStrategy
{
    public class Board
    {
        public int width { get; set; }
        public int height { get; set; }
        public int rows { get; set; }
        public int cols { get; set; }
        public Warrior[][] warriors { get; set; }
        public Color[][] tileTints { get; set; }
        ImageAtlas tileAtlas;
        


        public Board(int rows, int cols, int width, int height, Texture2D tileImage)
        {
            this.rows = rows;
            this.cols = cols;
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
            this.resetTints();
            this.width = width;
            this.height = height;
            int tileWidth = width / warriors[0].Length;
            int tileHeight = height / warriors.Length;
            this.tileAtlas = new ImageAtlas(tileImage, tileWidth, tileHeight, 13, 20, 108);
        }

        public void drawTiles(SpriteBatch spriteBatch)
        {
            int tileWidth = width / warriors[0].Length;
            int tileHeight = height / warriors.Length;
            for (int i = 0; i < warriors.Length; i++)
            {
                for (int j = 0; j < warriors[i].Length; j++)
                {
                    this.tileAtlas.draw(spriteBatch, new Rectangle(j*tileWidth, i * tileHeight, tileWidth, tileHeight), tileTints[i][j]);
                }
            }
        }
        public Vector2 getLocation(double row, double col)
        {
            double width = this.width * col / this.cols;
            double height = this.height * row / this.rows;
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
    }
}
