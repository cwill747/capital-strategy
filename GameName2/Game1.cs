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

#endregion

namespace CapitalStrategy
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{

		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch { get; set; }

		
        public int gameState { get; set; }
        public String username { get; set; }
        public String password { get; set; }

        public static SpriteFont gameFont;
        public static SpriteFont menuFont;
        public static SpriteFont smallFont;
        public static Texture2D inputPaneImage;
        public static Texture2D button;
        public static Texture2D background;
        

        // List of windows. GameState class determines index of current window
        public Windows.Window[] windows { get; set; }
        

		public Game1()
			: base()
		{
			this.graphics = new GraphicsDeviceManager(this);
			this.graphics.PreferredBackBufferWidth = 600 + 400;
			this.graphics.PreferredBackBufferHeight = 600 + 50;
			IsMouseVisible = true;
			Content.RootDirectory = "Content";
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
			
			TouchPanel.EnabledGestures = GestureType.Tap;
			
            this.gameState = GameState.login;
			
            this.windows = new Windows.Window[GameState.totalStates];
            this.windows[GameState.login] = new Windows.Login(this);
            this.windows[GameState.gameMatch] = new Windows.GameMatch(this);
            this.windows[GameState.mainMenu] = new Windows.MainMenu(this);

            foreach (Windows.Window window in windows)
            {
                if (window != null)
                {

                    window.Initialize();
                }
            }
			base.Initialize();
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
            Game1.inputPaneImage = Content.Load<Texture2D>("GUI/inputPane");
            Game1.smallFont = Content.Load<SpriteFont>("fonts/smallFont");
            Game1.button = Content.Load<Texture2D>("GUI/button");
            Game1.background = Content.Load<Texture2D>("login/loginBackground");
            Game1.gameFont = Content.Load<SpriteFont>("fonts/gamefont");
            Game1.menuFont = Content.Load<SpriteFont>("fonts/menufont");
           
            foreach (Windows.Window window in windows)
            {
                if (window != null)
                {
                    window.LoadContent();
                }
            }

            
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
            windows[gameState].Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
            windows[gameState].Draw();
			base.Draw(gameTime);
		}

	}
}
