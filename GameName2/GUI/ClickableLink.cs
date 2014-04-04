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
    class ClickableLink
    {
        public Boolean isPressed { get; set; }
        public SpriteFont labelFont { get; set; }
        public Boolean clicked { get; set; }
        public Boolean isDisabled { get; set; }
        public Boolean isVisible { get; set; }

        public ClickableLink(Rectangle location, SpriteFont labelFont, Boolean isDisabled = false, Boolean isVisible = true)
        {
            this.isPressed = false;
            this.labelFont = labelFont;
            this.clicked = false;
            this.isDisabled = isDisabled;
            this.isVisible = isVisible;
        }

    }
}
