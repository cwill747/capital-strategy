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
    class InputDialog
    {
        public const double cursorDuration = 500;

        public float CHAR_WIDTH = 4;
        public String label { get; set; }
        public String content { get; set; }
        public Rectangle location { get; set; }
        public Boolean isActive { get; set; }
        public SpriteFont labelFont { get; set; }
        public SpriteFont inputFont { get; set; }
        public double cursorState { get; set; }
        public Boolean mask { get; set; }
        public Boolean isVisible { get; set; }

        public InputDialog(String label, Rectangle location, Boolean isActive = false, Boolean mask = false, Boolean isVisible = true)
        {
            this.label = label;
            this.location = location;
            this.isActive = isActive;
            this.content = "";
            this.labelFont = Game1.smallFont;
            this.inputFont = Game1.smallFont;
            this.cursorState = 0;
            this.mask = mask;
            this.isVisible = isVisible;
        }

        public void clear()
        {
            this.content = "";
        }

        public void update(GameTime gameTime)
        {
            this.cursorState += (gameTime.ElapsedGameTime.Milliseconds / (double)InputDialog.cursorDuration);
            this.cursorState = cursorState % 2;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (this.isVisible)
            {
                Vector2 labelDim = this.labelFont.MeasureString(label);
                Vector2 contentDim = this.inputFont.MeasureString(content);
                spriteBatch.Begin();

                spriteBatch.DrawString(labelFont, label, new Vector2(location.X - (labelDim.X + 20), location.Y), Color.White);
                Color inputBackground = Color.White;
                if (!this.isActive)
                {
                    inputBackground = Color.FromNonPremultiplied(new Vector4((float).85, (float).85, (float).85, 1));
                }
                spriteBatch.Draw(Game1.inputPaneImage, this.location, inputBackground);

                // keep substringing until content is not too long
                String output = content;
                if (this.mask)
                {
                    output = "";
                    for (int i = 0; i < content.Length; i++)
                    {
                        output += "*";
                    }
                }
                while (this.inputFont.MeasureString(output).X >= location.Width - 8)
                {
                    output = output.Substring(1);
                }
                spriteBatch.DrawString(this.inputFont, output, new Vector2(this.location.X + 4, this.location.Y), Color.Black);
                if (this.isActive && cursorState < 1)
                {
                    spriteBatch.DrawString(this.inputFont, "|", new Vector2(this.location.X + this.inputFont.MeasureString(output).X + 6, this.location.Y), Color.Black);
                }
                spriteBatch.End();
            }
        }

        public void backspace()
        {
            if (this.content.Length > 0)
            {
                this.content = this.content.Substring(0, this.content.Length - 1);
            }
        }
        public void addChar(Char add)
        {
            this.content = this.content + add;
        }
        public void handleKey(Keys key)
        {
            if (key.Equals(Keys.Back))
            {
                this.backspace();
            }
            else
            {
                Char[] charArray = key.ToString().ToLower().ToCharArray();
                if (charArray.Length == 1)
                {
                    this.addChar(charArray[0]);
                }
                if (key.Equals(Keys.Space))
                {
                    this.addChar(' ');
                }

            }
        }
        public Boolean handleClick(MouseState mouseState)
        {
            if (this.isVisible && mouseState.X >= location.X && mouseState.X <= location.X + location.Width && mouseState.Y >= location.Y && mouseState.Y <= location.Y + location.Height)
            {
                this.isActive = true;
                return true;
            }
            return false;
        }
        public void toggleActive()
        {
            this.isActive = !this.isActive;
            if (!this.isVisible)
            {
                this.isActive = false;
            }
        }
        public void setVisible(Boolean isVisible)
        {
            this.isVisible = isVisible;
            if (this.isVisible == false)
            {
                this.toggleActive();
            }
        }
    }
}
