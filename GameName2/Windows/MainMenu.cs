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
        public Button findMatchButton { get; set; }
        public Button customizeArmyButton { get; set; }
        public MouseState pastState { get; set; }
        public BackButton backButton { get; set; }
        public Vector2 welcomeVector { get; set; }
        public Dialog dialog { get; set; }
        public Button dialogCancel { get; set; }
        public TextAnimation dialogText { get; set; }
        
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
            int height = 330;
            int offsetX = (this.windowManager.Window.ClientBounds.Width - width) / 2;
            int offsetY = (this.windowManager.Window.ClientBounds.Height - height) / 2;
            this.welcomeVector = new Vector2(offsetX, offsetY);
            this.findMatchButton = new Button("FIND MATCH", new Rectangle(offsetX, offsetY+100, 350, 100), Game1.menuFont);
            this.customizeArmyButton = new Button("CUSTOMIZE ARMY", new Rectangle(offsetX, offsetY + 230, 350, 100), Game1.menuFont);
            this.backButton = new BackButton();
            int dialogWidth = 600;
            int dialogHeight = 300;
            this.dialog = new Dialog(this.windowManager, dialogWidth, dialogHeight, isVisible: false);
            this.dialogCancel = new Button("CANCEL", dialog.getComponentLocation(200, 200, 70), Game1.menuFont, isVisible: false);
            List<String> phrases = new List<String>();
            phrases.Add("Searching for opponent");
            phrases.Add("Searching for opponent.");
            phrases.Add("Searching for opponent..");
            phrases.Add("Searching for opponent...");
            
            this.dialogText = new TextAnimation(dialog.getComponentLocation(100, (int)Game1.menuFont.MeasureString("Searching for opponent...").X, 100), phrases, 500, Game1.menuFont, isVisible: false);
        }

        public void Update(GameTime gameTime)
        {
            MouseState newState = Mouse.GetState();
            this.dialogText.update(gameTime);
            if (!this.pastState.Equals(newState))
            {
                if (newState.LeftButton == ButtonState.Pressed && pastState.LeftButton != ButtonState.Pressed)
                {
                    if (!this.dialog.isVisible)
                    {
                        if (this.findMatchButton.checkClick(newState))
                        {

                        }
                        if (this.customizeArmyButton.checkClick(newState))
                        {
                        }
                        if (this.backButton.checkClick(newState))
                        {
                        }
                    }
                    else
                    {
                        if (this.dialogCancel.checkClick(newState))
                        {
                        }
                    }
                }
                if (newState.LeftButton == ButtonState.Released && pastState.LeftButton != ButtonState.Released)
                {
                    if (!this.dialog.isVisible)
                    {
                        if (this.findMatchButton.unClick(newState))
                        {
                            // will actually open find match dialog here in future
                            // then enter gameplay when match is found
                            /*
                             *this.dialogText.isVisible = true;
                            this.dialogCancel.isVisible = true;
                            this.dialog.isVisible = true;
                             */
                            this.windowManager.gameState = GameState.gameMatch;
                            this.windowManager.windows[GameState.gameMatch].Initialize();
                            Game1.gameStates.Push(GameState.mainMenu);
                        }
                        if (this.customizeArmyButton.unClick(newState))
                        {
                            this.windowManager.gameState = GameState.customizeArmy;
                            this.windowManager.windows[GameState.customizeArmy].Initialize();
                            Game1.gameStates.Push(GameState.mainMenu);
                        }
                        if (this.backButton.unClick(newState))
                        {
                            int newGameState = Game1.gameStates.Pop();
                            this.windowManager.gameState = newGameState;
                            this.windowManager.windows[newGameState].Initialize();
                        }
                    }
                    else
                    {
                        if (this.dialogCancel.unClick(newState))
                        {
                            this.dialogCancel.isVisible = false;
                            this.dialog.isVisible = false;
                            this.dialogText.isVisible = false;
                        }
                    }
                }
            }
            this.pastState = newState;
        }

        public void Draw()
        {
            this.windowManager.spriteBatch.Begin();
            windowManager.spriteBatch.Draw(Game1.background, new Rectangle(0, 0, this.windowManager.Window.ClientBounds.Width, this.windowManager.Window.ClientBounds.Height), Color.White);
            this.windowManager.spriteBatch.DrawString(Game1.gameFont, "Welcome, " + this.windowManager.username + "!", this.welcomeVector, Color.White);
            this.windowManager.spriteBatch.End();
            this.findMatchButton.draw(this.windowManager.spriteBatch);
            this.customizeArmyButton.draw(this.windowManager.spriteBatch);
            this.backButton.drawBackButton(windowManager.spriteBatch);
            this.dialog.draw();
            this.dialogCancel.draw(this.windowManager.spriteBatch);
            this.dialogText.draw(this.windowManager.spriteBatch);
         
        }
    }
}
