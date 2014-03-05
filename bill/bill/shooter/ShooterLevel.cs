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
    class ShooterLevel : GameState
    {
        static bool contentLoaded = false;
        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;
        protected bool paused, allowPause;
        Random rand = new Random();

        static SpriteFont pauseFont;

        static Song shooterMusic;

        const int CROSSHAIRSIZE = 25;
        static Texture2D crossHairTexture;
        static BaseObject crossHair = new BaseObject(new Rectangle(0, 0, CROSSHAIRSIZE, CROSSHAIRSIZE));
        
        const int GUNWIDTH = 50, GUNHEIGHT = 25;
        const int GUNWHEELSIZE = 30;//22;
        const float WHEELCIRCUMFERENCE = GUNWHEELSIZE / 2 * (float)Math.PI;
        //float gunMoveSpeed = 50, gunTurnSpeed = 3;//, gunWheelRotationSpeed = 8;

        static Texture2D gunTexture, gunBoardTexture, gunWheelTexture;
        //static BaseObject gunBoard, gunWheel1, gunWheel2;
        static CrystalGun gun;

        /*static Vector2 gunPosition;
        static Vector2 GunPosition
        {
            get
            {
                return gunPosition;
            }
            set
            {
                gunPosition = gun.PrecisePosition = value;
                gunBoard.CenterPoint = value;
                gunWheel1.PrecisePosition = gunBoard.CenterPoint - new Vector2(26, 0);
                gunWheel2.PrecisePosition = gunBoard.CenterPoint + new Vector2(26, 0);
            }
        }
        static float GunPositionX
        {
            get
            {
                return gunPosition.X;
            }
            set
            {
                gunPosition.X = gun.PreciseX = value;
                gunBoard.CenterPointX = value;
                gunWheel1.PrecisePosition = gunBoard.CenterPoint - new Vector2(26, 0);
                gunWheel2.PrecisePosition = gunBoard.CenterPoint + new Vector2(26, 0);
            }
        }
        static float GunPositionY
        {
            get
            {
                return gunPosition.Y;
            }
            set
            {
                gunPosition.Y = gun.PreciseY = value;
                gunBoard.CenterPointY = value;
                gunWheel1.PrecisePosition = gunBoard.CenterPoint - new Vector2(26, 0);
                gunWheel2.PrecisePosition = gunBoard.CenterPoint + new Vector2(26, 0);
            }
        }*/

        static Texture2D blastTexture;

        public ShooterLevel(EventHandler callback)
            : base(callback)
        {
            if (!contentLoaded)
            {
                shooterMusic = Content.Load<Song>("shootermusic");
                pauseFont = Content.Load<SpriteFont>("Battle1BillStatusFont");
                crossHairTexture = Content.Load<Texture2D>("crosshair");
                gunTexture = Content.Load<Texture2D>("bluecrystal");
                gunBoardTexture = Content.Load<Texture2D>("wood");
                gunWheelTexture = Content.Load<Texture2D>("woodwheel2");
                blastTexture = Content.Load<Texture2D>("explosionsmall");
                contentLoaded = true;
            }

            Game1.Game.IsMouseVisible = false;

            gun = new CrystalGun(new Rectangle(0, 0, GUNWIDTH, GUNHEIGHT), new Rectangle(0, 0, 60, 5), GUNWHEELSIZE, gunTexture, gunBoardTexture, gunWheelTexture);
            gun.Position = gun.PrecisePosition = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height - GUNWHEELSIZE / 2);
            gun.MoveSpeed = 50;
            gun.TurnSpeed = 3;
            gun.Texture = gunTexture;

            gun.ShootDelay = 175;
            gun.BulletSize = 8;
            gun.BulletSpeed = 800;

            /*gunBoard = new BaseObject(new Rectangle(0, 0, 60, 5));
            gunBoard.CenterPoint = gun.Position;
            gunBoard.Texture = gunBoardTexture;*/

            /*gunWheel1 = new BaseObject(new Rectangle(0, 0, GUNWHEELSIZE, GUNWHEELSIZE));
            gunWheel1.PrecisePosition = gunBoard.CenterPoint - new Vector2(26, 0);

            gunWheel2 = new BaseObject(new Rectangle(0, 0, GUNWHEELSIZE, GUNWHEELSIZE));
            gunWheel2.PrecisePosition = gunBoard.CenterPoint + new Vector2(26, 0);

            gunWheel1.Texture = gunWheel2.Texture = gunWheelTexture;*/

            MediaPlayer.Play(shooterMusic);
            MediaPlayer.Volume = .5f;
            MediaPlayer.IsRepeating = true;
        }

        public override void Update(GameTime gameTime)
        {
            // check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                cleanup();
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
                    MediaPlayer.Volume /= 4;
                }
                else
                {
                    MediaPlayer.Volume *= 4;
                }
            }
            if (paused)
                return;

            //show fps every 1 second
            fpsElapsedTime += gameTime.ElapsedGameTime;
            if (fpsElapsedTime > TimeSpan.FromSeconds(1))
            {
                Game1.Game.Window.Title = "FPS: " + frameCounter;
                fpsElapsedTime -= TimeSpan.FromSeconds(1);
                frameCounter = 0;
            }

            // update position of crosshair position
            crossHair.CenterPoint = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            // update blasts and explosions
            updateBlasts(gameTime);
            updateExplosions();

            // move gun
            moveGun(gameTime);

            // update gun angle
            updateGunAngle(gameTime);

            // move and rotate bullets
            Bullet.moveBullets(gameTime);
            rotatePeanuts(gameTime);

            // shoot on click
            checkForShoot(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // debug monitor stuff
            Game1.Game.DebugMonitor.AddLine("gun rotation: " + gun.Rotation.ToString("F2"));
            Game1.Game.DebugMonitor.AddLine(Blast.Blasts.Count + " blasts");

            GraphicsDevice.Clear(Color.Gray);

            // bullets
            foreach (Bullet b in Bullet.Peanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            // blasts
            foreach (Blast b in Blast.Blasts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            /*// gun board
            spriteBatch.Draw(gun.BoardTexture, gun.Board, Color.White);

            // gun wheels
            spriteBatch.Draw(gunWheel1.Texture, new Rectangle(gunWheel1.X, gunWheel1.Y, gunWheel1.Width, gunWheel1.Height), null, Color.White, gunWheel1.Rotation, gunWheel1.TextureCenterOrigin, SpriteEffects.None, 0f);
            spriteBatch.Draw(gunWheel1.Texture, new Rectangle(gunWheel2.X, gunWheel2.Y, gunWheel2.Width, gunWheel2.Height), null, Color.White, gunWheel2.Rotation, gunWheel2.TextureCenterOrigin, SpriteEffects.None, 0f);

            // gun
            spriteBatch.Draw(gun.Texture, new Rectangle(gun.X, gun.Y, gun.Width, gun.Height), null, Color.White, gun.Rotation, new Vector2(100, gun.Texture.Height / 2), SpriteEffects.None, 0f);
            */

            gun.Draw(spriteBatch);

            // crosshair
            spriteBatch.Draw(crossHairTexture, crossHair, Color.White);

            //pause and fps count
            Vector2 pauseStringSize = pauseFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(pauseFont, "PAUSED", new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2 - pauseStringSize.X / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;
        }

        void cleanup()
        {
        }

        void updateGunAngle(GameTime gameTime)
        {
            //gun.Rotation = (float)Math.Atan2(crossHair.CenterPoint.Y - gunPosition.Y, crossHair.CenterPoint.X - gunPosition.X);
            gun.turnTowards(crossHair.CenterPoint, gun.Position, gun.TurnSpeed, gameTime);
        }

        void checkForShoot(GameTime gameTime)
        {
            /*timeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastShot >= shootDelay)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (Vector2.Distance(gunPosition, crossHair.CenterPoint) < gun.Width)
                        return;

                    timeSinceLastShot = 0;

                    Vector2 shootPoint = gunPosition + new Vector2(gun.Width * (float)Math.Cos(gun.Rotation), gun.Width * (float)Math.Sin(gun.Rotation));

                    //float distance = Vector2.Distance(shootPoint, crossHair.CenterPoint);
                    float distance = Vector2.Distance(gunPosition, crossHair.CenterPoint) - gun.Width;

                    Vector2 target = shootPoint + new Vector2(distance * (float)Math.Cos(gun.Rotation), distance * (float)Math.Sin(gun.Rotation));

                    Bullet b = new ExplodingBullet(BulletType.Peanut, bulletSpeed, target);
                    b.Width = bulletSize;
                    b.Height = bulletSize;
                    b.CenterPoint = shootPoint;
                }
            }*/
            gun.CheckForShoot(gameTime, crossHair.CenterPoint);
        }

        void rotatePeanuts(GameTime gameTime)
        {
            foreach (Bullet p in Bullet.Peanuts)
                p.Rotation += Util.ScaleWithGameTime(15, gameTime);
        }

        void updateBlasts(GameTime gameTime)
        {
            // add new blasts from queue
            for (int i = 0; i < ExplodingBullet.Explosions.Count; )
            {
                Vector2 explosionPoint = ExplodingBullet.Explosions[i];

                Blast b = new Blast(explosionPoint, 15, 25);
                b.Texture = blastTexture;
                b.Rotation = MathHelper.TwoPi * (float)rand.NextDouble();

                ExplodingBullet.Explosions.Remove(explosionPoint);
            }

            Blast.UpdateBlasts(gameTime);
        }

        void updateExplosions()
        {
            /*// add new explosions from queue
            for (int i = 0; i < ExplodingBullet.Explosions.Count; )
            {
                Vector2 explosionPoint = ExplodingBullet.Explosions[i];

                Blast b = new Blast(explosionPoint, 10, 50);
                b.Texture = blastTexture;
                b.Rotation = MathHelper.TwoPi * (float)rand.NextDouble();

                ExplodingBullet.Explosions.Remove(explosionPoint);
            }*/
        }

        void moveGun(GameTime gameTime)
        {
            gun.MoveTowardsCrossHair(gameTime, crossHair.CenterPoint, GraphicsDevice);
        }
    }
}