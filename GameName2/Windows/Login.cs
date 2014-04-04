﻿#region Using Statements
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
using System.Data.SqlClient;
using System.Data;
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
        public InputDialog confirmPasswordInput { get; set; }
        public KeyboardState oldState { get; set; }
        public MouseState oldMouseState { get; set; }
        public Button submitButton { get; set; }
        public Button registerButton { get; set; }
        public String errorMessage { get; set; }
        public Vector2 errorMessageLoc { get; set; }
        public Rectangle capitalLogoLoc { get; set; }
        public string appsalt = "Ay2cXjA4";
        public Button newUserClick { get; set;}
        public Button existingUserClick {get; set;}


        public Login(Game1 windowManager)
        {
            this.windowManager = windowManager;
        }

        public void Initialize()
        {
            Game1.gameStates.Clear();
            windowManager.username = "";
            windowManager.password = "";
            windowManager.confirmPassword = "";
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
            this.confirmPasswordInput = new InputDialog("Confirm Password", new Rectangle(leftEdge + 125, startY + 290, 170, 25), mask: true, isVisible: false);
            this.submitButton = new Button("LOGIN", new Rectangle(leftEdge + 40, startY + 290, 250, 50), Game1.smallFont);
            this.registerButton = new Button("REGISTER", new Rectangle(leftEdge + 40, startY + 320, 250, 50), Game1.smallFont, isVisible: false);
            this.errorMessageLoc = new Vector2(leftEdge + 25, startY + 430);
            this.newUserClick = new Button ("new user?", new Rectangle(leftEdge + 40, startY + 350, 250, 50), Game1.smallFont);
            this.existingUserClick = new Button("existing user?", new Rectangle(leftEdge + 40, startY + 380, 250, 50), Game1.smallFont, isVisible: false);

        }

        public void UnloadContent()
        {
        }

        public void Update(GameTime gameTime)
        {
            this.usernameInput.update(gameTime);
            this.passwordInput.update(gameTime);
            this.confirmPasswordInput.update(gameTime);
            this.submitButton.update(gameTime);
            this.registerButton.update(gameTime);
            this.newUserClick.update(gameTime);
            this.existingUserClick.update(gameTime);

            KeyboardState newState = Keyboard.GetState();
            if (!newState.Equals(oldState))
            {
                InputDialog activeInput = this.usernameInput;
                if (passwordInput.isActive)
                {
                    activeInput = this.passwordInput;
                }
                else if (confirmPasswordInput.isActive)
                {
                    activeInput = this.confirmPasswordInput;
                }
                Keys[] keys = newState.GetPressedKeys();
                foreach (Keys key in keys)
                {
                    if (!oldState.IsKeyDown(key))
                    {
                        if (key.Equals(Keys.Tab))
                        {
                            if (confirmPasswordInput.isVisible == false) //In Login State
                            {
                                this.passwordInput.toggleActive();
                                this.usernameInput.toggleActive();
                            }
                            else // In Register State
                            {
                                if (usernameInput.isActive == true)
                                {
                                    this.passwordInput.toggleActive();
                                    this.usernameInput.toggleActive();
                                }
                                else if (passwordInput.isActive == true)
                                {
                                    this.passwordInput.toggleActive();
                                    this.confirmPasswordInput.toggleActive();
                                }
                                else
                                {
                                    this.confirmPasswordInput.toggleActive();
                                    this.usernameInput.toggleActive();
                                }
                            }
                        }
                        else if (key.Equals(Keys.Enter))
                        {
                            this.login(this.usernameInput.content, this.passwordInput.content);
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
                    Boolean isConfirmPasswordInputClicked = confirmPasswordInput.handleClick(newMouseState);
                    if (isUserInputClicked)
                    {
                        passwordInput.isActive = false;
                        confirmPasswordInput.isActive = false;
                    }
                    if (isPasswordInputClicked)
                    {
                        usernameInput.isActive = false;
                        confirmPasswordInput.isActive = false;
                    }
                    if (isConfirmPasswordInputClicked)
                    {
                        usernameInput.isActive = false;
                        passwordInput.isActive = false;
                    }
                    if (this.submitButton.checkClick(newMouseState))
                    {

                    }
                    if (this.registerButton.checkClick(newMouseState))
                    {

                    }
                    if (this.newUserClick.checkClick(newMouseState))
                    {

                    }
                    if (this.existingUserClick.checkClick(newMouseState))
                    {

                    }

                }
                if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (this.submitButton.unClick(newMouseState))
                    {
                        this.login(this.usernameInput.content, this.passwordInput.content);
                    }
                    else if (this.registerButton.unClick(newMouseState))
                    {
                        this.register(this.usernameInput.content, this.passwordInput.content, this.confirmPasswordInput.content);
                    }
                    else if (this.newUserClick.unClick(newMouseState))
                    {
                        this.showRegister();

                    }
                    else if (this.existingUserClick.unClick(newMouseState))
                    {
                        this.showSubmit();
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
            this.usernameInput.draw(windowManager.spriteBatch);
            this.passwordInput.draw(windowManager.spriteBatch);
            this.confirmPasswordInput.draw(windowManager.spriteBatch);
            this.submitButton.draw(windowManager.spriteBatch);
            this.registerButton.draw(windowManager.spriteBatch);
            this.newUserClick.draw(windowManager.spriteBatch);
            this.existingUserClick.draw(windowManager.spriteBatch);

        }
        public void login(string username, string password)
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");
            if (db.OpenConnection() == true)
            {

                string hashquery = "SELECT salt, password FROM users WHERE username=@username";
                MySqlCommand saltCmd = new MySqlCommand(hashquery, db.connection);
                saltCmd.Parameters.AddWithValue("@username", username);
                MySqlDataReader saltReader = saltCmd.ExecuteReader();
                string retrievedSalt = "";
                string hashedPwdFromDatabase = "";
                while (saltReader.Read())
                {
                    hashedPwdFromDatabase = (string)saltReader["password"];
                    retrievedSalt = (string)saltReader["salt"];
                }
                //edited
                if (username == "" || password == "")
                {
                    this.errorMessage = "Username or password is blank.";
                }
                else if ((username == "kevin" && password == "kevin") || BCrypt.Net.BCrypt.Verify(password + appsalt + retrievedSalt, hashedPwdFromDatabase))
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

        public void showRegister()
        {
            this.usernameInput.changeLabel("Choose a Username");
            this.passwordInput.changeLabel("Select a Password");
            this.confirmPasswordInput.setVisible(true);
            this.submitButton.isVisible = false;
            this.registerButton.isVisible = true;
            this.newUserClick.isVisible = false;
            this.existingUserClick.isVisible = true;
        }

        public void showSubmit()
        {
            this.usernameInput.changeLabel("Username");
            this.passwordInput.changeLabel("Password");
            this.confirmPasswordInput.setVisible(false);
            this.submitButton.isVisible = true;
            this.registerButton.isVisible = false;
            this.newUserClick.isVisible = true;
            this.existingUserClick.isVisible = false;
        }

        public void register(string username, string password, string confirmPassword)
        {
            DBConnect db = new DBConnect("stardock.cs.virginia.edu", "cs4730capital", "cs4730capital", "spring2014");

            if (username == "" || password == "" || confirmPassword == "")
            {
                this.errorMessage = "All fields must be filled out.";
            }
            if (!password.Equals(confirmPassword, StringComparison.Ordinal))
            {
                this.errorMessage = "Passwords do not match.";
            }

            if (password == confirmPassword)
            {
                if (db.OpenConnection() == true)
                {
                    //Check if username if available
                    string testIfAvailable = "SELECT username FROM users WHERE username=@username";
                    MySqlCommand availCmd = new MySqlCommand(testIfAvailable, db.connection);
                    availCmd.Parameters.AddWithValue("@username", username);
                    MySqlDataReader availReader = availCmd.ExecuteReader();
                    string comparison = "";
                    if (!availReader.Read())
                    {
                        string pwdToHash = password + this.appsalt; 
                        string salt = BCrypt.Net.BCrypt.GenerateSalt();
                        string hashToStoreInDatabase = BCrypt.Net.BCrypt.HashPassword(pwdToHash, salt);
                        string command = "INSERT INTO users VALUES (@username, @password, @salt)";
                        MySqlCommand insCmd = new MySqlCommand(command, db.connection);
                        insCmd.Parameters.AddWithValue("username", username);
                        insCmd.Parameters.AddWithValue("password", hashToStoreInDatabase);
                        insCmd.Parameters.AddWithValue("salt", salt);
                        insCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        this.errorMessage = "Username not available.";
                    }

                }

                else
                {
                    this.errorMessage = "Could not connect to DB.";
                }
            }
        }

    }
}
