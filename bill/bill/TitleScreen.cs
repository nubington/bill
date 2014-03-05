using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace bill
{
    class TitleScreen : GameState
    {
        static bool contentLoaded = false;
        DateTime startTime;
        //List<BaseObject> titleSprites;

        SimpleButton startButton;
        BaseObject exitButton, physicsButton, isaac;
        static Texture2D isaacTexture, button1Texture, button2Texture;
        static Texture2D isaac1, isaac2, isaac3, isaac4, isaac5;
        static SpriteFont titleFont1, titleFont2;

        Animation isaacAnimation;

        static Song titleTheme;

        const int STARTBUTTONWIDTH = 80, STARTBUTTONHEIGHT = 40;
        const int EXITBUTTONWIDTH = 60, EXITBUTTONHEIGHT = 40;
        const int INSTRUCTIONSBUTTONWIDTH = 90, INSTRUCTIONSBUTTONHEIGHT = 40;
        const int BUTTONSPACING = 5;

        public TitleScreen(EventHandler callback, bool startMusic)
            : base(callback)
        {
            // initialize
            startTime = DateTime.Now;
            Game1.Game.Window.Title = "Welcome to Bill's lair.";
            Game1.Game.IsMouseVisible = true;
            isaac = new BaseObject(new Rectangle(0, 0, 100, 100), new Vector2(0, 0));
            isaac.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height / 2);

            startButton = new SimpleButton(new Rectangle(0, 0, STARTBUTTONWIDTH, STARTBUTTONHEIGHT), button1Texture, button1Texture, button2Texture);
            startButton.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, isaac.Y - 40);
            SimpleButton.AddButton(startButton);

            physicsButton = new BaseObject(new Rectangle(0, 0, INSTRUCTIONSBUTTONWIDTH, INSTRUCTIONSBUTTONHEIGHT), new Vector2(0, 0));
            physicsButton.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, isaac.Y + isaac.Height + 40);

            exitButton = new BaseObject(new Rectangle(0, 0, EXITBUTTONWIDTH, EXITBUTTONHEIGHT), new Vector2(0, 0));
            exitButton.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, physicsButton.Y + physicsButton.Height + 40);

            // load content
            if (!contentLoaded)
            {
                button1Texture = Content.Load<Texture2D>("titlebutton1");
                button2Texture = Content.Load<Texture2D>("titlebutton2");
                isaacTexture = Content.Load<Texture2D>("isaac");
                isaac1 = Content.Load<Texture2D>("isaacswirl1");
                isaac2 = Content.Load<Texture2D>("isaacswirl2");
                isaac3 = Content.Load<Texture2D>("isaacswirl3");
                isaac4 = Content.Load<Texture2D>("isaacswirl4");
                isaac5 = Content.Load<Texture2D>("isaacswirl5");
                titleFont1 = Content.Load<SpriteFont>("TitleFont1");
                titleFont2 = Content.Load<SpriteFont>("TitleFont2");
                titleTheme = Content.Load<Song>("titletheme");
                contentLoaded = true;
            }

            isaacAnimation = new Animation(1.5f, 6f, isaac1 , isaac2, isaac3, isaac4, isaac5, isaac4, isaac3, isaac2, isaac1);
            //isaacAnimation.Start();

            // play music
            if (startMusic)
            {
                MediaPlayer.Play(titleTheme);
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = .25f;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                (exitButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed))
                Game1.Game.Exit();

            MouseState mouseState = Mouse.GetState();
            
            /*if (startButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                returnControl("enterbill");
                return;
            }*/
            SimpleButton.UpdateAll(mouseState);
            if (startButton.Triggered)
            {
                cleanup();
                returnControl("enterbill");
                return;
            }
            else if (physicsButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                cleanup();
                returnControl("physics");
                return;
            }

            // mute check
            checkForMute();

            // go to test
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.T) &&
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.E) &&
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S))
            {
                cleanup();
                returnControl("test");
            }

            // go to shooter level
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S) &&
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.H) &&
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.T))
            {
                cleanup();
                returnControl("shooter");
            }

            // go to rts
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.R) &&
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.T) &&
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S))
            {
                cleanup();
                returnControl("rts");
            }

            // skip to act 2
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S) && 
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.K) && 
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.I))
            {
                cleanup();
                returnControl("entercarchase");
            }

            // animate isaac
            int secondsElapsed = (int)(DateTime.Now - startTime).TotalSeconds;
            //if (!isaacAnimation.IsRunning && secondsElapsed != 0 && secondsElapsed % 5 == 0)
            if (!isaacAnimation.IsRunning && secondsElapsed % 5 == 0)
                isaacAnimation.Start();
            isaacAnimation.Update();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            bool startButtonMousedOver = startButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool physicsButtonMousedOver = physicsButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool exitButtonMousedOver = exitButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            string startButtonMsg = "ENTER", exitButtonMsg = "EXIT", physicsButtonMsg = "PHYSICS";
            Color color;

            GraphicsDevice.Clear(Color.Gray);
            spriteBatch.Draw(isaacAnimation, isaac.Rectangle, Color.White);

            // start button
            if (!startButtonMousedOver)
            {
                spriteBatch.Draw(button1Texture, startButton, Color.White);
                color = Color.Gray;
            }
            else
            {
                spriteBatch.Draw(button2Texture, startButton, Color.White);
                color = Color.Red;
            }
            spriteBatch.DrawString(titleFont2, startButtonMsg, new Vector2((int)(startButton.X + startButton.Width / 2 - titleFont2.MeasureString(startButtonMsg).X / 2), (int)(startButton.Y + startButton.Height / 2 - titleFont2.MeasureString(startButtonMsg).Y / 2)), color);
            
            // physics button
            physicsButton.Rotation += 1f;
            if (!physicsButtonMousedOver)
            {
                spriteBatch.Draw(button1Texture, physicsButton, Color.White);
                color = Color.Gray;
            }
            else
            {
                spriteBatch.Draw(button2Texture, new Rectangle((int)physicsButton.CenterPoint.X, (int)physicsButton.CenterPoint.Y, (int)(physicsButton.Width * .75), physicsButton.Height), null, Color.White, physicsButton.Rotation, new Vector2(button2Texture.Width / 2, button2Texture.Height / 2), SpriteEffects.None, 0f);
                color = Color.Red;
            }
            spriteBatch.DrawString(titleFont2, physicsButtonMsg, new Vector2((int)(physicsButton.X + physicsButton.Width / 2 - titleFont2.MeasureString(physicsButtonMsg).X / 2), (int)(physicsButton.Y + physicsButton.Height / 2 - titleFont2.MeasureString(physicsButtonMsg).Y / 2)), color);

            // exit button
            if (!exitButtonMousedOver)
            {
                spriteBatch.Draw(button1Texture, exitButton, Color.White);
                color = Color.Gray;
            }
            else
            {
                spriteBatch.Draw(button2Texture, exitButton, Color.White);
                color = Color.Red;
            }
            spriteBatch.DrawString(titleFont2, exitButtonMsg, new Vector2((int)(exitButton.X + exitButton.Width / 2 - titleFont2.MeasureString(exitButtonMsg).X / 2), (int)(exitButton.Y + exitButton.Height / 2 - titleFont2.MeasureString(exitButtonMsg).Y / 2)), color);

            spriteBatch.DrawString(titleFont1, "Created by Alex Bagnall", new Vector2(5, Graphics.GraphicsDevice.Viewport.Height - titleFont1.MeasureString("|").Y), Color.Black);
            spriteBatch.DrawString(titleFont1, "Special thanks to Darth Cranium", new Vector2(Graphics.GraphicsDevice.Viewport.Width - 214, Graphics.GraphicsDevice.Viewport.Height - titleFont1.MeasureString("|").Y), Color.Black);
        }

        void cleanup()
        {
            SimpleButton.RemoveButton(startButton);
        }
    }
}