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
    class Button
    {
        public Rectangle location { get; set; }
        public Boolean isPressed { get; set; }
        public String label { get; set; }
        public SpriteFont labelFont { get; set; }
        public Boolean clicked { get; set; }
        public Button(String label, Rectangle location)
        {
            this.location = location;
            this.isPressed = false;
            this.label = label;
            this.labelFont = Game1.smallFont;
            this.clicked = false;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            Color colorToUse = Color.White;
            if (this.clicked)
            {
                colorToUse = Color.Gray;
            }
            spriteBatch.Begin();
            spriteBatch.Draw(Game1.button, location, colorToUse);
            Vector2 labelDim = this.labelFont.MeasureString(label);
            float x = location.X + (location.Width - labelDim.X) / 2;
            float y = location.Y + (location.Height - labelDim.Y) / 2;
            spriteBatch.DrawString(this.labelFont, label, new Vector2(x, y), colorToUse);
            spriteBatch.End();
        }

        public void update(GameTime gameTime)
        {
        }

        public Boolean checkClick(MouseState mouseState)
        {
            if (mouseState.X >= location.X && mouseState.X <= location.X + location.Width && mouseState.Y >= location.Y && mouseState.Y <= location.Y + location.Height)
            {
                this.clicked = true;
                return true;
            }
            return false;
        }
        public void unClick()
        {
            this.clicked = false;
        }
    }
}
