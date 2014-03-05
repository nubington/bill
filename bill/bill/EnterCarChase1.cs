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
using System.Diagnostics;

namespace bill
{
    class EnterCarChase1 : Battle1
    {
        static bool contentLoaded = false;
        Stopwatch startTimer = new Stopwatch();

        const int BILLCARSIZE = 125, STARTDELAY = 800;

        static Song carChaseMusic;

        static Texture2D isaacTexture, billCarTexture, roadTexture;

        static int scrollSpeed = 650;
        int actualScrollSpeed;

        Vector2 isaacTarget;

        public EnterCarChase1(EventHandler callback)
            : base(callback, false)
        {
            Game1.Game.Window.Title = "";

            CarChase1.background1 = new Rectangle(0, 0, 480, 480);
            CarChase1.background2 = new Rectangle(CarChase1.background1.X + CarChase1.background1.Width, 0, 480, 480);
            CarChase1.background3 = new Rectangle(CarChase1.background2.X + CarChase1.background2.Width, 0, 480, 480);

            //bill.X = 0;
            //bill.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - bill.Height / 2;
            bill.Width = (int)(BILLCARSIZE * 1.34594595f);
            bill.Height = BILLCARSIZE;
            bill.X = 0;
            bill.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - bill.Height / 2;
            //bill.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2);
            bill.Speed = (int)(GameSettings.DEFAULTBILLSPEED * .5f);
            //billTarget = bill.CenterPoint;
            //bill.ShootDelay = (int)(500 / GameSettings.BillShootSpeed);
            billTarget = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, bill.Y + bill.Height / 2);

            //isaac.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .25f, Graphics.GraphicsDevice.Viewport.Height / 2);
            isaac.X = 0;
            isaac.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - isaac.Height / 2;
            isaacTarget = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .25f, Graphics.GraphicsDevice.Viewport.Height / 2);

            if (!contentLoaded)
            {
                roadTexture = Content.Load<Texture2D>("roadbackground2");
                isaacTexture = Content.Load<Texture2D>("isaacdope");
                billCarTexture = Content.Load<Texture2D>("billcar1");
                carChaseMusic = Content.Load<Song>("carchasemusic");
                contentLoaded = true;
            }

            isaac.Texture = isaacTexture;
            bill.Texture = billCarTexture;

            startTimer.Start();

            MediaPlayer.Play(carChaseMusic);
            MediaPlayer.Volume = .25f;
            MediaPlayer.IsRepeating = false;
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

            // mute check
            checkForMute();

            // pause check
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.P))
                allowPause = true;
            if (allowPause && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.P))
            {
                paused ^= true;
                allowPause = false;
                if (paused)
                {
                    startTimer.Stop();
                    MediaPlayer.Pause();
                }
                else
                {
                    startTimer.Start();
                    MediaPlayer.Resume();
                }
            }
            if (paused)
                return;

            if (startTimer.ElapsedMilliseconds >= 8000)
            {
                returnControl("carchase");
                return;
            }

            actualScrollSpeed = (int)Math.Round(Util.ScaleWithGameTime(scrollSpeed, gameTime));
            scrollBackground(gameTime);

            if (startTimer.ElapsedMilliseconds >= STARTDELAY)
            {
                //scrollBackground(gameTime);
                bill.moveTowards(billTarget, gameTime, true);
            }

            if (startTimer.ElapsedMilliseconds >= 4000 && isaac.CenterPoint != isaacTarget)
                isaac.moveTowards(isaacTarget, gameTime, true);

            //moveBillRandom(gameTime);

            //moveIsaac(gameTime);

            //checkForBillShoot(gameTime);

            //Bullet.moveBullets(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(roadTexture, CarChase1.background1, Color.White);
            spriteBatch.Draw(roadTexture, CarChase1.background2, Color.White);
            spriteBatch.Draw(roadTexture, CarChase1.background3, Color.White);

            if (startTimer.ElapsedMilliseconds >= STARTDELAY)
            {
                spriteBatch.Draw(bill.Texture, bill, Color.White);
                if (startTimer.ElapsedMilliseconds >= 4000)
                    spriteBatch.Draw(isaac.Texture, isaac, Color.White);
            }

            if (startTimer.ElapsedMilliseconds >= 5750 && startTimer.ElapsedMilliseconds < 6750)
                spriteBatch.DrawString(billStatusFont, "INCONCEIVABLE", new Vector2((int)(isaac.X + isaac.Width / 2 - billStatusFont.MeasureString("INCONCEIVABLE").X / 2), (int)(isaac.Y - billStatusFont.MeasureString("INCONCEIVABLE").Y)), Color.White);

            //if (timeElapsed.TotalMilliseconds >= 6750 && timeElapsed.TotalMilliseconds < 8000)
            //    spriteBatch.DrawString(billStatusFont, "marg dar", new Vector2((int)(bill.X + bill.Width / 2 - billStatusFont.MeasureString("marg dar").X / 2), (int)(bill.Y - billStatusFont.MeasureString("bill come back").Y)), Color.White);
            
            //pause and fps count
            Vector2 pauseStringSize = billStatusFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(billStatusFont, "PAUSED", new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2 - pauseStringSize.X / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            //else
                //frameCounter++;
        }

        void scrollBackground(GameTime gameTime)
        {
            if (CarChase1.background1.X < -CarChase1.background1.Width)
                CarChase1.background1.X = CarChase1.background3.X + CarChase1.background3.Width;
            else if (CarChase1.background2.X < -CarChase1.background2.Width)
                CarChase1.background2.X = CarChase1.background1.X + CarChase1.background1.Width;
            else if (CarChase1.background3.X < -CarChase1.background3.Width)
                CarChase1.background3.X = CarChase1.background2.X + CarChase1.background2.Width;


            CarChase1.background1.X -= actualScrollSpeed;
            CarChase1.background2.X -= actualScrollSpeed;
            CarChase1.background3.X -= actualScrollSpeed;
        }

        void moveBillRandom(GameTime gameTime)
        {
            checkIfBillMovementTargetHit(gameTime);
            bill.moveTowards(billTarget, gameTime, true);
        }

        void checkIfBillMovementTargetHit(GameTime gameTime)
        {
            if (bill.CenterPoint == billTarget)
                billTarget = new Vector2(rand.Next(bill.Width / 2 + Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Width - bill.Width / 2), rand.Next(bill.Height / 2, Graphics.GraphicsDevice.Viewport.Height - bill.Height));
        }

        void checkForBillShoot(GameTime gameTime)
        {
            bill.TimeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (bill.TimeSinceLastShot >= bill.ShootDelay)
            {
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], new Vector2(0, bill.CenterPoint.Y), scrollSpeed);
                bill.TimeSinceLastShot = 0;
            }
        }
    }
}