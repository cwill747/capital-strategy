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
using CapitalStrategy.Windows;

namespace CapitalStrategy.GUI
{
    public class AttackInfoPane
    {
        public int x { get; set; }
        public int y { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool isVisible { get; set; }
        public Warrior warriorToAttack { get; set; }
        public Warrior attackingWarrior { get; set; }
        public int damage { get; set; }
        public double hitChance { get; set; }
        public int baseDamage { get; set; }
        public bool hasTypeAdvantage { get; set; }
        public bool hasFromBehindBonus { get; set; }
        private GameMatch game { get; set; }
        int chanceHit;
        string content;


        public AttackInfoPane(GameMatch game, int width, int height, bool isVisible = false)
        {
            this.width = width;
            this.height = height;
            this.isVisible = isVisible;
            this.warriorToAttack = null;
            this.game = game;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            if (this.isVisible)
            {
                spriteBatch.Begin();
                int padding = 5;
                //spriteBatch.Draw(Game1.charcoal, new Rectangle(this.x - padding, this.y - padding, width + 2 * padding,
                 //   this.height + 2 * padding), Color.White);
                spriteBatch.Draw(Game1.infoBackground, new Rectangle(x - 20, y - 7, width + 40, height + 14), Color.White);
                string message = ""; 
                if (this.baseDamage >= 0)
                {
                    message = "Base damage: " + this.baseDamage + "\n";
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
                }
                else
                {
                    message = "Fully heals target.\n";   
                }
                message += "Chance hit: " + this.chanceHit + "%";
                
                spriteBatch.End();
                this.warriorToAttack.drawInArbitraryLocation(this.x + 10, this.y + 35);
                //this.attackingWarrior.drawInArbitraryLocation(this.x + 10, this.y + this.game.board.WARRIORHEIGHT);
                int xEnd = (int)this.x - (this.game.board.WARRIORWIDTH - this.game.board.location.Width / this.game.board.cols) / 2 + this.game.board.WARRIORWIDTH;
                int yEnd = (int)this.y - (this.game.board.WARRIORHEIGHT - this.game.board.location.Height / this.game.board.rows) / 2 - this.game.board.location.Height / this.game.board.rows / 3 + this.game.board.WARRIORHEIGHT;
                int tileWidth = this.game.BOARDWIDTH / this.game.board.cols;
                int tileHeight = this.game.BOARDHEIGHT / this.game.board.rows;
                double widthPerHP = ((double)tileWidth) / 100;
                
                spriteBatch.Begin();
                message = "target HP: " + this.warriorToAttack.health + "/" + this.warriorToAttack.maxHealth + "\n" + message;
                spriteBatch.DrawString(Game1.smallFont, message, new Vector2(xEnd, this.y + 3), Color.Red);
                //spriteBatch.Draw(Game1.charcoal, new Rectangle(xLoc - 2, yLoc - 2, (int)(widthPerHP * this.warriorToAttack.maxHealth) + 4, tileHeight / 10 + 4), Color.WhiteSmoke);
                //spriteBatch.Draw(game.white, new Rectangle(xLoc, yLoc, (int)(widthPerHP * this.warriorToAttack.maxHealth), tileHeight / 10), Color.WhiteSmoke);
                //spriteBatch.Draw(game.red, new Rectangle(xLoc, yLoc, (int)(widthPerHP * this.warriorToAttack.health), tileHeight / 10), Color.WhiteSmoke);
                spriteBatch.End();
            }
        }

        public void updateContents(Warrior attacking, Warrior target)
        {
            this.warriorToAttack = new Warrior(target);
            this.warriorToAttack.direction = Direction.S;
            this.attackingWarrior = new Warrior(attacking);
            this.attackingWarrior.direction = Direction.N;
            this.baseDamage = (attacking.attack * 20) / target.defense;
            this.hasTypeAdvantage = attacking.warriorClass.indexOfAdvantageAgainst == target.warriorClass.index;
            int direction = target.getDirectionTo(attacking);
            this.hasFromBehindBonus = direction == (target.direction + 4) % 8;
            if (hasFromBehindBonus)
            {
                chanceHit = 100;
            }
            else if (direction == target.direction || direction == (target.direction + 1) % 8 || direction == (target.direction - 1) % 8)
            {
                chanceHit = 70;
            }
            else
            {
                chanceHit = 85;
            }
            this.relocate(this.game.board.getLocation(attacking.row, attacking.col), this.game.board.getLocation(target.row, target.col));
        }

        public void relocate(Vector2 avoid1, Vector2 avoid2)
        {
            //this.x = this.game.BOARDWIDTH - this.width - 10;
            //this.y = 15;
            
            this.x = (int)(avoid2.X + 30);
            this.y = (int)(avoid2.Y - this.height - 40);
            int count = 0;
            while (this.covers(avoid1) || this.covers(avoid2) || y < 15 || y > this.game.BOARDHEIGHT - 15 - this.height ||
                x < 15 || x > this.game.BOARDWIDTH - this.width - 15)
            {
                count++;
                if (count > 5000)
                {
                    return;
                }
                if (y < this.game.BOARDHEIGHT - 15 - this.height)
                {
                    
                    y += 10;
                }
                else
                {
                    x += 10;
                    y = 15;
                    if (x > this.game.BOARDWIDTH - this.width - 15)
                    {
                        x = 15;
                    }
                }
            }
        }
        public bool covers(Vector2 point)
        {
            return this.getBounds().Intersects(new Rectangle((int)point.X, (int)point.Y - 40, (int)this.game.board.WARRIORWIDTH, (int)this.game.board.WARRIORHEIGHT));
        }

        public Rectangle getBounds()
        {
            return new Rectangle(x, y, width, height);
        }

    }
}
