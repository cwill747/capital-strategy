﻿#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using CapitalStrategy.Windows;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace CapitalStrategy
{
    /// <summary>
    /// Class specific to the instance of a particular warrior
    /// </summary>
    public class Warrior : WarriorType
    {
        public static Color yourMoveColor = Color.Blue;
        public static Color notYourMoveColor = Color.LightBlue;
        public static Color attackColor = Color.OrangeRed;
        public static Color targetAcquiredColor = Color.Red;

        public int id { get; set; }
        public double row { get; set; } // needs to be a float for movement
        public double col { get; set; }
        public int destRow { get; set; }
        public int destCol { get; set; }
        public int direction { get; set; } // use Direction class
        public int health { get; set; }
        public int cooldown { get; set; } // cooldown is set to maxCooldown when the unit attacks (even if it attacked nothing)
        public int state { get; set; } // use movement class
        public double stateDepth;
        public Boolean isYours; // by yours, I mean player 1/bottom player
        public DijkstraNode currentStep { get; set; }
        public Board board { get; set; }
        public int curDelay { get; set; }
        public string description;
        public float x { get; set; }
        public float y { get; set; }
        public ContentManager Content { get; set; }


        public Warrior(Board board, int id, double row, double col, int direction, int state, Boolean isYours, WarriorType warriorType)
            : base(warriorType)
        {
            this.id = id;
            this.board = board;
            this.row = row;
            this.col = col;
            this.direction = direction;
            this.state = state;
            this.isYours = isYours;
            this.health = warriorType.maxHealth;
            this.cooldown = 0;
            this.stateDepth = 0;
            this.curDelay = 0;
            this.description = warriorType.description;
        }

        public Warrior(Warrior previousWarrior)
            : base(previousWarrior)
        {
            this.board = previousWarrior.board;
            this.row = previousWarrior.row;
            this.col = previousWarrior.col;
            this.direction = Direction.S;
            this.state = State.walking;
            this.health = previousWarrior.maxHealth;
            this.cooldown = 0;
            this.stateDepth = 0;
            this.curDelay = 0;
            this.isYours = previousWarrior.isYours;
            this.description = previousWarrior.description;
            this.health = previousWarrior.health;
        }
        public void draw()
        {
            SpriteBatch spriteBatch = this.game.spriteBatch;
            Vector2 destinationLoc = board.getLocation(this.row, this.col);
            //string EntityInfo = "This is a warrior";
            SpriteFont menufont = this.game.menufont;
            Rectangle destination = new Rectangle((int)destinationLoc.X - (board.WARRIORWIDTH - board.location.Width / board.cols) / 2, (int)destinationLoc.Y - (board.WARRIORHEIGHT - board.location.Height / board.rows) / 2 - board.location.Height / board.rows / 3, board.WARRIORWIDTH, board.WARRIORHEIGHT);
            ImageAtlas source = this.states[state];


            source.draw(spriteBatch, destination, direction, (int)stateDepth, this.isYours ? Color.White : Color.Red);
            Vector2 vec = new Vector2(0, 0);
        }
        public void drawToLocation()
        {
            SpriteBatch spriteBatch = this.game.spriteBatch;
            Vector2 destinationLoc = new Vector2(this.x, this.y);
            //string EntityInfo = "This is a warrior";
            SpriteFont menufont = this.game.menufont;
            Rectangle destination = new Rectangle((int)destinationLoc.X - (board.WARRIORWIDTH - board.location.Width / board.cols) / 2, (int)destinationLoc.Y - (board.WARRIORHEIGHT - board.location.Height / board.rows) / 2 - board.location.Height / board.rows / 3, board.WARRIORWIDTH, board.WARRIORHEIGHT);
            ImageAtlas source = this.states[state];
            Color color = this.isYours ? Color.White : Color.LightSalmon;
            source.draw(spriteBatch, destination, direction, (int)stateDepth, color);
        }

        public void drawInArbitraryLocation(int x, int y)
        {
            SpriteBatch spriteBatch = this.game.spriteBatch;
            Rectangle destination = new Rectangle((int)x - (board.WARRIORWIDTH - board.location.Width / board.cols) / 2, (int)y - (board.WARRIORHEIGHT - board.location.Height / board.rows) / 2 - board.location.Height / board.rows / 3, board.WARRIORWIDTH, board.WARRIORHEIGHT);
            this.stateDepth = 0;
            this.state = State.walking;
            this.direction = Direction.S;
            ImageAtlas source = this.states[state];
            Color color = this.isYours ? Color.White : Color.Red;
            source.draw(spriteBatch, destination, this.direction, (int)this.stateDepth, color);
        }

        public void drawWarriorType(int x, int y)
        {
            this.game.spriteBatch.Begin();
            string selwarrior = this.type.ToUpperInvariant();
            this.game.spriteBatch.DrawString(this.game.infofont, selwarrior, new Vector2(x, y), Color.White);
            this.game.spriteBatch.End();
        }
        public void update(GameTime gameTime, GameMatch game)
        {
            int millis = gameTime.ElapsedGameTime.Milliseconds;
            if (curDelay <= 0)
            {
                stateDepth += millis * (double)this.states[state].cols / this.stateDurations[state];
                if (stateDepth > this.states[state].cols && (this.state == State.attack || this.state == State.beenHit))
                {
                    this.state = State.stopped;
                    if (game.turnProgress == TurnProgress.attacking)
                    {
                        this.game.attackInfoPane.isVisible = false;
                        game.turnProgress = TurnProgress.attacked;
                       
                    }

                }
                if (stateDepth > this.states[state].cols && this.health <= 0)
                {
                    if (this.state == State.tippingOver)
                    {
                        //death!
                        this.board.warriors[(int)this.row][(int)this.col] = null;
                        this.game.yourWarriors.Remove(this);
                        this.game.opponentWarriors.Remove(this);
                    }
                    this.curDelay = 500;
                    this.state = State.tippingOver;
                }

            }
            else
            {
                curDelay -= gameTime.ElapsedGameTime.Milliseconds;
                this.stateDepth = 0;
            }
            stateDepth = stateDepth % this.states[state].cols;
        }
        #region movement
        public void updateUserOptions(Boolean isYourTurn)
        {
            if (this.state == State.stopped)
            {

                Boolean[][] discovered = this.bredthFirst((int)this.row, (int)this.col, this.maxMove, passThroughTeam: true);
                for (int i = 0; i < discovered.Length; i++)
                {
                    for (int j = 0; j < discovered[i].Length; j++)
                    {
                        if (discovered[i][j] && board.warriors[i][j] == null)
                        {
                            board.tileTints[i][j] = (this.isYours && isYourTurn) ? Warrior.yourMoveColor : Warrior.notYourMoveColor;
                        }
                    }

                }
            }
            else if (this.state == State.attack)
            {
            }
        }
        public Boolean[][] bredthFirst(int startRow, int startCol, int maxDepth, Boolean ignoreWarriors = false, bool passThroughTeam = false)
        {
            
            List<BredthFirstNode> bfns = new List<BredthFirstNode>();
            bfns.Add(new BredthFirstNode(startRow, startCol, 0));
            Boolean[][] discovered = new Boolean[board.rows][];
            for (int i = 0; i < discovered.Length; i++)
            {
                discovered[i] = new Boolean[board.cols];
            }
            if (startRow < 0 || startRow >= this.board.rows || startCol < 0 || startCol >= this.board.cols)
            {
                return discovered;
            }
            discovered[startRow][startCol] = true;
            while (bfns.Count != 0)
            {
                BredthFirstNode bfn = bfns[0];
                bfns.RemoveAt(0);
                if (bfn.depth >= maxDepth)
                {
                    break;
                }
                int row = bfn.row;
                int col = bfn.col;
                int depth = bfn.depth;
                if (row + 1 < board.rows && !discovered[row + 1][col] && (board.warriors[row + 1][col] == null || ignoreWarriors ||
                    (board.warriors[row + 1][col] != null && board.warriors[row + 1][col].isYours && passThroughTeam)))
                {
                    //board.tileTints[row + 1][col] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row + 1][col] = true;
                    if (!ignoreWarriors || board.warriors[row + 1][col] == null)
                    {
                        bfns.Add(new BredthFirstNode(row + 1, col, depth + 1));
                    }
                }
                if (row - 1 > -1 && !discovered[row - 1][col] && (board.warriors[row - 1][col] == null || ignoreWarriors ||
                    (row - 1 > -1 && board.warriors[row - 1][col] != null && board.warriors[row - 1][col].isYours && passThroughTeam)))
                {
                    // board.tileTints[row - 1][col] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row - 1][col] = true;
                    if (!ignoreWarriors || board.warriors[row - 1][col] == null)
                    {
                        bfns.Add(new BredthFirstNode(row - 1, col, depth + 1));
                    }
                }
                if (col + 1 < board.cols && !discovered[row][col + 1] && (board.warriors[row][col + 1] == null || ignoreWarriors ||
                    (board.warriors[row][col + 1] != null && board.warriors[row][col + 1].isYours && passThroughTeam)))
                {
                    // board.tileTints[row][col + 1] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row][col + 1] = true;
                    if (!ignoreWarriors || board.warriors[row][col + 1] == null)
                    {
                        bfns.Add(new BredthFirstNode(row, col + 1, depth + 1));
                    }
                }
                if (col - 1 > -1 && !discovered[row][col - 1] && (board.warriors[row][col - 1] == null || ignoreWarriors ||
                    (board.warriors[row][col - 1] != null && board.warriors[row][col - 1].isYours && passThroughTeam)))
                {
                    //board.tileTints[row][col - 1] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row][col - 1] = true;
                    if (!ignoreWarriors || board.warriors[row][col - 1] == null)
                    {
                        bfns.Add(new BredthFirstNode(row, col - 1, depth + 1));
                    }
                }
            }
            return discovered;
        }

        public Boolean isValidMove(Board board, int moveRow, int moveCol)
        {
            return bredthFirst((int)this.row, (int)this.col, this.maxMove, passThroughTeam: true)[moveRow][moveCol] && this.board.warriors[moveRow][moveCol] == null;
        }
        public bool moveTo(int destRow, int destCol)
        {
            if (destRow == this.row && destCol == this.col)
            {
                return false;
            }
            this.state = State.walking;
            this.stateDepth = 0;
            this.destRow = destRow;
            this.destCol = destCol;
            this.currentStep = this.dijkstra((int)this.row, (int)this.col, destRow, destCol, passThroughTeam: true);
            return true;
        }

        public DijkstraNode dijkstra(int startRow, int startCol, int destRow, int destCol, bool passThroughTeam = false)
        {
            DijkstraNode start = new DijkstraNode(startRow, startCol, 0, null, null);
            List<DijkstraNode> dijkstras = new List<DijkstraNode>();
            dijkstras.Add(start);
            Boolean[][] discovered = new Boolean[board.rows][];
            for (int i = 0; i < discovered.Length; i++)
            {
                discovered[i] = new Boolean[board.cols];
            }
            discovered[startRow][startCol] = true;
            DijkstraNode destination = null;
            while (dijkstras.Count != 0)
            {
                DijkstraNode dijkstraNode = dijkstras[0];
                dijkstras.RemoveAt(0);
                if (dijkstraNode.row == destRow && dijkstraNode.col == destCol)
                {
                    destination = dijkstraNode;
                    break;
                }
                int row = dijkstraNode.row;
                int col = dijkstraNode.col;
                int depth = dijkstraNode.depth;
                if (row + 1 < board.rows && !discovered[row + 1][col] && (board.warriors[row + 1][col] == null ||
                    (board.warriors[row + 1][col] != null && board.warriors[row + 1][col].isYours == this.isYours && passThroughTeam)))
                {
                    //board.tileTints[row + 1][col] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row + 1][col] = true;
                    dijkstras.Add(new DijkstraNode(row + 1, col, depth + 1, dijkstraNode, null));
                }
                if (row - 1 > -1 && !discovered[row - 1][col] && (board.warriors[row - 1][col] == null ||
                    (board.warriors[row - 1][col] != null && board.warriors[row - 1][col].isYours == this.isYours && passThroughTeam)))
                {
                    // board.tileTints[row - 1][col] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row - 1][col] = true;
                    dijkstras.Add(new DijkstraNode(row - 1, col, depth + 1, dijkstraNode, null));
                }
                if (col + 1 < board.cols && !discovered[row][col + 1] && (board.warriors[row][col + 1] == null ||
                    (board.warriors[row][col + 1] != null && board.warriors[row][col + 1].isYours == this.isYours && passThroughTeam)))
                {
                    // board.tileTints[row][col + 1] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row][col + 1] = true;
                    dijkstras.Add(new DijkstraNode(row, col + 1, depth + 1, dijkstraNode, null));
                }
                if (col - 1 > -1 && !discovered[row][col - 1] && (board.warriors[row][col - 1] == null ||
                    (board.warriors[row][col - 1] != null && board.warriors[row][col - 1].isYours == this.isYours && passThroughTeam)))
                {
                    //board.tileTints[row][col - 1] = this.isYours ? Warrior.moveColor : Warrior.enemyMoveColor;
                    discovered[row][col - 1] = true;
                    dijkstras.Add(new DijkstraNode(row, col - 1, depth + 1, dijkstraNode, null));
                }
            }
            // update next's
            DijkstraNode curNode = destination;
            while (curNode.previous != null)
            {
                curNode.previous.next = curNode;
                curNode = curNode.previous;
            }

            return start;
        }

        /// <summary>
        /// updates coordinates of warrior, updates board
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns>Returns true if warrior has reached his destination</returns>
        public Boolean move(GameTime gameTime)
        {
            if (this.currentStep.next == null)
            {
                return true;
            }
            // find direction
            int xDiff = this.currentStep.next.col - this.currentStep.col;
            int yDiff = this.currentStep.next.row - this.currentStep.row;
            this.setDirection(xDiff, yDiff);
            int pastRow = (int)this.row;
            int pastCol = (int)this.col;
            this.board.warriors[(int)this.row][(int)this.col] = null;
            this.row += speed * gameTime.ElapsedGameTime.Milliseconds * yDiff / 1000;
            this.col += speed * gameTime.ElapsedGameTime.Milliseconds * xDiff / 1000;

            if ((yDiff == 1 && pastRow != (int)this.row) || (xDiff == 1 && pastCol != (int)this.col) ||
                (yDiff == -1 && this.row <= this.currentStep.next.row) || (xDiff == -1 && this.col <= this.currentStep.next.col))
            {
                this.currentStep = this.currentStep.next;
                this.row = currentStep.row;
                this.col = currentStep.col;
            }
            if (this.currentStep.next == null)
            {
                // we have reached destination
                this.state = State.stopped;
                this.row = currentStep.row;
                this.col = currentStep.col;
                this.board.warriors[(int)this.row][(int)this.col] = this;
                return true;
            }
            this.board.warriors[(int)this.row][(int)this.col] = this;



            return false;
        }

        public void setDirection(int xDiff, int yDiff)
        {
            // angle is in radians, always between 0 and pi
            yDiff *= -1;
            double angle = 360 * Math.Atan(((double)yDiff) / xDiff) / (2 * Math.PI);

            if (angle < 90 && xDiff < 0)
            {
                angle = angle + 180;
            }
            if (angle < 0)
            {
                angle += 360;
            }

            // now we want it to be clockwise, with 0 at 3pi/4
            angle = angle - 112.5;
            if (angle < 0)
            {
                angle += 360;
            }
            angle = 360 - angle;

            // now normalize to int 0 - 7
            int direction = (int)(8 * angle / (360));

            if (direction == 0)
            {
                this.direction = Direction.N;
            }
            else if (direction == 1)
            {
                this.direction = Direction.NE;
            }
            else if (direction == 2)
            {
                this.direction = Direction.E;
            }
            else if (direction == 3)
            {
                this.direction = Direction.SE;
            }
            else if (direction == 4)
            {
                this.direction = Direction.S;
            }
            else if (direction == 5)
            {
                this.direction = Direction.SW;
            }
            else if (direction == 6)
            {
                this.direction = Direction.W;
            }
            else if (direction == 7)
            {
                this.direction = Direction.NW;
            }
        }
        #endregion

        public void drawAttackRange(bool isInCustomizeArmy = false)
        {
            if (this.attackRange.HasValue)
            {
                // draw the attack range
                Boolean[][] cordsInRange = bredthFirst((int)this.row, (int)this.col, this.attackRange.Value, ignoreWarriors: true);
                for (int i = 0; i < cordsInRange.Length; i++)
                {
                    for (int j = 0; j < cordsInRange[i].Length; j++)
                    {
                        if (cordsInRange[i][j] && isStrikable((int)this.row, (int)this.col, i, j) && (i != (int)this.row || j != (int)this.col) && !isInCustomizeArmy)
                        {
                            board.tileTints[i][j] = Warrior.attackColor;
                        }
                        else if (cordsInRange[i][j] && isStrikable((int)this.row, (int)this.col, i, j) && (i != (int)this.row || j != (int)this.col) && isInCustomizeArmy)
                        {
                            if (i >= 5)
                            {
                                board.tileTints[i][j] = Warrior.attackColor;
                            }
                            else
                            {
                                board.tileTints[i][j] = Color.DarkRed;
                            }
                        }
                    }
                }
            }
            if (this.attackPoints != null)
            {
                // draw the points
                foreach (Point p in this.attackPoints)
                {
                    int row = (int)this.row + p.X;
                    int col = (int)this.col + p.Y;
                    if (row >= 0 && row < this.board.rows && col >= 0 && col < this.board.cols)
                    {
                        board.tileTints[row][col] = Warrior.attackColor;
                    }
                }
            }
        }
        public Boolean isStrikable(int startRow, int startCol, int endRow, int endCol)
        {
            if (this.attackRange.HasValue)
            {
                int xDiff = endCol - startCol;
                int yDiff = endRow - startRow;
                if (Math.Abs(xDiff) + Math.Abs(yDiff) <= this.attackRange.Value)
                {
                    Boolean switched = false;
                    if (Math.Abs(yDiff) > Math.Abs(xDiff))
                    {
                        switched = true;
                        swap(ref xDiff, ref yDiff);
                        swap(ref startRow, ref startCol);
                        swap(ref endRow, ref endCol);
                    }
                    if (xDiff < 0)
                    {
                        xDiff = -1 * xDiff;
                        yDiff = -1 * yDiff;
                        swap(ref startRow, ref endRow);
                        swap(ref startCol, ref endCol);
                    }
                    double slope = ((double)yDiff) / xDiff;
                    double curRow = startRow + .5;
                    for (int col = startCol + 1; col < endCol; col++)
                    {
                        curRow += slope;
                        if ((switched && this.board.warriors[col][(int)curRow] != null) || (!switched && this.board.warriors[(int)curRow][col] != null))
                        {
                            return false;
                        }
                    }
                    return true;
                }

            }
            if (this.attackPoints != null)
            {
                foreach (Point p in this.attackPoints)
                {
                    int row = p.X + startRow;
                    int col = p.Y + startCol;
                    if (row == endRow && col == endCol)
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public void beginAttack(int targetRow, int targetCol)
        {
            this.state = State.attack;
            this.stateDepth = 0;
            this.setDirection(targetCol - (int)this.col, targetRow - (int)this.row);
        }

        public void swap(ref int arg0, ref int arg1)
        {
            int temp = arg0;
            arg0 = arg1;
            arg1 = temp;
        }

        public void takeHit(int delay)
        {


            this.curDelay = delay;
            this.state = State.beenHit;
            this.stateDepth = 0;
        }
        public int strike(Warrior target)
        {
            //only the white mage can hit itself
            if ((target == this) && this.type != "white mage")
            {
                return 0;
            }
            // handle anyone else attacking
            else if (target != null)
            {
                //Random rand = new Random();
                //int damage = (rand.Next(this.attack / 2) + this.attack) * 20 / target.defense;
                //target.health -= damage;
                Random rand = new Random();
                int randomNum = rand.Next(0, 101);
                int damage = (this.attack * 20) / target.defense;
                bool doesHit = true;

                string attackSound = this.attackSound;
                SoundEffect effect;
                // effect = Content.Load<SoundEffect>(attackSound);
                //effect.Play();


                // bonus check
                if (this.warriorClass.indexOfAdvantageAgainst == target.warriorClass.index)
                {
                    damage = (int)(damage * 1.33);
                }
                int direction = target.getDirectionTo(this);
                if (direction == target.direction || direction == (target.direction + 1) % 8 || direction == (target.direction - 1) % 8)
                {
                    //if target is facing towards from attacking unit, 80% chance of hitting
                    doesHit = randomNum <= 70;
                }
                else if (direction == (target.direction + 4) % 8)
                {
                    //if target is facing away from attacking unit, does 1.5* damage and always hits
                    damage = (int)(damage * 1.33);
                    doesHit = true;
                }
                else
                {
                    //if target's side is towards attacking unit, 90% chance to hit
                    doesHit = randomNum <= 85;
                    
                }

                //accuracy check
                if (doesHit)
                {
                    target.health -= damage;
                    if (target.health > target.maxHealth)
                    {
                        target.health = target.maxHealth;
                    }
                    return damage;
                }
                return 0;
            }
            return 0;
            // handle death in motions
        }
        public int getDirectionTo(Warrior attacker)
        {
            int rowDiff = (int)(attacker.row - this.row);
            int colDiff = (int)(attacker.col - this.col);
            // angle is in radians, always between 0 and pi
            rowDiff *= -1;
            double angle = 360 * Math.Atan(((double)rowDiff) / colDiff) / (2 * Math.PI);

            if (angle < 90 && colDiff < 0)
            {
                angle = angle + 180;
            }
            if (angle < 0)
            {
                angle += 360;
            }

            // now we want it to be clockwise, with 0 at 3pi/4
            angle = angle - 112.5;
            if (angle < 0)
            {
                angle += 360;
            }
            angle = 360 - angle;

            // now normalize to int 0 - 7
            return (int)(8 * angle / (360));

            
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Warrior))
            {
                Warrior w = (Warrior)obj;
                return this.id == w.id;
            }
            return false;
        }



        public class BredthFirstNode
        {
            public int row { get; set; }
            public int col { get; set; }
            public int depth { get; set; }

            public BredthFirstNode(int row, int col, int depth)
            {
                this.row = row;
                this.col = col;
                this.depth = depth;
            }
        }
        public class DijkstraNode : BredthFirstNode
        {
            public DijkstraNode previous { get; set; }
            public DijkstraNode next { get; set; }

            public DijkstraNode(int row, int col, int depth, DijkstraNode previous, DijkstraNode next)
                : base(row, col, depth)
            {
                this.previous = previous;
                this.next = next;
            }
        }
    }
}