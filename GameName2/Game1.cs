#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input.Touch;
using System.Xml;
using MySql.Data.MySqlClient;
#endregion

namespace CapitalStrategy
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		public int ROWS = 10;
		public int COLS = 10;
		public int BOARDWIDTH = 600;
		public int BOARDHEIGHT = 600;
        public int SELECTED_WARRIOR_INFO_X = 610;
        public int SELECTED_WARRIOR_INFO_Y = 40;

		public int WARRIORWIDTH { get; set; }
		public int WARRIORHEIGHT { get; set; }

		public Texture2D red;
		public Texture2D white;
        public Texture2D charcoal;
		public Texture2D heartIcon;
		public Texture2D attackIcon;
		public Texture2D shieldIcon;
		public Texture2D moveIcon;
        public SpriteFont menufont;
        public SpriteFont infofont;
		GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch { get; set; }
		Texture2D background;
		Rectangle backgroundRec;
		public Board board { get; set; }
		WarriorType axestanShield;
		WarriorType whiteMage;
		WarriorType firedragon;
		WarriorType blueArcher;
		WarriorType crocy;
		WarriorType magier;
		Warrior selectedWarrior;
		Warrior currentTurnWarrior;
		public Boolean isYourTurn { get; set; }
		public int turnProgress { get; set; }
		public int targetCol { get; set; }
		public int targetRow { get; set; }
		public Warrior beingAttacked { get; set; }
        Warrior displayWarrior;
		MouseWrapper mouseState;
        public int gameState { get; set; }
        

        

		public Game1()
			: base()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = this.BOARDWIDTH + 400;
			graphics.PreferredBackBufferHeight = this.BOARDHEIGHT + 50;
			IsMouseVisible = true;
			Content.RootDirectory = "Content";
		}


        public void login()
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            string query = "SELECT * FROM users";
            if (db.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, db.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    System.Diagnostics.Debug.WriteLine(dataReader["username"]);
                    System.Diagnostics.Debug.WriteLine(dataReader["password"]);
                }

                //close Data Reader
                dataReader.Close();
            }
        }
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			this.WARRIORWIDTH = this.BOARDWIDTH * 2 / this.COLS;
			this.WARRIORHEIGHT = this.BOARDHEIGHT * 2 / this.ROWS;
			TouchPanel.EnabledGestures = GestureType.Tap;
			isYourTurn = true;

            this.gameState = GameState.login;
			this.turnProgress = TurnProgress.beginning;
			base.Initialize();
            login();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
            menufont = Content.Load<SpriteFont>("fonts/gamefont");
            infofont = Content.Load<SpriteFont>("fonts/fontTemp");

			background = Content.Load<Texture2D>("stars");
			red = Content.Load<Texture2D>("colors/red");
			white = Content.Load<Texture2D>("colors/white");
            charcoal = Content.Load<Texture2D>("colors/charcoal");
			heartIcon = Content.Load<Texture2D>("icons/heartIcon");
			attackIcon = Content.Load<Texture2D>("icons/attackIcon");
			shieldIcon = Content.Load<Texture2D>("icons/shieldIcon");
			moveIcon = Content.Load<Texture2D>("icons/moveIcon");
			backgroundRec = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
			Texture2D tileImage = Content.Load<Texture2D>("floortileatlas.jpg");
			board = new Board(ROWS, COLS, this.BOARDWIDTH, this.BOARDHEIGHT, tileImage);
			// Game1 game, int maxHealth, int attack, int defense, int accuracy, int evade, int maxMove, double speed, String type, int[] imageDimensions, int[] stateDurations, Point[] attackPoints, int? attackRange, int attackDelayConst, int attackDelayRate)

            XmlTextReader reader = new XmlTextReader("Configuration/WarriorTypes.xml");

			axestanShield = new WarriorType(
                this,       // game
                80,         // maxhealth
                50,         // attack
                40,         // defense
				100,        // accuracy
                30,         // evade
                3,          // maxMove
                3,          // speed
                "axestan shield", // type 
				"blue archer", new int[] { 1, 8, 8, 13, 7, 9, 7 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 }, 
				null, 1, 500, 0);
			firedragon = new WarriorType(this, 60, 40, 70,
				80, 60, 4, 5, "firedragon", 
				"crocy", new int[] { 1, 7, 7, 9, 1, 11, 7 }, new int[] { 1000, 400, 1000, 1000, 1000, 1000, 1000 },
				null, 2, 500, 0);
			blueArcher = new WarriorType(this, 70, 40, 50,
				75, 20, 4, 3, "blue archer", 
				"magier", new int[] { 1, 8, 8, 13, 9, 13, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
				null, 6, 500, 10);
			this.whiteMage = new WarriorType(this, 50, -80, 30,
				100, 0, 4, 3, "white mage", 
				null, new int[] { 1, 8, 8, 13, 9, 13, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 }, 
				new Point[] { new Point(-1,-1), new Point(1,1), 
					new Point(1,-1), new Point(-1,1), new Point(-2, 0), 
					new Point(2, 0), new Point(0, 2), new Point(0, -2), 
					new Point(1,0), new Point(-1,0),new Point(0,1),
					new Point(0,-1),new Point(0,0)}, 
				null, 500, 0);
			crocy = new WarriorType(this, 90, 60, 50,
				50, 50, 2, 2, "crocy", 
				"firedragon", new int[] { 1, 8, 8, 11, 9, 11, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
				null, 1, 500, 0);
			magier = new WarriorType(this, 70, 40, 45,
				75, 25, 3, 3, "magier", 
				"axestan shield", new int[] { 9, 7, 7, 9, 9, 10, 9 }, new int[] { 1000, 500, 1000, 1500, 1000, 1000, 1000 },
				null, 3, 500, 10);

            PlayerArmy p1 = new PlayerArmy(Direction.N);
            p1.AddWarrior(axestanShield); 
            p1.AddWarrior(whiteMage);
            p1.AddWarrior(firedragon);
            p1.AddWarrior(crocy);
            p1.AddWarrior(magier);
            p1.AddWarrior(blueArcher);

            PlayerArmy p2 = new PlayerArmy(Direction.S);
            p2.AddWarrior(axestanShield);
            p2.AddWarrior(whiteMage);
            p2.AddWarrior(blueArcher);
            p2.AddWarrior(crocy);
            p2.AddWarrior(magier);
			for (int i = 0; i < 2; i++)
			{
				board.warriors[i == 0 ? 7 : 9 - 7][3] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 3, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, axestanShield);
				board.warriors[i == 0 ? 7 : 9 - 7][5] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 5, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, axestanShield);
				board.warriors[i == 0 ? 9 : 9 - 9][5] = new Warrior(this.board, i == 0 ? 9 : 9 - 9, 5, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, whiteMage);
				board.warriors[i == 0 ? 9 : 9 - 9][2] = new Warrior(this.board, i == 0 ? 9 : 9 - 9, 2, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, whiteMage);
				board.warriors[i == 0 ? 6 : 9 - 6][7] = new Warrior(this.board, i == 0 ? 6 : 9 - 6, 7, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, firedragon);
				board.warriors[i == 0 ? 7 : 9 - 7][6] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 6, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, blueArcher);
				board.warriors[i == 0 ? 7 : 9 - 7][4] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 4, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, crocy);
				board.warriors[i == 0 ? 8 : 9 - 8][3] = new Warrior(this.board, i == 0 ? 8 : 9 - 8, 3, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, magier);
				board.warriors[i == 0 ? 8 : 9 - 8][5] = new Warrior(this.board, i == 0 ? 8 : 9 - 8, 5, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, magier);
				board.warriors[i == 0 ? 7 : 9 - 7][2] = new Warrior(this.board, i == 0 ? 7 : 9 - 7, 2, i == 0 ? Direction.N : Direction.S, State.stopped, i == 0, blueArcher);
			}


			mouseState = new MouseWrapper(board, Mouse.GetState());
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (this.currentTurnWarrior != null)
			{
				if (this.turnProgress == TurnProgress.moving)
				{
					Boolean finishedMoving = currentTurnWarrior.move(gameTime);
					if (finishedMoving)
					{
						this.turnProgress = TurnProgress.moved;
					}
				}
				if (this.turnProgress == TurnProgress.moved)
				{
					this.currentTurnWarrior.drawAttackRange();
				}
				if (this.turnProgress == TurnProgress.targetAcquired)
				{
					this.board.tileTints[this.targetRow][this.targetCol] = Warrior.targetAcquiredColor;
				}
				if (this.turnProgress == TurnProgress.attacked)
				{
					// calculate damage
					this.currentTurnWarrior.strike(this.beingAttacked);
					this.turnProgress = TurnProgress.beginning;
					this.isYourTurn = !this.isYourTurn;
				}
			}
			mouseState.update(Mouse.GetState());
			if (mouseState.wasClicked() && mouseState.isOverGrid)
			{
				this.handleClickOverGrid();
			}
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// TODO: Add your update logic here
			for (int row = 0; row < board.warriors.Length; row++)
			{
				for (int col = 0; col < board.warriors[row].Length; col++)
				{
					Warrior warrior = board.warriors[row][col];
					if (warrior != null)
					{
						warrior.update(gameTime, this);
					}

				}
			}

            if (displayWarrior != null)
            {
                displayWarrior.update(gameTime, this);
            }
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here
			spriteBatch.Begin();
			spriteBatch.Draw(background, backgroundRec, Color.White);
			spriteBatch.End();
			board.drawTiles(spriteBatch);
			for (int row = 0; row < board.warriors.Length; row++)
			{
				for (int col = 0; col < board.warriors[row].Length; col++)
				{
					Warrior warrior = board.warriors[row][col];
					if (warrior != null)
					{
						warrior.draw();
					}
				}
			}
			if (mouseState.isOverGrid && board.warriors[mouseState.row][mouseState.col] != null)
			{
				this.drawHealthBar(mouseState.row, mouseState.col);
			}
			this.drawInfoFrame(this.selectedWarrior);

            this.spriteBatch.Begin();
            string turnInfo = (this.isYourTurn) ? "It is your turn" : "Waiting for opponent";
            this.spriteBatch.DrawString(this.infofont, turnInfo, new Vector2(SELECTED_WARRIOR_INFO_X, SELECTED_WARRIOR_INFO_Y), Color.White);
            this.spriteBatch.End();


			base.Draw(gameTime);
		}

		public void handleClickOverGrid()
		{
			this.board.resetTints();
			this.board.tileTints[mouseState.row][mouseState.col] = Color.LightBlue;
            // Draw right side panel information about this warrior

			if (this.turnProgress == TurnProgress.beginning)
			{

				if (selectedWarrior != null && ((this.isYourTurn && this.selectedWarrior.isYours) || (!this.isYourTurn && !this.selectedWarrior.isYours)))
				{
					// find if this is a valid move
					if (selectedWarrior.isValidMove(board, mouseState.row, mouseState.col))
					{
						selectedWarrior.moveTo(mouseState.row, mouseState.col);
						this.turnProgress = TurnProgress.moving;
						this.currentTurnWarrior = selectedWarrior;
					}
				}
				this.selectedWarrior = board.warriors[mouseState.row][mouseState.col];
				if (this.selectedWarrior != null)
				{
					System.Diagnostics.Debug.WriteLine("here");
					selectedWarrior.updateUserOptions(this.isYourTurn);
				}
			}
			else if (this.turnProgress == TurnProgress.moved)
			{
				// clicking to acquire target
				if (this.currentTurnWarrior.isStrikable((int)this.currentTurnWarrior.row, (int)this.currentTurnWarrior.col, this.mouseState.row, this.mouseState.col))
				{
					this.turnProgress = TurnProgress.targetAcquired;
					this.targetRow = this.mouseState.row;
					this.targetCol = this.mouseState.col;
				}
			}
			else if (this.turnProgress == TurnProgress.targetAcquired)
			{
				if (this.mouseState.row == this.targetRow && this.mouseState.col == this.targetCol)
				{
					this.turnProgress = TurnProgress.attacking;
					this.beingAttacked = this.board.warriors[this.targetRow][this.targetCol];
					this.currentTurnWarrior.beginAttack(this.targetRow, this.targetCol);
					if (beingAttacked != null) {
						int xDiff = (int)(currentTurnWarrior.col - beingAttacked.col);
						int yDiff = (int)(currentTurnWarrior.row - beingAttacked.row);
						beingAttacked.setDirection (xDiff, yDiff);
						beingAttacked.takeHit (currentTurnWarrior.getAttackDelay (xDiff, yDiff));
					} else {
						return;
					}
				}
				else {
					this.turnProgress = TurnProgress.moved;
					return;
				}
			}



		}

		public int drawHealthBar(int row, int col)
		{
			Warrior warrior = this.board.warriors[row][col]; // aleady checked if it was null
			// want to display health as bar across top -- two rectangles, white and red
			int tileWidth = this.BOARDWIDTH / this.board.cols;
			int tileHeight = this.BOARDHEIGHT / this.board.rows;
			int xLoc = (int)(warrior.col * tileWidth);
			int yLoc = (int)(warrior.row * tileHeight);
			int healthBarY = yLoc - tileHeight / 2; // most warriors are taller than the tile
			// then attack, defense and move
			double widthPerHP = ((double)tileWidth) / 100;
			this.spriteBatch.Begin();
            this.spriteBatch.Draw(charcoal, new Rectangle(xLoc - 2, healthBarY - 2, (int)(widthPerHP * warrior.maxHealth) + 4, tileHeight / 10 + 4), Color.WhiteSmoke);
			this.spriteBatch.Draw(white, new Rectangle(xLoc, healthBarY, (int)(widthPerHP * warrior.maxHealth), tileHeight / 10), Color.WhiteSmoke);
			this.spriteBatch.Draw(red, new Rectangle(xLoc, healthBarY, (int)(widthPerHP * warrior.health), tileHeight / 10), Color.WhiteSmoke);
			this.spriteBatch.End();
			return healthBarY + tileHeight / 10;
		}
		public void drawInfoFrame(Warrior selectedWarrior)
		{
			if (selectedWarrior == null)
			{
				return;
			}
			int row = (int)selectedWarrior.row;
			int col = (int)selectedWarrior.col;
			int tileWidth = this.BOARDWIDTH / this.board.cols;
			int tileHeight = this.BOARDHEIGHT / this.board.rows;
			int xLoc = (int)(selectedWarrior.col * tileWidth);
			int yLoc = (int)(selectedWarrior.row * tileHeight);
			int bottomOfHealthBar = this.drawHealthBar(row, col);
			int totalDistance = yLoc + tileHeight - bottomOfHealthBar;
			// we have 3 things to draw, so split evenly
			int padding = (int)(.1 * totalDistance / 6);
			int iconHeight = (int)(.9 * totalDistance / 3);
			int toDrawY = bottomOfHealthBar + padding;
			int toDrawX = (int)(xLoc + tileWidth);
			int iconWidth = tileWidth / 4;
			double widthPerPoint = ((double)tileWidth) / 100;
            int barHeight = tileHeight / 10;

            /*
			this.spriteBatch.Begin();
			this.spriteBatch.Draw(attackIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
            this.spriteBatch.Draw(charcoal, new Rectangle(toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
			this.spriteBatch.Draw(white, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
			this.spriteBatch.Draw(red, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.attack), barHeight), Color.WhiteSmoke);
			toDrawY += iconHeight + padding;
			this.spriteBatch.Draw(shieldIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
            this.spriteBatch.Draw(charcoal, new Rectangle(toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
			this.spriteBatch.Draw(white, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
			this.spriteBatch.Draw(red, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.defense), barHeight), Color.WhiteSmoke);
			toDrawY += iconHeight + padding;



			//this.spriteBatch.Draw(moveIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
			//this.spriteBatch.Draw(white, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
			//this.spriteBatch.Draw(red, new Rectangle(toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.maxMove * 10), barHeight), Color.WhiteSmoke);
			this.spriteBatch.End();
            */


            if (this.selectedWarrior != null)
            {
                displayWarrior = new Warrior(this.selectedWarrior);
                int imgPadding = 40;
                toDrawX = SELECTED_WARRIOR_INFO_X;
                toDrawY = SELECTED_WARRIOR_INFO_Y + 80;
                displayWarrior.drawInArbitraryLocation(toDrawX, toDrawY);
                displayWarrior.drawWarriorType(toDrawX + this.WARRIORWIDTH / 2 + imgPadding, toDrawY - imgPadding);
                this.spriteBatch.Begin();
                toDrawX = toDrawX + this.WARRIORWIDTH / 2 + imgPadding;

                // Draw the warriors health
                this.spriteBatch.Draw(heartIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.Draw(charcoal, new Rectangle(2 + toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * selectedWarrior.maxHealth) + 4, barHeight + 4), Color.WhiteSmoke);
                this.spriteBatch.Draw(white, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.maxHealth), barHeight), Color.WhiteSmoke);
                this.spriteBatch.Draw(red, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.health), barHeight), Color.WhiteSmoke);
                toDrawY += iconHeight + padding;

                // Draw the warriors attack strength
                this.spriteBatch.Draw(attackIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.Draw(charcoal, new Rectangle(2+ toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
                this.spriteBatch.Draw(white, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
                this.spriteBatch.Draw(red, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.attack), barHeight), Color.WhiteSmoke);
                toDrawY += iconHeight + padding;

                // Draw the warriors defense strength
                   // @TODO: Change the 2+ on this to be a resolution-independent value
                this.spriteBatch.Draw(shieldIcon, new Rectangle(toDrawX, toDrawY, iconWidth, iconHeight), Color.White);
                this.spriteBatch.Draw(charcoal, new Rectangle(2 + toDrawX + iconWidth + padding - 2, toDrawY + iconHeight / 2 - barHeight / 2 - 2, (int)(widthPerPoint * 100) + 4, barHeight + 4), Color.WhiteSmoke);
                this.spriteBatch.Draw(white, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * 100), barHeight), Color.WhiteSmoke);
                this.spriteBatch.Draw(red, new Rectangle(2 + toDrawX + iconWidth + padding, toDrawY + iconHeight / 2 - barHeight / 2, (int)(widthPerPoint * selectedWarrior.defense), barHeight), Color.WhiteSmoke);
                toDrawY += iconHeight + padding;
                this.spriteBatch.End();
            }


		}
	}
}
