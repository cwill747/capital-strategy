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

namespace CapitalStrategy.Windows
{
    class MainMenu : Window
    {
        public Game1 windowManager { get; set; }
        public Button playGameButton { get; set; }
        public Button customizeArmyButton { get; set; }
        public MouseState pastState { get; set; }
        
        public MainMenu(Game1 windowManager)
        {
            this.windowManager = windowManager;
        }

        public void Initialize()
        {
            this.pastState = new MouseState();
        }

        public void LoadContent()
        {
            int width = 350;
            int height = 230;
            int offsetX = (this.windowManager.Window.ClientBounds.Width - width) / 2;
            int offsetY = (this.windowManager.Window.ClientBounds.Height - height) / 2;
            this.playGameButton = new Button("PLAY GAME!", new Rectangle(offsetX, offsetY, 350, 100), Game1.menuFont);
            this.customizeArmyButton = new Button("CUSTOMIZE ARMY", new Rectangle(offsetX, offsetY + 130, 350, 100), Game1.menuFont);

        }

        public void Update(GameTime gameTime)
        {
            MouseState newState = Mouse.GetState();
            if (!this.pastState.Equals(newState))
            {
                if (newState.LeftButton == ButtonState.Pressed && pastState.LeftButton != ButtonState.Pressed)
                {
                    if (this.playGameButton.checkClick(newState))
                    {
                        
                    }
                    if (this.customizeArmyButton.checkClick(newState))
                    {
                    }
                }
                if (newState.LeftButton == ButtonState.Released && pastState.LeftButton != ButtonState.Released)
                {
                    if (this.playGameButton.unClick(newState))
                    {
                        this.windowManager.gameState = GameState.gameMatch;
                        this.windowManager.windows[GameState.gameMatch].Initialize();
                    }
                    this.customizeArmyButton.unClick(newState);
                }
            }
            this.pastState = newState;
        }

        public void Draw()
        {
            this.windowManager.spriteBatch.Begin();
            windowManager.spriteBatch.Draw(Game1.background, new Rectangle(0, 0, this.windowManager.Window.ClientBounds.Width, this.windowManager.Window.ClientBounds.Height), Color.White);
            this.windowManager.spriteBatch.End();
            this.playGameButton.draw(this.windowManager.spriteBatch);
            this.customizeArmyButton.draw(this.windowManager.spriteBatch);
        }
    }
}
