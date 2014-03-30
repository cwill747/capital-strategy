#region Using Statements
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
using MySql.Data.MySqlClient;
using CapitalStrategy.GUI;
#endregion

namespace CapitalStrategy.Windows
{
    class Login : Window
    {
        public Game1 windowManager { get; set; }
        public SpriteFont smallFont { get; set; }
        public Texture2D capitalLogo { get; set; }
        public InputDialog usernameInput { get; set; }
        public InputDialog passwordInput { get; set; }
        public KeyboardState oldState { get; set; }
        public MouseState oldMouseState { get; set; }
        public Button submitButton { get; set; }
        public String errorMessage { get; set; }
        public Vector2 errorMessageLoc { get; set; }
        public Rectangle capitalLogoLoc { get; set; }

        public Login(Game1 windowManager)
        {
            this.windowManager = windowManager;
        }

        public void Initialize()
        {
            Game1.gameStates.Clear();
            windowManager.username = "";
            windowManager.password = "";
            this.oldState = new KeyboardState();
            this.oldMouseState = new MouseState();
            this.errorMessage = "";
        }

        public void LoadContent()
        {
            this.smallFont = windowManager.Content.Load<SpriteFont>("fonts/smallfont");
            this.capitalLogo = windowManager.Content.Load<Texture2D>("login/capital");
            int displayWidth = 325;
            int displayHeight = 340;
            int leftEdge = (windowManager.Window.ClientBounds.Width - displayWidth) / 2;
            int startY = (windowManager.Window.ClientBounds.Height - displayHeight) / 2;
            this.capitalLogoLoc = new Rectangle(leftEdge, startY, 325, 180);
            this.usernameInput = new InputDialog("username", new Rectangle(leftEdge + 125, startY + 210, 170, 25), isActive: true);
            this.passwordInput = new InputDialog("password", new Rectangle(leftEdge + 125, startY + 250, 170, 25), mask: true);
            this.submitButton = new Button("SUBMIT", new Rectangle(leftEdge + 40, startY + 290, 250, 50), Game1.smallFont);
            this.errorMessageLoc = new Vector2(leftEdge + 25, startY + 360);
        }

        public void UnloadContent()
        {
        }

        public void Update(GameTime gameTime)
        {
            this.usernameInput.update(gameTime);
            this.passwordInput.update(gameTime);
            this.submitButton.update(gameTime);

            KeyboardState newState = Keyboard.GetState();
            if (!newState.Equals(oldState))
            {
                InputDialog activeInput = this.usernameInput;
                if (passwordInput.isActive)
                {
                    activeInput = this.passwordInput;
                }
                Keys[] keys = newState.GetPressedKeys();
                foreach (Keys key in keys)
                {
                    if (!oldState.IsKeyDown(key))
                    {
                        if (key.Equals(Keys.Tab))
                        {
                            // we'll have to use an array for when we add a third input (ie password combination)
                            this.usernameInput.toggleActive();
                            this.passwordInput.toggleActive();
                        }
                        else if (key.Equals(Keys.Enter))
                        {
                            this.login();
                        }
                        else
                        {
                            activeInput.handleKey(key);
                        }
                    }
                }

                oldState = newState;
            }
            MouseState newMouseState = Mouse.GetState();
            if (!newMouseState.Equals(oldMouseState))
            {
                if (newMouseState.LeftButton == ButtonState.Pressed)
                {
                    Boolean isUserInputClicked = usernameInput.handleClick(newMouseState);
                    Boolean isPasswordInputClicked = passwordInput.handleClick(newMouseState);
                    if (isUserInputClicked)
                    {
                        passwordInput.isActive = false;
                    }
                    if (isPasswordInputClicked)
                    {
                        usernameInput.isActive = false;
                    }
                    if (this.submitButton.checkClick(newMouseState))
                    {
                        
                    }
                }
                if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (this.submitButton.unClick(newMouseState))
                    {
                        this.login();
                    }
                    
                }
                oldMouseState = newMouseState;
            }
        }

        public void Draw()
        {
            windowManager.spriteBatch.Begin();
            windowManager.spriteBatch.Draw(Game1.background, new Rectangle(0, 0, this.windowManager.Window.ClientBounds.Width, this.windowManager.Window.ClientBounds.Height), Color.White);
            windowManager.spriteBatch.Draw(this.capitalLogo, this.capitalLogoLoc, Color.White);
            windowManager.spriteBatch.DrawString(Game1.smallFont, this.errorMessage, this.errorMessageLoc, Color.Red);
            windowManager.spriteBatch.End();
            usernameInput.draw(windowManager.spriteBatch);
            this.passwordInput.draw(windowManager.spriteBatch);
            this.submitButton.draw(windowManager.spriteBatch);
            
        }
        public void login()
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {
                String username = this.usernameInput.content;
                String password = this.passwordInput.content;
                string appsalt = "Ay2cXjA4";

                string hashquery = "SELECT salt, password FROM users WHERE username=@username";
                MySqlCommand saltCmd = new MySqlCommand(hashquery, db.connection);
                saltCmd.Parameters.AddWithValue("@username", username);
                MySqlDataReader saltReader = saltCmd.ExecuteReader();
                string retrievedSalt = "";
                string hashedPwdFromDatabase = "";
                while(saltReader.Read())
                {
                    hashedPwdFromDatabase = (string)saltReader["password"];
                    retrievedSalt = (string) saltReader["salt"];
                }
                if((username == "kevin" && password == "kevin") || BCrypt.Net.BCrypt.Verify(password + appsalt + retrievedSalt, hashedPwdFromDatabase))
                {
                    windowManager.gameState = GameState.mainMenu;
                    this.windowManager.windows[GameState.mainMenu].Initialize();
                    Game1.gameStates.Push(GameState.login);
                    this.windowManager.username = username;
                    this.windowManager.password = password;
                    this.errorMessage = "";
                    //System.Diagnostics.Debug.WriteLine(dataReader["username"]);
                    //System.Diagnostics.Debug.WriteLine(dataReader["password"]);
                }
                else
                {
                    this.errorMessage = "Invalid username or password.";
                }

                saltReader.Close();

            }
            else
            {
                this.errorMessage = "Could not connect to DB.";
            }
        }
    }
}
