using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CapitalStrategy.Windows;

namespace CapitalStrategy
{
	/// <summary>
	/// Describes a general type of warrior
	/// </summary>
	public class WarriorType
	{
		String[] stateStrings = { "stopped", "walking", "running", "attack", "been hit", "tipping over", "talking" };
		public GameMatch game { get; set; }

		public int maxHealth { get; set; }
		public int maxCooldown { get; set;}
		public int attack { get; set; }
		public int defense { get; set; }
		public int accuracy { get; set;}
		public int evade { get; set;}
		public int maxMove { get; set; }
		public double speed { get; set; }
		public ImageAtlas[] states { get; set; }
		public String type { get; set; }
        public WarriorClass warriorClass { get; set; }
		public int[] stateDurations { get; set; } // time to complete a state (in millis)
		public Point[] attackPoints { get; set; }
		public int? attackRange { get; set; }
		public int attackDelayConst { get; set; }
		public int attackDelayRate { get; set; }
        public String attackSound { get; set; }
        public String description;
		// note: will need to add way of showing attack area

		public WarriorType(GameMatch game, int maxHealth, int maxCooldown, int attack, int defense, int accuracy, int evade, int maxMove, double speed, String type, WarriorClass warriorClass, int[] imageDimensions, int[] stateDurations, Point[] attackPoints, int? attackRange, int attackDelayConst, int attackDelayRate, String attackSound, String description)
		{
			this.game = game;
			this.maxHealth = maxHealth;
			this.maxCooldown = maxCooldown;
			this.attack = attack;
			this.defense = defense;
			this.accuracy = accuracy;
			this.evade = evade;
			this.maxMove = maxMove;
			this.speed = speed;
			this.type = type;
            this.warriorClass = warriorClass;
			this.states = loadStates(type, imageDimensions);
			this.stateDurations = stateDurations;
			this.attackPoints = attackPoints;
			this.attackRange = attackRange;
			this.attackDelayConst = attackDelayConst;
			this.attackDelayRate = attackDelayRate;
            this.attackSound = "soundEffects/" + attackSound;
            this.description = description;
		}

		public WarriorType(WarriorType warriorType)
		{
			this.game = warriorType.game;
			this.maxHealth = warriorType.maxHealth;
			this.maxCooldown = warriorType.maxCooldown;
			this.attack = warriorType.attack;
			this.defense = warriorType.defense;
			this.accuracy = warriorType.accuracy;
			this.evade = warriorType.evade;
			this.maxMove = warriorType.maxMove;
			this.speed = warriorType.speed;
			this.type = warriorType.type;
            this.warriorClass = warriorType.warriorClass;
			this.states = warriorType.states;
			this.stateDurations = warriorType.stateDurations;
			this.attackPoints = warriorType.attackPoints;
			this.attackRange = warriorType.attackRange;
			this.attackDelayConst = warriorType.attackDelayConst;
			this.attackDelayRate = warriorType.attackDelayRate;
            this.attackSound = warriorType.attackSound;
		}

		public ImageAtlas[] loadStates(String type, int[] imageDimensions)
		{
			ImageAtlas[] states = new ImageAtlas[stateStrings.Length];
			for (int i = 0; i < stateStrings.Length; i++)
			{
				String fileName = "sprites/"+type+"/"+type + " " + stateStrings[i] + ".png";
				Texture2D texture = this.game.Content.Load<Texture2D>(fileName);
				states[i] = new ImageAtlas(texture, 8, imageDimensions[i], 0);
			}


			return states;
		}

		public int getAttackDelay(int diffX, int diffy)
		{
			diffX = Math.Abs(diffX);
			diffy = Math.Abs(diffy);
			double distance = Math.Sqrt(diffX * diffX + diffy * diffy);
			return (int)(this.attackDelayConst + distance * this.attackDelayRate);
		}
	}
}
