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
    public class Dialog
    {
        // this class is probably not going to be done very generally because I'm tired
        // buttons will be 
        public Rectangle dialogBox { get; set; }
        public Boolean isVisible { get; set; }
        public Game1 windowManager { get; set; }
        public const int MARGIN = 10;

        public Dialog(Game1 windowManager, int width, int height, Boolean isVisible = false)
        {
            this.windowManager = windowManager;
            this.isVisible = isVisible;
            // find where rectangle should be
            this.dialogBox = new Rectangle((this.windowManager.Window.ClientBounds.Width - width) / 2,
                (this.windowManager.Window.ClientBounds.Height - height) / 2, width, height);
        }

        //public void update(GameTime gameTime);
        public void draw()
        {
            if (this.isVisible)
            {
                // fade everything out
                this.windowManager.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                this.windowManager.spriteBatch.Draw(Game1.charcoal, new Rectangle(0, 0, this.windowManager.Window.ClientBounds.Width, this.windowManager.Window.ClientBounds.Height), Color.White * .7f);
                this.windowManager.spriteBatch.End();

                // draw dialog box background
                this.windowManager.spriteBatch.Begin();
                this.windowManager.spriteBatch.Draw(Game1.charcoal, new Rectangle(dialogBox.X - MARGIN, dialogBox.Y - MARGIN, dialogBox.Width + 2 * MARGIN, dialogBox.Height + 2 * MARGIN), Color.White);
                this.windowManager.spriteBatch.Draw(Game1.background, this.dialogBox, Color.White);
                this.windowManager.spriteBatch.End();
            }
        }

        public void draw2()
        {
            if (this.isVisible)
            {
                // draw dialog box background
                this.windowManager.spriteBatch.End();
                this.windowManager.spriteBatch.Begin();
                this.windowManager.spriteBatch.Draw(Game1.charcoal, new Rectangle(dialogBox.X - MARGIN, dialogBox.Y - MARGIN, dialogBox.Width + 2 * MARGIN, dialogBox.Height + 2 * MARGIN), Color.White);
                this.windowManager.spriteBatch.Draw(Game1.background, this.dialogBox, Color.White);
                this.windowManager.spriteBatch.End();
            }
        }

        public Rectangle getComponentLocation(int relativeHeight, int width, int height)
        {
            return new Rectangle((this.windowManager.Window.ClientBounds.Width - width) / 2,
                this.dialogBox.Y + relativeHeight, width, height);
        }
    }
}
