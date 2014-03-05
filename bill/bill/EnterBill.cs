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
    class EnterBill : GameState
    {
        static bool contentLoaded = false;
        DateTime startTime;

        BaseObject bill, skipButton;
        static Texture2D billTexture, bill2Texture, button1Texture, button2Texture;
        static SpriteFont enterBillFont1, titleFont2, actTitleFont;

        const int BILLWIDTH = 200, BILLHEIGHT = 200;
        const int SKIPWIDTH = 60, SKIPHEIGHT = 30;

        static Song enterBill;

        public EnterBill(EventHandler callback)
            : base(callback)
        {
            // initialize
            Stats.Clear();
            startTime = DateTime.Now;
            Game1.Game.Window.Title = "";
            bill = new BaseObject(new Rectangle(0, 0, BILLWIDTH, BILLHEIGHT), new Vector2(0, 0));
            bill.CenterPoint = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            skipButton = new BaseObject(new Rectangle(GraphicsDevice.Viewport.Width - SKIPWIDTH, GraphicsDevice.Viewport.Height - SKIPHEIGHT, SKIPWIDTH, SKIPHEIGHT), new Vector2(0, 0));
            
            // load content
            if (!contentLoaded)
            {
                billTexture = Content.Load<Texture2D>("billtransparent");
                bill2Texture = Content.Load<Texture2D>("bill2transparent");
                button1Texture = Content.Load<Texture2D>("titlebutton1");
                button2Texture = Content.Load<Texture2D>("titlebutton2");
                enterBillFont1 = Content.Load<SpriteFont>("EnterBillFont1");
                titleFont2 = Content.Load<SpriteFont>("TitleFont2");
                actTitleFont = Content.Load<SpriteFont>("ActTitleFont");
                enterBill = Content.Load<Song>("enterbill");
            }

            // play music
            MediaPlayer.Play(enterBill);
            MediaPlayer.Volume = 1;
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                returnControl("title");
                return;
            }

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Enter) ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Space) || 
                skipButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                returnControl();
                return;
            }

            TimeSpan timeElapsed = DateTime.Now - startTime;
            if (timeElapsed.TotalSeconds >= 12)
            {
                returnControl();
                return;
            }

            // mute check
            checkForMute();
        }

        int glowyEyesCounter = 0;
        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Gray);

            TimeSpan timeElapsed = DateTime.Now - startTime;
            bool skipButtonMousedOver = skipButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            string skipButtonMsg = "SKIP",
                message1 = "Bill is flipping out!";
                //message2 = "Use your peanuts to defeat him!";

            spriteBatch.DrawString(actTitleFont, "Act I", new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - actTitleFont.MeasureString("Act I").X / 2), (int)(actTitleFont.MeasureString("Act I").Y)), Color.Black);

            spriteBatch.DrawString(enterBillFont1, message1, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - enterBillFont1.MeasureString(message1).X / 2), (int)(bill.Y - enterBillFont1.MeasureString(message1).Y * 2)), Color.Black);

            //spriteBatch.DrawString(enterBillFont1, message2, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - enterBillFont1.MeasureString(message2).X / 2), (int)(bill.Y + bill.Height + enterBillFont1.MeasureString(message2).Y * 1.25f)), Color.Black);

            //if ((int)timeElapsed.TotalMilliseconds % 2 == 1)
            if (glowyEyesCounter++ % 3 == 0)
                spriteBatch.Draw(bill2Texture, bill.Rectangle, Color.White);
            else
                spriteBatch.Draw(billTexture, bill.Rectangle, Color.White);

            if (skipButtonMousedOver)
                spriteBatch.Draw(button2Texture, skipButton, Color.White);
            else
                spriteBatch.Draw(button1Texture, skipButton, Color.White);
            spriteBatch.DrawString(titleFont2, skipButtonMsg, new Vector2(skipButton.X + skipButton.Width / 2 - titleFont2.MeasureString(skipButtonMsg).X / 2, skipButton.Y + skipButton.Height / 2 - titleFont2.MeasureString(skipButtonMsg).Y / 2), Color.Red);
        }
    }
}