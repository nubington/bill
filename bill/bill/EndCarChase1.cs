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
    class EndCarChase1 : Battle1
    {
        static bool contentLoaded = false;
        Stopwatch startTimer = new Stopwatch();
        DateTime isaacSizeMaxedTime;

        const int BILLCARSIZE = 125, STARTDELAY = 850;

        static Song carChaseMusic;

        //static Rectangle carChaseBackground1, carChaseBackground2, carChaseBackground3;

        static Texture2D isaacTexture, billCarTexture, roadTexture;

        static int scrollSpeed = 650;
        int actualScrollSpeed;

        Vector2 isaacTarget;

        public EndCarChase1(EventHandler callback, Character isaac, Character bill)
            : base(callback, false)
        {
            Game1.Game.Window.Title = "";

            //carChaseBackground1 = new Rectangle(0, 0, 480, 480);
            //carChaseBackground2 = new Rectangle(carChaseBackground1.X + carChaseBackground1.Width, 0, 480, 480);
            //carChaseBackground3 = new Rectangle(carChaseBackground2.X + carChaseBackground2.Width, 0, 480, 480);

            //bill.X = 0;
            //bill.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - bill.Height / 2;
            //bill.Width = (int)(BILLCARSIZE * 1.34594595f);
            //bill.Height = BILLCARSIZE;
            //bill.X = -bill.Width;
            //bill.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - bill.Height / 2;
            //bill.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2);
            //bill.Speed = (int)(GameSettings.DEFAULTBILLSPEED * .5f);
            //billTarget = bill.CenterPoint;
            //bill.ShootDelay = (int)(500 / GameSettings.BillShootSpeed);
            this.bill = bill;
            bill.Speed = 100;
            billTarget = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2);

            //isaac.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .25f, Graphics.GraphicsDevice.Viewport.Height / 2);
            //isaac.X = 0;
            //isaac.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - isaac.Height / 2;
            this.isaac = isaac;
            isaac.Speed = 100;
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

            /*MediaPlayer.Play(carChaseMusic);
            MediaPlayer.Volume = .25f;
            MediaPlayer.IsRepeating = true;*/
        }

        float rotationSpeed = 10f;

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

            //if (startTimer.ElapsedMilliseconds >= 10000)
                //MediaPlayer.Volume -= .001f;
            //if (MediaPlayer.Volume == 0)
            //if (MediaPlayer.State == MediaState.Stopped)
            
            // go to PostGame
            if (isaacSizeMaxedTime == DateTime.MinValue && isaac.Height == Graphics.GraphicsDevice.Viewport.Height)
                isaacSizeMaxedTime = DateTime.Now;
            if (isaacSizeMaxedTime != DateTime.MinValue && (DateTime.Now - isaacSizeMaxedTime).TotalSeconds >= 1)
            {
                returnControl("postgame");
                return;
            }

            actualScrollSpeed = (int)Math.Round(Util.ScaleWithGameTime(scrollSpeed, gameTime));
            scrollBackground(gameTime);

            //if (startTimer.ElapsedMilliseconds >= STARTDELAY)
            //{
                //scrollBackground(gameTime);
                //bill.moveTowards(billTarget, gameTime);
            //}

            //if (startTimer.ElapsedMilliseconds < 3000)
                //bill.moveTowards(billTarget, gameTime);
            //else
            //else if (startTimer.ElapsedMilliseconds < 7000)
            {
                moveBillRandom(gameTime);
                bill.Speed += 2;
                rotationSpeed += .05f;
                bill.Rotation += Util.ScaleWithGameTime(rotationSpeed, gameTime);
            }

            if (startTimer.ElapsedMilliseconds > 3000)
            {
                billSprayFireRandom();
                rotateCans(gameTime);
                Bullet.moveBullets(gameTime);
                isaacTarget = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height / 2);
                isaac.moveTowardsPrecise(isaacTarget, gameTime, true);
                if (isaac.Height < Graphics.GraphicsDevice.Viewport.Height)
                {
                    isaac.Width += 1;
                    isaac.Height += 1;
                }
            }

            if (isaac.CenterPoint != isaacTarget)
                isaac.moveTowardsPrecise(isaacTarget, gameTime, true);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(roadTexture, CarChase1.background1, Color.White);
            spriteBatch.Draw(roadTexture, CarChase1.background2, Color.White);
            spriteBatch.Draw(roadTexture, CarChase1.background3, Color.White);

            spriteBatch.Draw(isaac.Texture, isaac, Color.White);
            spriteBatch.Draw(bill.Texture, new Rectangle((int)bill.CenterPoint.X, (int)bill.CenterPoint.Y, bill.Width, bill.Height), null, Color.White, bill.Rotation, bill.TextureCenterOrigin, SpriteEffects.None, 0f);

            foreach (Bullet b in Bullet.Cans)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

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
            bill.moveTowardsPrecise(billTarget, gameTime, true);
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

        void billSprayFireRandom()
        {
            Vector2 target = new Vector2(rand.Next(Graphics.GraphicsDevice.Viewport.Width), rand.Next(Graphics.GraphicsDevice.Viewport.Height));
            //for (int i = 0; i < 4; i++)
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], target, 1000);
            /*Vector2 target;
            for (int i = 0; i < 4; i++)
            {
                target = new Vector2(rand.Next(Graphics.GraphicsDevice.Viewport.Width), rand.Next(Graphics.GraphicsDevice.Viewport.Height));
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], target, 1000);
            }*/
        }

        void rotateCans(GameTime gameTime)
        {
            foreach (Bullet c in Bullet.Cans)
                c.Rotation += Util.ScaleWithGameTime(5, gameTime);
        }
    }
}