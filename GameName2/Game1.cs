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
        public NetClient client;
        public List<WarriorType> warriorTypes = new List<WarriorType>();
        public Client otherPlayer;
        public GameMatch gameMatch;

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
            warriorTypes.Add(new WarriorType(
                game,       // game
                80,         // maxhealth
				2,			// maxCool (number of turns + 1)
                50,         // attack
                40,         // defense
                100,        // accuracy
                30,         // evade
                3,          // maxMove
                3,          // speed
                "axestan shield", // type 
                "melee",
                "ranged", new int[] { 1, 8, 8, 13, 7, 9, 7 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                null, 1, 500, 0, "swordStrike"));
			warriorTypes.Add(new WarriorType(game, 60, 3, 40, 70,
                80, 60, 4, 5, "firedragon", "magic",
                "melee", new int[] { 1, 7, 7, 9, 1, 11, 7 }, new int[] { 1000, 400, 1000, 1000, 1000, 1000, 1000 },
                null, 2, 500, 0, "dragonRoar"));
			warriorTypes.Add(new WarriorType(game, 70, 3, 40, 50,
                75, 20, 4, 3, "blue archer", "ranged",
                "magic", new int[] { 1, 8, 8, 13, 9, 13, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                null, 6, 500, 10, "bowShot"));
			warriorTypes.Add(new WarriorType(game, 50, 2, -80, 30,
                100, 0, 4, 3, "white mage", "magic",
                "none", new int[] { 1, 8, 8, 13, 9, 13, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                new Point[] { new Point(-1,-1), new Point(1,1), 
					new Point(1,-1), new Point(-1,1), new Point(-2, 0), 
					new Point(2, 0), new Point(0, 2), new Point(0, -2), 
					new Point(1,0), new Point(-1,0),new Point(0,1),
					new Point(0,-1),new Point(0,0)},
                null, 500, 0, "heal"));
			warriorTypes.Add(new WarriorType(game, 90, 2, 60, 50,
                50, 50, 2, 2, "crocy", "melee",
                "ranged", new int[] { 1, 8, 8, 11, 9, 11, 9 }, new int[] { 1000, 700, 1000, 1000, 1000, 1000, 1000 },
                null, 1, 500, 0, "swordStrike"));
			warriorTypes.Add(new WarriorType(game, 70, 2, 40, 45,
                75, 25, 3, 3, "magier","magic",
                "melee", new int[] { 9, 7, 7, 9, 9, 10, 9 }, new int[] { 1000, 500, 1000, 1500, 1000, 1000, 1000 },
                null, 3, 500, 10, "thunder"));

            
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
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            if (status == NetConnectionStatus.Connected)
                            {
                                Console.WriteLine("Connected! UID: " + msg.SenderConnection.RemoteUniqueIdentifier + " , IP: " + msg.SenderConnection.RemoteEndPoint.ToString());
                            }
                            break;
                        case NetIncomingMessageType.Data:
                            msgType type = (msgType)msg.ReadInt32();
                            Message m;
                            if (type == msgType.Chat)
                            {
                                long sentFrom = msg.ReadInt64();
                                long sendToUUID = msg.ReadInt64();
                                string message = msg.ReadString();
                                m = new Message(type, sentFrom, sendToUUID);
                                m.msg = message;

                                if (m.msg == "SERVER HELLO")
                                {
                                    NetOutgoingMessage om = client.CreateMessage();
                                    om.WritePadBits();
                                    Message clientHello = new Message(msgType.Chat, client.UniqueIdentifier, client.ServerConnection.RemoteUniqueIdentifier);
                                    clientHello.msg = "CLIENT HELLO:"+ this.username;
                                    clientHello.handleMessage(ref om);
                                    Console.WriteLine("Sending message: " + clientHello.ToString());
                                    client.SendMessage(om, NetDeliveryMethod.ReliableUnordered);



                                }
                            }
                            else if (type == msgType.Matchmaking)
                            {
                                long sentFrom = msg.ReadInt64();
                                long sentTo = msg.ReadInt64();
                                string message = msg.ReadString();
                                m = new Message(type, sentFrom, sentTo);
                                m.msg = message;
                                if (m.msg != "SEEKING")
                                {
                                    // I have found a new match
                                    otherPlayer = new Client(m.sentFrom, true, false, m.msg.Substring(0, m.msg.IndexOf(':')));
                                    bool isMyTurn = Boolean.Parse(m.msg.Substring(m.msg.IndexOf(':')+1));
                                    Console.WriteLine("Opponent found: " + otherPlayer.username);
                                    this.gameState = GameState.gameMatch;
                                    this.windows[GameState.gameMatch].Initialize();
                                    this.gameMatch.isYourTurn = isMyTurn;
                                    Game1.gameStates.Push(GameState.mainMenu);
                                }
                            }
                            else
                            {
                                m = new Message(
                                    type,
                                    msg.ReadInt64(), // UID of the client the message was sent from
                                    msg.ReadInt64(), // UID of the client the message is sent to (should be us)
                                    new int[2] { msg.ReadInt32(), msg.ReadInt32() }, // The end location of the piece moved
                                    new int[2] { msg.ReadInt32(), msg.ReadInt32() }, // Where the piece attacked
                                    msg.ReadInt32(), // The damage dealt
                                    msg.ReadInt32(), //The attacked unit ID
                                    msg.ReadInt32(), // The attacker unit ID
                                    msg.ReadBoolean() // Whether the unit died or not
                                );

                                /*
                                 *             m.Write(this.type);
                                                m.Write(this.sentFrom);
                                                m.Write(this.sendToUUID);
                                                m.Write(this.endLocation[0]);
                                                m.Write(this.endLocation[1]);
                                                m.Write(this.attackedLocation[0]);
                                                m.Write(this.attackedLocation[1]);
                                                m.Write(this.damageDealt);
                                                m.Write(this.attackedUnitID);
                                                m.Write(this.attackerUnitID);
                                                m.Write(this.unitAttackDied);
                                 */
                                this.gameMatch.waitingForTurn = false;
                                this.gameMatch.handleOpponentMove(m);

                            }
                            Console.WriteLine(m.ToString());

                            break;
                    }
                }
            }
            

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

        public WarriorType getWarriorType(String warriorType) {
            if (warriorType.Equals("axestan warrior")) 
            {
                return warriorTypes[0];
            }
            if (warriorType.Equals("firedragon")) 
            {
                return warriorTypes[1];
            }
            if (warriorType.Equals("blue archer")) 
            {
                return warriorTypes[2];
            }
            if (warriorType.Equals("white mage")) 
            {
                return warriorTypes[3];
            }
            if (warriorType.Equals("crocy")) 
            {
                return warriorTypes[4];
            }
            if (warriorType.Equals("magier")) 
            {
                return warriorTypes[5];
            }
            return warriorTypes[1];
        }


        public string confirmPassword { get; set; }
    }
}
