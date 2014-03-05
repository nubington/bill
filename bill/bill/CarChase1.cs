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
    class CarChase1 : Battle1
    {
        static bool contentLoaded = false;

        Stopwatch timer = new Stopwatch();
        const int TIMELIMIT = 60;

        const int BILLCARSIZE = 125;

        static SpriteFont timerFont;
        static Song carChaseMusic;
        public static Rectangle background1, background2, background3;
        static Texture2D isaacTexture, billCarTexture, roadTexture;

        static int scrollSpeed = 625;
        int actualScrollSpeed;

        bool wipingOut = false;
        DateTime wipeOutStartTime;
        const int WIPEOUTDURATION = 750, WIPEOUTROTATIONSPEED = 25;

        Character peanutGenerator;
        Vector2 peanutGeneratorTarget;
        const int PEANUTGENERATORSPEED = 200, PEANUTGENERATORSHOOTDELAY = 500;
        int peanutCount;

        public CarChase1(EventHandler callback)
            : base(callback, false)
        {
            //carChaseBackground1 = new Rectangle(0, 0, 480, 480);
            //carChaseBackground2 = new Rectangle(carChaseBackground1.X + carChaseBackground1.Width, 0, 480, 480);
            //carChaseBackground3 = new Rectangle(carChaseBackground2.X + carChaseBackground2.Width, 0, 480, 480);

            //bill.X = 0;
            //bill.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - bill.Height / 2;
            bill.Width = (int)(BILLCARSIZE * 1.34594595f);
            bill.Height = BILLCARSIZE;
            bill.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, Graphics.GraphicsDevice.Viewport.Height / 2);
            bill.Speed = (int)(GameSettings.DEFAULTBILLSPEED * .5f);
            billTarget = bill.CenterPoint;
            bill.ShootDelay = (int)(1000 / GameSettings.DEFAULTBILLSHOOTSPEED);
            //billTarget = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .75f, bill.Y + bill.Height / 2);

            isaac.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width * .25f, Graphics.GraphicsDevice.Viewport.Height / 2);
            isaac.Speed = GameSettings.DEFAULTISAACSPEED;
            //isaac.X = isaac.Width;
            //isaac.Y = Graphics.GraphicsDevice.Viewport.Height / 2 - isaac.Height / 2;

            peanutGenerator = new Character(new Rectangle(Graphics.GraphicsDevice.Viewport.Width, Graphics.GraphicsDevice.Viewport.Height / 2, 1, 1), new Vector2(PEANUTGENERATORSPEED, PEANUTGENERATORSPEED));
            peanutGeneratorTarget = peanutGenerator.CenterPoint;
            peanutGenerator.ShootDelay = PEANUTGENERATORSHOOTDELAY;

            peanutCount = 0;
            
            if (!contentLoaded)
            {
                roadTexture = Content.Load<Texture2D>("roadbackground2");
                isaacTexture = Content.Load<Texture2D>("isaacdope");
                billCarTexture = Content.Load<Texture2D>("billcar1");
                carChaseMusic = Content.Load<Song>("carchasemusic");
                timerFont = Content.Load<SpriteFont>("TitleFont2");
                contentLoaded = true;
            }

            isaac.Texture = isaacTexture;
            bill.Texture = billCarTexture;

            timer.Start();

            /*MediaPlayer.Play(carChaseMusic);
            MediaPlayer.Volume = .5f;
            MediaPlayer.IsRepeating = true;*/
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                PowerUp.RemoveAlivePowerUps();
                returnControl("title");
                return;
            }

            // go to EndCarChase1
            if (timer.Elapsed.TotalSeconds >= TIMELIMIT)
            {
                Stats.CarChasePeanutScore = peanutCount;
                returnControl(isaac, bill);
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
                    timer.Stop();
                    MediaPlayer.Pause();
                }
                else
                {
                    timer.Start();
                    MediaPlayer.Resume();
                }
            }
            if (paused)
                return;

            //show fps every 1 second
            fpsElapsedTime += gameTime.ElapsedGameTime;
            if (fpsElapsedTime > TimeSpan.FromSeconds(1))
            {
                fpsElapsedTime -= TimeSpan.FromSeconds(1);
                Game1.Game.Window.Title = "FPS: " + frameCounter;
                //Game1.Game.Window.Title = "FPS: " + frameCounter + " | Projectiles: " + (Bullet.Peanuts.Count + Bullet.Cans.Count) + " | Powerups: " + PowerUp.AlivePowerUps.Count + " | Sound instances: " + SoundEffectManager.SoundEffectInstances.Count + ".";
                //Game1.Game.Window.Title = "Peanuts: " + Bullet.Peanuts.Length + " Cans: " + Bullet.Cans.Length + " Powerups: " + PowerUp.AlivePowerUps.Count;
                frameCounter = 0;
            }

            actualScrollSpeed = (int)Math.Round(Util.ScaleWithGameTime(scrollSpeed, gameTime));
            scrollBackground(gameTime);

            if (isaac.CenterPoint.X > Graphics.GraphicsDevice.Viewport.Width / 2)
                bill.moveTowards(isaac.CenterPoint, gameTime, true);
            else
                moveBillRandom(gameTime);
            movePeanutGenerator(gameTime);
            if (!wipingOut)
                moveIsaac(gameTime);
            movePeanutsAndCans();

            if (GameSettings.ProjectileRotationCollision)
            {
                isaac.CalculateCorners();
                bill.CalculateCorners();
                foreach (Bullet b in Bullet.Peanuts)
                    b.CalculateCorners();
                foreach (Bullet b in Bullet.Cans)
                    b.CalculateCorners();
            }

            checkForWipeOut(gameTime);
            checkForBillShoot(gameTime);
            checkForPeanutGeneratorShoot(gameTime);
            checkForPeanutPickUp();

            /*if (++cleanupCounter >= 60)
            {
                SoundEffectManager.Cleanup();
                cleanupCounter = 0;
            }*/
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // background
            spriteBatch.Draw(roadTexture, background1, Color.White);
            spriteBatch.Draw(roadTexture, background2, Color.White);
            spriteBatch.Draw(roadTexture, background3, Color.White);

            // bill and isaac
            spriteBatch.Draw(bill.Texture, bill, Color.White);
            //spriteBatch.Draw(isaac.Texture, isaac, Color.White);
            spriteBatch.Draw(isaac.Texture, new Rectangle((int)isaac.CenterPoint.X, (int)isaac.CenterPoint.Y, isaac.Width, isaac.Height), null, Color.White, isaac.Rotation, isaac.TextureCenterOrigin, SpriteEffects.None, 0f);

            // projectiles
            foreach (Bullet b in Bullet.Peanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);
            foreach (Bullet b in Bullet.Cans)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            // timer
            string timeString = (TIMELIMIT - timer.Elapsed.TotalSeconds).ToString("F1");
            int posY = (int)(timerFont.MeasureString(timeString).Y / 2);
            spriteBatch.DrawString(timerFont, timeString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - timerFont.MeasureString(timeString).X / 2), posY), Color.Black);

            // peanut counter
            string peanutCounterString = "Peanuts: " + peanutCount;
            spriteBatch.DrawString(billStatusFont, peanutCounterString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - billStatusFont.MeasureString(peanutCounterString).X / 2), (int)(Graphics.GraphicsDevice.Viewport.Height - billStatusFont.MeasureString(peanutCounterString).Y * 1.5f)), Color.Black);
            //spriteBatch.DrawString(billStatusFont, peanutCounterString, new Vector2(10, posY), Color.Black);

            //pause and fps count
            Vector2 pauseStringSize = billStatusFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(billStatusFont, "PAUSED", new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2 - pauseStringSize.X / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;
        }

        void scrollBackground(GameTime gameTime)
        {
            if (background1.X < -background1.Width)
                background1.X = background3.X + background3.Width;
            else if (background2.X < -background2.Width)
                background2.X = background1.X + background1.Width;
            else if (background3.X < -background3.Width)
                background3.X = background2.X + background2.Width;

            background1.X -= actualScrollSpeed;
            background2.X -= actualScrollSpeed;
            background3.X -= actualScrollSpeed;
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
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], new Vector2(0, bill.CenterPoint.Y), scrollSpeed, rand.NextDouble() * 6.28, 1f);
                bill.TimeSinceLastShot = 0;
            }
        }

        void movePeanutGenerator(GameTime gameTime)
        {
            checkIfPeanutGeneratorTargetHit(gameTime);
            peanutGenerator.moveTowardsPrecise(peanutGeneratorTarget, gameTime, true);
        }

        void checkIfPeanutGeneratorTargetHit(GameTime gameTime)
        {
            if (peanutGenerator.Y == peanutGeneratorTarget.Y)
            {
                peanutGeneratorTarget = new Vector2(peanutGenerator.CenterPoint.X, rand.Next(isaac.Height, Graphics.GraphicsDevice.Viewport.Height - isaac.Height));
            }
        }

        void checkForPeanutGeneratorShoot(GameTime gameTime)
        {
            peanutGenerator.TimeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (peanutGenerator.TimeSinceLastShot >= peanutGenerator.ShootDelay)
            {
                peanutGenerator.shootBullet(BulletType.Peanut, new Vector2(0, peanutGenerator.CenterPoint.Y), scrollSpeed, rand.NextDouble() * 6.28, 1.4f);
                peanutGenerator.TimeSinceLastShot = 0;
            }
        }

        void checkForWipeOut(GameTime gameTime)
        {
            if (!wipingOut)
            {
                // check for wipe out
                if (isaac.Rectangle.Intersects(bill.Rectangle))
                {
                    wipingOut = true;
                    wipeOutStartTime = DateTime.Now;
                    soundEffectManager.Play(deathSound, 1f);
                    return;
                }
                for (int i = 0; i < Bullet.Cans.Count; )
                {
                    Bullet b = Bullet.Cans[i];
                    //if (isaac.Intersects(b))
                    if ((GameSettings.ProjectileRotationCollision && isaac.Intersects(b)) ||
                            isaac.Rectangle.Intersects(b.Rectangle))
                    {
                        wipingOut = true;
                        wipeOutStartTime = DateTime.Now;
                        soundEffectManager.Play(deathSound, 1f);
                        Bullet.Cans.Remove(b);
                    }
                    i++;
                }
            }
            else
            {
                // end wipeout
                if ((DateTime.Now - wipeOutStartTime).TotalMilliseconds > WIPEOUTDURATION)
                {
                    wipingOut = false;
                    isaac.Rotation = 0f;
                }
                // do wipeout movement
                else
                {
                    isaac.Rotation += Util.ScaleWithGameTime(WIPEOUTROTATIONSPEED, gameTime);
                    isaac.X -= actualScrollSpeed;
                    //if (isaac.X < isaac.minX)
                    //    isaac.X = isaac.minX;
                    isaac.X = (int)MathHelper.Max(isaac.X, isaac.minX);
                }
            }
        }

        void checkForPeanutPickUp()
        {
            if (!wipingOut)
            {
                for (int i = 0; i < Bullet.Peanuts.Count; )
                {
                    Bullet b = Bullet.Peanuts[i];
                    if ((GameSettings.ProjectileRotationCollision && isaac.Intersects(b)) || 
                            isaac.Rectangle.Intersects(b.Rectangle))
                    {
                        peanutCount++;
                        Bullet.Peanuts.Remove(b);
                        soundEffectManager.Play(getPowerUp, 1f);
                    }
                    else
                        i++;
                }
            }
        }

        void movePeanutsAndCans()
        {
            foreach (Bullet b in Bullet.Peanuts)
                b.X -= actualScrollSpeed;
            foreach (Bullet b in Bullet.Cans)
                b.X -= actualScrollSpeed;
        }
    }
}