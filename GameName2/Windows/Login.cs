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
using CapitalStrategy.GUI;
#endregion

namespace CapitalStrategy.Windows
{
    class Login : Window
    {
        public Game1 windowManager { get; set; }
        public SpriteFont smallFont { get; set; }
        public Texture2D background { get; set; }
        public Texture2D capitalLogo { get; set; }
        public InputDialog usernameInput { get; set; }
        public InputDialog passwordInput { get; set; }
        public KeyboardState oldState { get; set; }
        public MouseState oldMouseState { get; set; }
        public Button submitButton { get; set; }

        public Login(Game1 windowManager)
        {
            this.windowManager = windowManager;
        }

        public void Initialize()
        {
            this.oldState = new KeyboardState();
            this.oldMouseState = new MouseState();
        }

        public void LoadContent()
        {
            this.smallFont = windowManager.Content.Load<SpriteFont>("fonts/smallfont");
            this.background = windowManager.Content.Load<Texture2D>("login/loginBackground");
            this.capitalLogo = windowManager.Content.Load<Texture2D>("login/capital");
            int leftEdge = 250;
            int startY = 300;
            this.usernameInput = new InputDialog("username", new Rectangle(leftEdge, startY, 170, 25), isActive: true);
            this.passwordInput = new InputDialog("password", new Rectangle(leftEdge, startY + 40, 170, 25), mask: true);
            this.submitButton = new Button("SUBMIT", new Rectangle(leftEdge-85, startY + 80, 250, 50));
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
                    this.submitButton.unClick();
                    
                }
                oldMouseState = newMouseState;
            }
        }

        public void Draw()
        {
            windowManager.spriteBatch.Begin();
            windowManager.spriteBatch.Draw(this.background, new Rectangle(0, 0, this.windowManager.BOARDWIDTH, this.windowManager.BOARDHEIGHT), Color.White);
            windowManager.spriteBatch.Draw(this.capitalLogo, new Rectangle(125, 90, 325, 180), Color.White);
            windowManager.spriteBatch.End();
            usernameInput.draw(windowManager.spriteBatch);
            this.passwordInput.draw(windowManager.spriteBatch);
            this.submitButton.draw(windowManager.spriteBatch);
        }
    }
}
