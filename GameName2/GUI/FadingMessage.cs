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

namespace CapitalStrategy.GUI
{
    public class FadingMessage
    {
        private float x;
        private float y;
        public int fadeDelay { get; set; }
        private int currentDelay = -1;
        public string message { get; set; }
        private SpriteFont font;
        public Color color { get; set; }

        public FadingMessage(float centerX, float centerY, string message, SpriteFont font, int fadeDelay, Color color)
        {
            this.fadeDelay = fadeDelay;
            this.font = font;
            this.message = message;
            this.moveTo(centerX, centerY);
            this.color = color;
        }

        public void show()
        {
            this.currentDelay = this.fadeDelay;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (currentDelay > 0)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, message, new Vector2(x, y), color);
                spriteBatch.End();
            }
        }

        public void moveTo(float newCenterX, float newCenterY)
        {
            Vector2 dim = font.MeasureString(message);
            this.x = newCenterX - dim.X / 2;
            this.y = newCenterY - dim.Y / 2;
        }


        public void update(GameTime gameTime)
        {
            this.currentDelay -= gameTime.ElapsedGameTime.Milliseconds;
        }
    }
}
