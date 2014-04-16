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
    public class BackButton : Button
    {
        public BackButton() : base("", new Rectangle(20, 20, 50, 50), Game1.smallFont)
        {
            
        }

        
        public void drawBackButton(SpriteBatch spriteBatch)
        {
            Color colorToUse = Color.White;
            if (this.clicked)
            {
                colorToUse = Color.Gray;
            }
            spriteBatch.Begin();
            spriteBatch.Draw(Game1.backButton, location, colorToUse);
            spriteBatch.End();
        }

        public void update(GameTime gameTime)
        {
        }
    }
}
