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
    public class AttackInfoPane
    {
        public Rectangle dialogBox { get; set; }
        public bool isVisible { get; set; }
        public Warrior warriorToAttack { get; set; }
        public int damage { get; set; }
        public double hitChance { get; set; }
        public int baseDamage { get; set; }
        public bool hasTypeAdvantage { get; set; }
        public bool hasFromBehindBonus { get; set; }


        public AttackInfoPane(int width, int height, bool isVisible = false)
        {
            this.dialogBox = new Rectangle(0, 0, width, height);
            this.isVisible = isVisible;
            this.warriorToAttack = null;
            
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (this.isVisible)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(Game1.background, dialogBox, Color.White);
                spriteBatch.End();
            }
        }

    }
}
