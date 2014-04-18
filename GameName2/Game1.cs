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
using CapitalStrategy.Windows;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Diagnostics;
using CapitalStrategyServer.Messaging;
using CapitalStrategyServer;
using MySql.Data.MySqlClient;
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
        public static Stack<int> gameStates { get; set; }
        public String username { get; set; }
        public String password { get; set; }

        public static SpriteFont gameFont;
        public static SpriteFont menuFont;
        public static SpriteFont smallFont;
        public static Texture2D inputPaneImage;
        public static Texture2D button;
        public static Texture2D background;
        public static Texture2D backButton;
        public static Texture2D tileImage;
        public static Texture2D charcoal;
        public static Texture2D infoBackground { get; set; }
        public NetClient client;
        public WarriorType[] warriorTypes { get; set; }
        public WarriorClass[] warriorClasses { get; set; }
        public Client otherPlayer;
        public GameMatch gameMatch;
        public Messaging.Messaging msgManager;

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

            NetPeerConfiguration config = new NetPeerConfiguration("xnaapp");
            client = new NetClient(config);

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
            Game1.gameStates = new Stack<int>();
			
            this.windows = new Windows.Window[GameState.totalStates];
            this.windows[GameState.login] = new Windows.Login(this);
            this.gameMatch = new Windows.GameMatch(this);
            this.windows[GameState.gameMatch] = this.gameMatch;
            this.windows[GameState.mainMenu] = new Windows.MainMenu(this);
            this.windows[GameState.customizeArmy] = new Windows.CustomizeArmy(this);


            foreach (Windows.Window window in windows)
            {
                if (window != null)
                {

                    window.Initialize();
                }
            }
			base.Initialize();

            client.Start();
            Debug.WriteLine("Client 1: " + client.UniqueIdentifier.ToString());
            msgManager = new Messaging.Messaging(client, this.username);
            msgManager.game = this;
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
            Game1.button = Content.Load<Texture2D>("GUI/button");
            Game1.backButton = Content.Load<Texture2D>("GUI/back_button");
            Game1.background = Content.Load<Texture2D>("login/loginBackground");
            Game1.tileImage = Content.Load<Texture2D>("floortileatlas.jpg");
            Game1.charcoal = Content.Load<Texture2D>("colors/charcoal");
            Game1.infoBackground = Content.Load<Texture2D>("GUI/info_background2.png");

            Game1.gameFont = Content.Load<SpriteFont>("fonts/gamefont");
            Game1.menuFont = Content.Load<SpriteFont>("fonts/menufont");
            Game1.smallFont = Content.Load<SpriteFont>("fonts/smallFont");
           
            foreach (Windows.Window window in windows)
            {
                if (window != null)
                {
                    window.LoadContent();
                }
            }
            GameMatch game = (GameMatch)this.windows[GameState.gameMatch];
            this.warriorClasses = this.loadWarriorClasses();
            this.warriorTypes = this.loadWarriorTypes();
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

            if (client.Status == NetPeerStatus.Running)
            {
                NetIncomingMessage msg;
                while ((msg = client.ReadMessage()) != null)
                {
                    msgManager.handleIncomingMessage(msg);
                }
            }

            this.msgManager.update();
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

        

        // returns list of warrior classes populated from db
        public WarriorClass[] loadWarriorClasses()
        {
            // first get count of warrior classes
            int numClasses = this.countTableRows("warrior_classes");

            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                // under assumption results are indexed 0,1,2.. with no numbers missing
                
                WarriorClass[] warriorClasses = new WarriorClass[numClasses];
                string query = "SELECT warrior_class_id, warrior_class_name, warrior_class_advantage_id FROM warrior_classes ORDER BY warrior_class_id ASC";
                MySqlCommand cmd = new MySqlCommand(query, db.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    int warriorClassId = Int32.Parse(dataReader["warrior_class_id"].ToString());
                    string warriorClassName = dataReader["warrior_class_name"].ToString();
                    int warriorClassAdvantageIndex = Int32.Parse(dataReader["warrior_class_advantage_id"].ToString());
                    WarriorClass wc = new WarriorClass(warriorClassId, warriorClassName, warriorClassAdvantageIndex);
                    warriorClasses[warriorClassId] = wc;
                }

                //close Data Reader
                dataReader.Close();
                return warriorClasses;
            }
            return null;
        }

        public int countTableRows(string tableName)
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                String query = "SELECT COUNT(*) as count FROM " + tableName;
                MySqlCommand cmd = new MySqlCommand(query, db.connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if (dataReader.Read())
                {
                    int count = Int32.Parse(dataReader["count"].ToString());
                    dataReader.Close();
                    return count;
                }
            }
            return 0;
        }

        // returns list of warrior types populated from db
        public WarriorType[] loadWarriorTypes()
        {
            // first get count of warrior classes
            int numTypes = this.countTableRows("warrior_types");

            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                // under assumption results are indexed 0,1,2.. with no numbers missing

                WarriorType[] warriorTypes = new WarriorType[numTypes];
                string query = @"SELECT warrior_type_id, GROUP_CONCAT( depth ) as depths , GROUP_CONCAT( duration ) as durations,
                                    name, img_name, max_health, attack, defense, cooldown, accuracy, evade, move_range, move_speed,
                                    warrior_class_id, attack_range, attack_delay_const, attack_delay_rate, attack_sound, description
                                 FROM warrior_types AS wt
                                 NATURAL JOIN warrior_type_states
                                 GROUP BY wt.warrior_type_id
                ";
                MySqlCommand cmd = new MySqlCommand(query, db.connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    WarriorClass warriorClass = this.warriorClasses[Int32.Parse(dataReader["warrior_class_id"].ToString())];
                    string[] depthStrings = dataReader["depths"].ToString().Split(new char[]{','});
                    string[] durationStrings = dataReader["durations"].ToString().Split(new char[] { ',' });
                    int[] depths = new int[depthStrings.Length];
                    int[] durations = new int[durationStrings.Length];
                    for (int i = 0; i < depthStrings.Length; i++)
                    {
                        depths[i] = Int32.Parse(depthStrings[i]);
                        durations[i] = Int32.Parse(durationStrings[i]);
                    }
                    //img_name, max_health, attack, defense, cooldown, accuracy, evade, move_range, move_speed,
                     //               warrior_class_id, attack_range, attack_delay_const, attack_delay_rate, attack_sound
                    string imgName = dataReader["img_name"].ToString();
                    int maxHealth = Int32.Parse(dataReader["max_health"].ToString());
                    int attack = Int32.Parse(dataReader["attack"].ToString());
                    int defense = Int32.Parse(dataReader["defense"].ToString());
                    int cooldown = Int32.Parse(dataReader["cooldown"].ToString());
                    int accuracy = Int32.Parse(dataReader["accuracy"].ToString());
                    int evade = Int32.Parse(dataReader["evade"].ToString());
                    int moveRange = Int32.Parse(dataReader["move_range"].ToString());
                    int moveSpeed = Int32.Parse(dataReader["move_speed"].ToString());
                    int attackRange = Int32.Parse(dataReader["attack_range"].ToString());
                    int attackDelayConst = Int32.Parse(dataReader["attack_delay_const"].ToString());
                    int attackDelayRate = Int32.Parse(dataReader["attack_delay_rate"].ToString());
                    string attackSound = dataReader["attack_sound"].ToString();
                    string description = dataReader["description"].ToString();
                    WarriorType wt = new WarriorType(this.gameMatch, maxHealth, cooldown, attack, defense, accuracy, evade, moveRange,
                        moveSpeed, imgName, warriorClass, depths, durations, null, attackRange, attackDelayConst, attackDelayRate, attackSound, description);
                    warriorTypes[Int32.Parse(dataReader["warrior_type_id"].ToString())-1] = wt;
                }

                //close Data Reader
                dataReader.Close();
                return warriorTypes;
            }
            return null;
        }


        public string confirmPassword { get; set; }
    }
}
