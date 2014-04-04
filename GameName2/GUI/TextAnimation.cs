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
    public class TextAnimation
    {
        public Rectangle bounds { get; set; }
        public List<String> phrases { get; set; }
        public int interval { get; set; } // in milliseconds
        public SpriteFont textFont { get; set; }
        public Boolean isVisible { get; set; }

        private double currentStep = 0;

        public TextAnimation(Rectangle bounds, List<String> phrases, int interval, SpriteFont textFont, Boolean isVisible = true)
        {
            this.bounds = bounds;
            this.phrases = phrases;
            this.interval = interval;
            this.textFont = textFont;
            this.isVisible = isVisible;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (this.isVisible)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(this.textFont, phrases[(int)currentStep], new Vector2(bounds.X, bounds.Y), Color.White);
                spriteBatch.End();
            }
        }

        public void update(GameTime gameTime)
        {
            currentStep += gameTime.ElapsedGameTime.Milliseconds / (double) interval;
            currentStep = currentStep % phrases.Count;
        }
    }
}
