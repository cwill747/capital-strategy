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
        int chanceHit;
        string content;


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
                int padding = 5;
                spriteBatch.Draw(Game1.charcoal, new Rectangle(this.dialogBox.X - padding, this.dialogBox.Y - padding, this.dialogBox.Width + 2 * padding,
                    this.dialogBox.Height + 2 * padding), Color.White);
                spriteBatch.Draw(Game1.background, dialogBox, Color.White);
                string message = "Base damage: " + this.baseDamage + "\n";
                int totalDamage = this.baseDamage;
                if (this.hasTypeAdvantage)
                {
                    int bonusDamage = (int)(this.baseDamage * .33);
                    message += "Type adv bonus: +" + bonusDamage + "\n";
                    totalDamage += bonusDamage;
                }
                if (this.hasFromBehindBonus)
                {
                    int bonusDamage = (int)(this.baseDamage * .33);
                    message += "Back stab bonus: +" + bonusDamage + "\n";
                    totalDamage += bonusDamage;
                }
                if (this.hasTypeAdvantage || this.hasFromBehindBonus)
                {
                    message += "Total damage: " + totalDamage + "\n";
                }
                message += "Chance hit: " + this.chanceHit + "%";
                spriteBatch.DrawString(Game1.smallFont, message, new Vector2(this.dialogBox.X, this.dialogBox.Y), Color.White);
                spriteBatch.End();
            }
        }

        public void updateContents(Warrior attacking, Warrior target)
        {
            this.warriorToAttack = target;
            this.baseDamage = (attacking.attack * 25) / target.defense;
            this.hasTypeAdvantage = attacking.warriorClass.indexOfAdvantageAgainst == target.warriorClass.index;
            int direction = target.getDirectionTo(attacking);
            this.hasFromBehindBonus = direction == (target.direction + 4) % 8;
            if (hasFromBehindBonus)
            {
                chanceHit = 100;
            }
            else if (direction == target.direction || direction == (target.direction + 1) % 8 || direction == (target.direction - 1) % 8)
            {
                chanceHit = 80;
            }
            else
            {
                chanceHit = 90;
            }
        }

    }
}
