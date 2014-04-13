using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CapitalStrategy
{
    public class ImageAtlas
    {
        public Texture2D texture { get; set; }
        public int rows { get; set; }
        public int cols { get; set; }
        public int currentFrame { get; set; }

        public ImageAtlas(Texture2D texture, int rows, int cols, int currentFrame)
        {
            this.texture = texture;
            this.rows = rows;
            this.cols = cols;
            this.currentFrame = currentFrame;
        }


        public void draw(SpriteBatch spriteBatch, Rectangle location)
        {
            int rowIndex = currentFrame / cols;
            int colIndex = currentFrame % cols;
            draw(spriteBatch, location, rowIndex, colIndex);
        }
        public void draw(SpriteBatch spriteBatch, Rectangle location, Color tint)
        {
            int rowIndex = currentFrame / cols;
            int colIndex = currentFrame % cols;
            draw(spriteBatch, location, rowIndex, colIndex, tint);
        }
        public void draw(SpriteBatch spriteBatch, Rectangle location, int row, int col)
        {
            draw(spriteBatch, location, row, col, Color.White);
        }
        public void draw(SpriteBatch spriteBatch, Rectangle location, int row, int col, Color tint)
        {
            int width = texture.Width / cols;
            int height = texture.Height / rows;
            Rectangle source = new Rectangle(col * width, row * height, width, height);
            Rectangle destination = new Rectangle((int)location.X, (int)location.Y, location.Width, location.Height);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, destination, source, tint);
            spriteBatch.End();
        }
    }
}
