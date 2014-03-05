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
    class Battle1 : GameState
    {
        static bool contentLoaded = false;
        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;
        protected static bool paused, allowPause;
        protected static int cleanupCounter;

        protected static SoundEffect deathSound;
        static SoundEffect billHit;
        protected static SoundEffect billDeath;
        protected static SoundEffect getPowerUp, canDeath, useHammer;
        static Song music;

        protected HealthBar billHealthBar;

        int deaths = 0;
        protected Random rand = new Random();
        protected DateTime startTime;

        protected static SpriteFont font1;
        protected static SpriteFont powerUpFont;
        protected static SpriteFont billStatusFont;
        static Texture2D isaacTexture, billTexture, isaacUsingHammerTexture;
        static Texture2D shieldTexture, powerUpBarTexture, sledgeHammerTexture;

        protected Character bill;
        //Texture2D billTexture, billTexture2;
        protected Vector2 billTarget;
        public const int BILLWIDTH = 100, BILLHEIGHT = 100;

        protected Character isaac;
        public const int ISAACWIDTH = 50, ISAACHEIGHT = 50;

        static BaseObject sledgeHammer, sledgeHammerOrigin;
        static bool sledgeHammerInUse;
        //Direction sledgeHammerDirection;
        public const int SLEDGEHAMMERWIDTH = ISAACWIDTH / 2, SLEDGEHAMMERHEIGHT = (int)(ISAACHEIGHT * 1.75f);
        public const float SLEDGEHAMMERROTATIONSPEED = 30f;

        DateTime timeOfLastDeath, timeBillGotAngry;
        bool billIsAngry;
        string billHPString, billStatusString;
        string[] billMessages;
        protected string currentBillMessage;
        static string billStatus;
        protected const string LOWHPMESSAGE = "MUHHHHH";
        static double billSprayFireDelay = (double)(200 / GameSettings.BillSprayFireSpeed);
        static double timeSinceLastSpray = billSprayFireDelay;

        public Battle1(EventHandler callback, bool startMusic)
            : base(callback)
        {
            // initialize
            Game1.Game.IsMouseVisible = false;
            bill = new Character(new Rectangle(0, 0, BILLWIDTH, BILLHEIGHT), new Vector2(GameSettings.BillSpeed, GameSettings.BillSpeed));
            isaac = new Character(new Rectangle(Graphics.GraphicsDevice.Viewport.Width / 2 - ISAACWIDTH / 2, 0, ISAACWIDTH, ISAACHEIGHT), new Vector2(GameSettings.IsaacSpeed, GameSettings.IsaacSpeed));
            billHealthBar = new HealthBar(new Rectangle(0, 0, bill.Width, bill.Height / 12));
            billTarget = new Vector2(rand.Next(BILLWIDTH / 2, Graphics.GraphicsDevice.Viewport.Width - BILLWIDTH / 2), rand.Next(BILLHEIGHT / 2, Graphics.GraphicsDevice.Viewport.Height - BILLHEIGHT / 2));
            Bullet.Peanuts.Clear();
            Bullet.Cans.Clear();
            PowerUp.RemoveAlivePowerUps();
            startTime = timeOfLastDeath = DateTime.Now;
            bill.HP = bill.MaxHP = GameSettings.BillHP;
            bill.ShootDelay = (int)(1000 / GameSettings.BillShootSpeed);
            isaac.ShootDelay = (int)(250 / GameSettings.IsaacShootSpeed);
            isaac.TimeSinceLastShot = isaac.ShootDelay;
            isaac.Y = Graphics.GraphicsDevice.Viewport.Height - ISAACHEIGHT;
            billMessages = new string[] { "MUH", "MEH", "R U DUMB"};
            billHPString = "";
            billStatusString = "";
            sledgeHammer = new BaseObject(new Rectangle(0, 0, SLEDGEHAMMERWIDTH, SLEDGEHAMMERHEIGHT), 0f);
            sledgeHammerOrigin = new BaseObject(new Rectangle(0, 0, 1, SLEDGEHAMMERHEIGHT), 0f);
            allowPause = true;
            cleanupCounter = 0;
            generateIsaacSpawnPoints();

            // load content
            if (!contentLoaded)
            {
                deathSound = Content.Load<SoundEffect>("death");
                billHit = Content.Load<SoundEffect>("billhit");
                billDeath = Content.Load<SoundEffect>("billdeath");
                getPowerUp = Content.Load<SoundEffect>("getpowerup");
                canDeath = Content.Load<SoundEffect>("candeath");
                useHammer = Content.Load<SoundEffect>("usehammersound");
                music = Content.Load<Song>("battlemusic");
                billTexture = Content.Load<Texture2D>("bill");
                //billTexture = Content.Load<Texture2D>("billtransparent");
                //billTexture2 = Content.Load<Texture2D>("bill2transparent");
                isaacTexture = Content.Load<Texture2D>("isaac");
                isaacUsingHammerTexture = Content.Load<Texture2D>("isaacusinghammer");
                font1 = Content.Load<SpriteFont>("Battle1Font1");
                powerUpFont = Content.Load<SpriteFont>("Battle1PowerUpFont");
                billStatusFont = Content.Load<SpriteFont>("Battle1BillStatusFont");
                shieldTexture = Content.Load<Texture2D>("shield");
                powerUpBarTexture = Content.Load<Texture2D>("poweruptimebar");
                sledgeHammerTexture = Content.Load<Texture2D>("sledgehammer");
                contentLoaded = true;
            }

            bill.Texture = billTexture;
            isaac.Texture = isaacTexture;
            sledgeHammer.Texture = sledgeHammerTexture;

            if (startMusic)
            {
                MediaPlayer.Play(music);
                MediaPlayer.Volume = .25f;
                MediaPlayer.IsRepeating = true;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // check for exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game1.Game.Exit();
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                returnControl("title");
                return;
            }

            if (checkForBillDeath())
            {
                Stats.PeanutShots -= Bullet.Peanuts.Count;
                returnControl("endbattle1");
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
                    //MediaPlayer.Pause();
                    MediaPlayer.Volume /= 4;
                    foreach (PowerUp p in PowerUp.AlivePowerUps)
                        p.AliveTimer.Stop();
                    foreach (PowerUp p in isaac.ActivePowerUps)
                        p.ActiveTimer.Stop();
                }
                else
                {
                    //MediaPlayer.Resume();
                    MediaPlayer.Volume *= 4;
                    foreach (PowerUp p in PowerUp.AlivePowerUps)
                        p.AliveTimer.Start();
                    foreach (PowerUp p in isaac.ActivePowerUps)
                        p.ActiveTimer.Start();
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
            //Game1.Game.Window.Title = SoundEffectManager.soundEffectInstances.Count + " sound effect instances";

            billStatus = bill.HP <= (bill.MaxHP * .5) ? (bill.HP <= (bill.MaxHP * .1) ? "PISSED" : "Upset") : "Normal";

            if (billStatus == "Normal")
                moveBillRandom(gameTime);
            else if (billStatus == "Upset" && (int)(DateTime.Now - startTime).TotalSeconds % 8 != 7)
                moveBillRandom(gameTime);
            else
            {
                checkForBillSpray(gameTime);
                moveBillAngry(gameTime);
                if (!billIsAngry)
                {
                    billIsAngry = true;
                    timeBillGotAngry = DateTime.Now;
                    if (billStatus == "PISSED")
                        currentBillMessage = LOWHPMESSAGE;
                    else
                        currentBillMessage = billMessages[rand.Next(billMessages.Length)];
                }
            }

            if ((DateTime.Now - timeBillGotAngry).TotalSeconds >= 1)
                billIsAngry = false;
            if (currentBillMessage == LOWHPMESSAGE && billStatus != "PISSED")
                billIsAngry = false;

            moveIsaac(gameTime);

            Bullet.moveBullets(gameTime);
            rotatePeanuts(gameTime);
            rotateCans(gameTime);

            checkForPowerUpSpawnExpire(gameTime);

            // calculate rotated positions of everything
            // sledgehammer is calculated in own method
            isaac.CalculateCorners();
            foreach (Bullet b in Bullet.Cans)
                b.CalculateCorners();
            if (GameSettings.ProjectileRotationCollision)
            {
                bill.CalculateCorners();
                foreach (Bullet b in Bullet.Peanuts)
                    b.CalculateCorners();
            }

            checkForPowerUpAcquisition();

            checkForIsaacDeath();

            checkForBulletHits();

            checkForBillShoot(gameTime);
            checkForIsaacShoot(gameTime);

            checkForSledgeHammerUse(gameTime);
            checkForSledgeHammerHits();

            // clean up sound effect instances
            /*if (++cleanupCounter >= 60)
            {
                SoundEffectManager.Cleanup();
                cleanupCounter = 0;
            }*/

            //Game1.Game.Window.Title = "Bill's HP: " + bill.HP + "/" + bill.MaxHP + " Status: " + status;
            billHPString = "Bill's HP: " + bill.HP + "/" + bill.MaxHP;
            billStatusString = "Status: " + billStatus;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Gray);

            // powerups
            foreach (PowerUp p in PowerUp.AlivePowerUps)
                spriteBatch.Draw(p.Texture, p, Color.White);

            // powerup time bars
            foreach (PowerUp p in PowerUp.AlivePowerUps)
                spriteBatch.Draw(powerUpBarTexture, new Rectangle(p.X, p.Y + p.Height - p.Height / 15, (int)(p.Width * p.RemainingTimeAlive / p.AliveDuration), p.Height / 15), Color.White);

            // bill
            spriteBatch.Draw(bill.Texture, bill, Color.White);

            // bill's health bar
            Color billHpColor = bill.HP <= (bill.MaxHP * .75) ? (bill.HP <= (bill.MaxHP * .25) ? Color.Red : Color.Yellow) : Color.Green;
            spriteBatch.Draw(HealthBar.Texture, new Rectangle(bill.X, bill.Y, billHealthBar.Width, billHealthBar.Height), new Rectangle(0, 45, billHealthBar.Width, 44), Color.Gray);
            spriteBatch.Draw(HealthBar.Texture, new Rectangle(bill.X, bill.Y, (billHealthBar.Width * bill.HP / bill.MaxHP), billHealthBar.Height), new Rectangle(0, 45, billHealthBar.Width, 44), billHpColor);
            spriteBatch.Draw(HealthBar.Texture, new Rectangle(bill.X, bill.Y, billHealthBar.Width, billHealthBar.Height), new Rectangle(0, 0, HealthBar.Texture.Width, 44), Color.White);

            // isaac
            if (sledgeHammerInUse)
                spriteBatch.Draw(isaacUsingHammerTexture, isaac, Color.White);
            else
                spriteBatch.Draw(isaac.Texture, isaac, Color.White);

            // draw shield if active
            if (isaac.hasPowerUp(PowerUpType.CanShield))
                spriteBatch.Draw(shieldTexture, isaac, Color.White);

            // projectiles
            foreach (Bullet b in Bullet.Peanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);
            foreach (Bullet b in Bullet.Cans)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            // sledgehammer
            if (sledgeHammerInUse)
            {
                //spriteBatch.Draw(isaac.Texture, new Rectangle((int)sledgeHammer.UpperLeftCorner().X, (int)sledgeHammer.UpperLeftCorner().Y, 5, 5), Color.White);
                //spriteBatch.Draw(isaac.Texture, new Rectangle((int)sledgeHammer.UpperRightCorner().X, (int)sledgeHammer.UpperRightCorner().Y, 5, 5), Color.White);
                //spriteBatch.Draw(isaac.Texture, new Rectangle((int)sledgeHammer.LowerLeftCorner().X, (int)sledgeHammer.LowerLeftCorner().Y, 5, 5), Color.White);
                //spriteBatch.Draw(isaac.Texture, new Rectangle((int)sledgeHammer.LowerRightCorner().X, (int)sledgeHammer.LowerRightCorner().Y, 5, 5), Color.White); 
                spriteBatch.Draw(sledgeHammer.Texture, new Rectangle((int)isaac.CenterPoint.X, (int)isaac.CenterPoint.Y, sledgeHammer.Width, sledgeHammer.Height), null, Color.White, sledgeHammer.Rotation, new Vector2(sledgeHammer.Texture.Width / 2, sledgeHammer.Texture.Height), SpriteEffects.None, 0f);
            }

            // bill messages
            if (billIsAngry)
                spriteBatch.DrawString(font1, currentBillMessage, new Vector2(bill.X + bill.Width / 2 - font1.MeasureString(currentBillMessage).X / 2, bill.Y + bill.Height), Color.White);

            // active powerup info
            int offset = 1;
            foreach (PowerUp p in isaac.ActivePowerUps)
            {
                string msg = p.ToString();
                spriteBatch.DrawString(powerUpFont, msg, new Vector2(1, offset), Color.White);
                offset += (int)powerUpFont.MeasureString(msg).Y;
            }

            // bill status
            offset = 1;
            Vector2 billHPStringSize = billStatusFont.MeasureString(billHPString);
            int posX = Graphics.GraphicsDevice.Viewport.Width - (int)billHPStringSize.X - 3;
            spriteBatch.DrawString(billStatusFont, billHPString, new Vector2(posX, offset), Color.Black);
            offset += (int)billHPStringSize.Y;
            posX = Graphics.GraphicsDevice.Viewport.Width - (int)billStatusFont.MeasureString(billStatusString).X - 3;
            spriteBatch.DrawString(billStatusFont, billStatusString, new Vector2(posX, offset), Color.Black);

            //pause and fps count
            Vector2 pauseStringSize = billStatusFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(billStatusFont, "PAUSED", new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2 - pauseStringSize.X / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;

            /*foreach (Vector2 v in isaacSpawnPoints)
            {
                spriteBatch.Draw(isaac.Texture, new Rectangle((int)v.X, (int)v.Y, 2, 2), Color.White);
            */
        }

        void moveBillBouncy(GameTime gameTime)
        {
            bill.X += (int)Math.Round(Util.ScaleWithGameTime(bill.speed.X, gameTime));
            bill.Y += (int)Math.Round(Util.ScaleWithGameTime(bill.speed.Y, gameTime));

            // Check for bounce.
            if (bill.X > bill.maxX)
            {
                bill.speed.X *= -1;
                bill.X = bill.maxX;
            }
            else if (bill.X < bill.minX)
            {
                bill.speed.X *= -1;
                bill.X = bill.minX;
            }
            if (bill.Y > bill.maxY)
            {
                bill.speed.Y *= -1;
                bill.Y = bill.maxY;
            }
            else if (bill.Y < bill.minY)
            {
                bill.speed.Y *= -1;
                bill.Y = bill.minY;
            }
        }

        void moveBillAngry(GameTime gameTime)
        {
            billTarget = new Vector2(rand.Next(BILLWIDTH / 2, Graphics.GraphicsDevice.Viewport.Width - BILLWIDTH / 2), rand.Next(BILLHEIGHT / 2, Graphics.GraphicsDevice.Viewport.Height - BILLHEIGHT / 2));
            bill.moveTowards(billTarget, gameTime, true);
        }

        void moveBillRandom(GameTime gameTime)
        {
            checkIfBillMovementTargetHit(gameTime);
            bill.moveTowardsWeird(billTarget, gameTime, true);
        }

        void checkIfBillMovementTargetHit(GameTime gameTime)
        {
            if (bill.CenterPoint == billTarget)
                billTarget = new Vector2(rand.Next(bill.Width / 2, Graphics.GraphicsDevice.Viewport.Width - bill.Width / 2), rand.Next(bill.Height / 2, Graphics.GraphicsDevice.Viewport.Height - bill.Height / 2));
        }

        void checkForBillShoot(GameTime gameTime)
        {
            bill.TimeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (bill.TimeSinceLastShot >= bill.ShootDelay)
            {
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], isaac.CenterPoint, GameSettings.CanSpeed);
                bill.TimeSinceLastShot = 0;
            }
        }

        void checkForBillSpray(GameTime gameTime)
        {
            timeSinceLastSpray += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (billStatus == "Upset")
            {
                if (timeSinceLastSpray >= billSprayFireDelay)
                {
                    timeSinceLastSpray = 0;
                    billSprayFireRandom();
                    //billSprayFireAtIsaac();
                }
            }
            else if (billStatus == "PISSED")
            {
                if ((DateTime.Now - startTime).TotalMilliseconds % 1000 > 250)
                {
                    if (timeSinceLastSpray >= billSprayFireDelay)
                    {
                        timeSinceLastSpray = 0;
                        if (rand.Next(2) == 1)
                            billSprayFireCardinal();
                        else
                            billSprayFireIntercardinal();
                    }
                }
            }
        }

        void billSprayFireAtIsaac()
        {
            for (int i = 0; i < 4; i++)
            {
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], isaac.CenterPoint, GameSettings.CanSpeed);
            }
        }
        void billSprayFireRandom()
        {
            //Vector2 target;
            for (int i = 0; i < 2; i++)
            {
                Vector2 target = new Vector2(rand.Next(Graphics.GraphicsDevice.Viewport.Width), rand.Next(Graphics.GraphicsDevice.Viewport.Height));
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], target, GameSettings.CanSpeed);
            }
        }
        void billSprayFireCardinal()
        {
            foreach (Direction d in Direction.Cardinals)
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], d, GameSettings.CanSpeed);
        }
        void billSprayFireIntercardinal()
        {
            foreach (Direction d in Direction.Intercardinals)
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], d, GameSettings.CanSpeed);
        }
        void billSprayFireAllDirections()
        {
            foreach (Direction d in Direction.Directions)
                bill.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], d, GameSettings.CanSpeed);
        }

        bool checkForBillDeath()
        {
            if (bill.HP == 0)
                return true;
            return false;
        }

        protected void moveIsaac(GameTime gameTime)
        {
            float speed = isaac.Speed;

            // check for speed boost powerup
            foreach (PowerUp p in isaac.ActivePowerUps)
                if (p.Type == PowerUpType.SpeedBoost)
                {
                    speed = speed * (float)GameSettings.SpeedBoostModifier;
                    break;
                }

            float moveX = 0, moveY = 0;
            Keys[] keys = Keyboard.GetState().GetPressedKeys();
            foreach (Keys k in keys)
            {
                if (k == Keys.S)
                    moveY += speed;
                else if (k == Keys.W)
                    moveY += -speed;
                else if (k == Keys.D)
                    moveX += speed;
                else if (k == Keys.A)
                    moveX += -speed;
            }

            //float angle = (float)Math.Atan2((double)(moveY), (double)(moveX));
            //moveX = speed * (float)Math.Cos(angle);
            //moveY = speed * (float)Math.Sin(angle);

            isaac.movePrecise(new Vector2(moveX, moveY), gameTime, true);
        }

        void checkForIsaacShoot(GameTime gameTime)
        {
            isaac.TimeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (isaac.TimeSinceLastShot >= isaac.ShootDelay)
            {
                Direction direction;
                if (Keyboard.GetState().IsKeyDown(Keys.Up) && Keyboard.GetState().IsKeyDown(Keys.Right))
                    direction = Direction.NorthEast;
                else if (Keyboard.GetState().IsKeyDown(Keys.Up) && Keyboard.GetState().IsKeyDown(Keys.Left))
                    direction = Direction.NorthWest;
                else if (Keyboard.GetState().IsKeyDown(Keys.Down) && Keyboard.GetState().IsKeyDown(Keys.Right))
                    direction = Direction.SouthEast;
                else if (Keyboard.GetState().IsKeyDown(Keys.Down) && Keyboard.GetState().IsKeyDown(Keys.Left))
                    direction = Direction.SouthWest;
                else if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    direction = Direction.North;
                else if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    direction = Direction.South;
                else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    direction = Direction.East;
                else if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    direction = Direction.West;
                else return;

                isaac.shootBullet(BulletType.Peanut, direction, GameSettings.PeanutSpeed);
                Stats.PeanutShots++;

                isaac.TimeSinceLastShot = 0;
            }
        }

        void checkForSledgeHammerUse(GameTime gameTime)
        {
            /*if (sledgeHammerInUse)
            {
                if (sledgeHammerDirection == Direction.West)
                {
                    sledgeHammerOrigin.Rotation -= SLEDGEHAMMERROTATIONSPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sledgeHammer.Rotation -= SLEDGEHAMMERROTATIONSPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //if (sledgeHammer.Rotation <= -10.99f)
                    if (sledgeHammer.Rotation <= -3.14f)
                    {
                        sledgeHammerInUse = false;
                        return;
                    }
                }
                else if (sledgeHammerDirection == Direction.East)
                {
                    sledgeHammerOrigin.Rotation += SLEDGEHAMMERROTATIONSPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sledgeHammer.Rotation += SLEDGEHAMMERROTATIONSPEED * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    //if (sledgeHammer.Rotation >= 10.99f)
                    if (sledgeHammer.Rotation >= 3.14f)
                    {
                        sledgeHammerInUse = false;
                        return;
                    }
                }
                sledgeHammerOrigin.X = (int)isaac.CenterPoint.X - sledgeHammer.Width;
                sledgeHammerOrigin.Y = (int)isaac.CenterPoint.Y - sledgeHammer.Height;
                sledgeHammer.X = (int)sledgeHammerOrigin.UpperRightCorner().X + sledgeHammer.Width / 2;
                sledgeHammer.Y = (int)sledgeHammerOrigin.UpperRightCorner().Y;
                //4.71f
            }

            if (!sledgeHammerInUse && Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                //sledgeHammerOrigin.Rotation = 1.62f;
                //sledgeHammer.Rotation = 1.62f;
                sledgeHammerOrigin.Rotation = 0f;
                sledgeHammer.Rotation = 0f;
                sledgeHammerOrigin.X = (int)isaac.CenterPoint.X - sledgeHammer.Width;
                sledgeHammerOrigin.Y = (int)isaac.CenterPoint.Y - sledgeHammer.Height;
                sledgeHammer.X = (int)sledgeHammerOrigin.UpperRightCorner().X + sledgeHammer.Width / 2;
                sledgeHammer.Y = (int)sledgeHammerOrigin.UpperRightCorner().Y;
                sledgeHammerInUse = true;
                sledgeHammerDirection = Direction.West;
            }
            else if (!sledgeHammerInUse && Keyboard.GetState().IsKeyDown(Keys.E))
            {
                //sledgeHammerOrigin.Rotation = -1.62f;
                //sledgeHammer.Rotation = -1.62f;
                sledgeHammerOrigin.Rotation = 0f;
                sledgeHammer.Rotation = 0f;
                sledgeHammerOrigin.X = (int)isaac.CenterPoint.X - sledgeHammer.Width;
                sledgeHammerOrigin.Y = (int)isaac.CenterPoint.Y - sledgeHammer.Height;
                sledgeHammer.X = (int)sledgeHammerOrigin.UpperRightCorner().X + sledgeHammer.Width / 2;
                sledgeHammer.Y = (int)sledgeHammerOrigin.UpperRightCorner().Y;
                sledgeHammerInUse = true;
                sledgeHammerDirection = Direction.East;
            }*/

            if (sledgeHammerInUse && (!isaac.hasPowerUp(PowerUpType.SledgeHammer) || !isaac.getActivePowerUp(PowerUpType.SledgeHammer).ActiveTimer.IsRunning))
                sledgeHammerInUse = false;

            if (sledgeHammerInUse)
            {
                sledgeHammerOrigin.Rotation += Util.ScaleWithGameTime(SLEDGEHAMMERROTATIONSPEED, gameTime);
                sledgeHammer.Rotation += Util.ScaleWithGameTime(SLEDGEHAMMERROTATIONSPEED, gameTime);
                sledgeHammerOrigin.X = (int)isaac.CenterPoint.X - sledgeHammer.Width;
                sledgeHammerOrigin.Y = (int)isaac.CenterPoint.Y - sledgeHammer.Height;
                sledgeHammer.X = (int)sledgeHammerOrigin.UpperRightCorner.X + sledgeHammer.Width / 2;
                sledgeHammer.Y = (int)sledgeHammerOrigin.UpperRightCorner.Y;
                sledgeHammerOrigin.CalculateCorners();
                sledgeHammer.CalculateCorners();
            }

            if (!sledgeHammerInUse &&
                Keyboard.GetState().IsKeyDown(Keys.Space) &&
                isaac.hasPowerUp(PowerUpType.SledgeHammer) &&
                isaac.getActivePowerUp(PowerUpType.SledgeHammer).Charges > 0 &&
                (DateTime.Now - timeOfLastDeath).TotalSeconds > .5)
            {
                PowerUp p = isaac.getActivePowerUp(PowerUpType.SledgeHammer);
                p.ActiveTimer.Restart();
                soundEffectManager.Play(useHammer, .5f);

                sledgeHammerOrigin.Rotation = 0f;
                sledgeHammer.Rotation = 0f;
                sledgeHammerOrigin.X = (int)isaac.CenterPoint.X - sledgeHammer.Width;
                sledgeHammerOrigin.Y = (int)isaac.CenterPoint.Y - sledgeHammer.Height;
                sledgeHammer.X = (int)sledgeHammerOrigin.UpperRightCorner.X + sledgeHammer.Width / 2;
                sledgeHammer.Y = (int)sledgeHammerOrigin.UpperRightCorner.Y;
                sledgeHammerInUse = true;
            }
        }

        void checkForSledgeHammerHits()
        {
            if (sledgeHammerInUse)
            {
                for (int i = 0; i < Bullet.Cans.Count; )
                {
                    Bullet b = Bullet.Cans[i];
                    if (sledgeHammer.Intersects(b))
                    {
                        soundEffectManager.Play(canDeath, .5f);
                        Bullet.Cans.Remove(b);
                    }
                    else
                        i++;
                }
            }
        }

        protected void checkForIsaacDeath()
        {
            if (sledgeHammerInUse)
                return;

            if (bill.Rectangle.Intersects(isaac.Rectangle))
            {
                isaacDeath();
                return;
            }

            for (int i = 0; i < Bullet.Cans.Count; )
            {
                Bullet b = Bullet.Cans[i];
                if (isaac.Intersects(b))
                {
                    if (isaac.hasPowerUp(PowerUpType.CanShield))
                    {
                        PowerUp p = isaac.getActivePowerUp(PowerUpType.CanShield);
                        if (--p.Charges <= 0)
                            isaac.removePowerUp(PowerUpType.CanShield);
                        Bullet.Cans.Remove(b);
                        continue;
                    }
                    else
                    {
                        isaacDeath();
                        return;
                    }
                }
                i++;
            }
        }

        void isaacDeath()
        {
            soundEffectManager.Play(deathSound, 1f);
            deaths++;
            timeOfLastDeath = DateTime.Now;
            isaac.removePowerUpsExcept(PowerUpType.SledgeHammer, PowerUpType.CanShield);
            isaac.reduceCanShieldCharges();
            Stats.PeanutShots -= Bullet.Peanuts.Count;
            Bullet.Peanuts.Clear();
            sledgeHammerInUse = false;

            int startIndex = rand.Next(isaacSpawnPoints.Count);
            float adjustedSpawnSafetyFactor = (float)GameSettings.SpawnSafetyFactor;

            while (true)
            {
                if (tryToSpawnIsaac(startIndex, adjustedSpawnSafetyFactor))
                    break;
                adjustedSpawnSafetyFactor *= .9f;
            }

            bill.HP += GameSettings.BillLifeStealAmount;

            //int spawnCount = 0;
            /*do
            {
            spawn:
                if (++spawnCount == 400)
                {
                    adjustedSpawnSafetyFactor *= .9f;
                    spawnCount = 0;
                }
                isaac.X = rand.Next(isaac.maxX + 1);
                isaac.Y = rand.Next(isaac.maxY + 1);
                foreach (Bullet b in Bullet.Cans)
                    if (isaac.IsNear(b, adjustedSpawnSafetyFactor))
                        goto spawn;
            }
            while (isaac.IsNear(bill, adjustedSpawnSafetyFactor));
            */
        }

        bool tryToSpawnIsaac(int index, float factor)
        {
            for (int i = 0; i < isaacSpawnPoints.Count; i++, index++)
            {
                isaac.X = (int)isaacSpawnPoints[index % isaacSpawnPoints.Count].X;
                isaac.Y = (int)isaacSpawnPoints[index % isaacSpawnPoints.Count].Y;

                if (isaac.IsNear(bill, factor))
                    continue;
                if (isaacIsNearCan(factor))
                    continue;
                return true;
            }
            return false;
        }

        bool isaacIsNearCan(float factor)
        {
            foreach (Bullet b in Bullet.Cans)
                if (isaac.IsNear(b, factor))
                    return true;
            return false;
        }

        List<Vector2> isaacSpawnPoints;
        void generateIsaacSpawnPoints()
        {
            isaacSpawnPoints = new List<Vector2>();

            for (int x = 0; x < Graphics.GraphicsDevice.Viewport.Width; x += Graphics.GraphicsDevice.Viewport.Width / 16)
            {
                for (int y = 0; y < Graphics.GraphicsDevice.Viewport.Height; y += Graphics.GraphicsDevice.Viewport.Height / 10)
                {
                    isaacSpawnPoints.Add(new Vector2(x, y));
                }
            }
        }

        static void updatePotentialCollisions(Bullet object1, List<Bullet> objects)
        {
            if (objects.Count == 0)
                return;

            object1.PotentialCollisions.Clear();

            for (int i = 0; i < objects.Count; i++)
            {
                BaseObject object2 = objects[i];

                if (object2.RightBound < object1.LeftBound)
                    continue;

                if (object2.LeftBound > object1.RightBound)
                    break;

                if (object2.TopBound <= object1.BottomBound &&
                    object2.BottomBound >= object1.TopBound)
                    object1.PotentialCollisions.Add(object2);
            }
        }

        void checkForBulletHits()
        {
            if (frameCounter % 2 == 0)
            {
                Util.SortByX(Bullet.Cans);
                foreach (Bullet p in Bullet.Peanuts)
                    updatePotentialCollisions(p, Bullet.Cans);
            }

            // hits on cans
            if (GameSettings.CansBlockPeanuts)
            {
                for (int i = 0; i < Bullet.Peanuts.Count; )
                {
                    Bullet p = Bullet.Peanuts[i];

                    foreach (Bullet c in p.PotentialCollisions)
                    {
                        if (c.Intersects(p))
                        {
                            Bullet.Peanuts.Remove(p);
                            Stats.PeanutShots--;
                            goto nextPeanut;
                        }
                    }

                    /*foreach (Bullet c in Bullet.Cans)
                    {
                        if (GameSettings.ProjectileRotationCollision)
                        {
                            if (c.IsNear(p.CenterPoint) && c.Intersects(p))
                            {
                                Bullet.Peanuts.Remove(p);
                                Stats.PeanutShots--;
                                goto nextPeanut;
                            }
                        }
                        else
                        {
                            if (c.Rectangle.Intersects(p.Rectangle))
                            {
                                Bullet.Peanuts.Remove(p);
                                Stats.PeanutShots--;
                                goto nextPeanut;
                            }
                        }
                    }*/
                    i++;
                nextPeanut: ;
                }
            }

            // hits on bill
            for (int i = 0; i < Bullet.Peanuts.Count; )
            {
                Bullet b = Bullet.Peanuts[i];
                if ((GameSettings.ProjectileRotationCollision && bill.Intersects(b)) ||
                    bill.Rectangle.Intersects(b.Rectangle))
                {
                    Bullet.Peanuts.Remove(b);
                    soundEffectManager.Play(billHit, 1f);
                    bill.HP -= GameSettings.PeanutDamage;
                    Stats.PeanutHits++;
                }
                else
                    i++;
            }
        }

        int timeSinceLastPowerUpSpawn = 0;
        void checkForPowerUpSpawnExpire(GameTime gameTime)
        {
            //spawn
            timeSinceLastPowerUpSpawn += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastPowerUpSpawn >= GameSettings.PowerUpSpawnDelay)
            {
                PowerUp p = PowerUp.Random;
                do
                {
                    p.X = rand.Next(p.maxX + 1);
                    p.Y = rand.Next(p.maxY + 1);
                }
                while (isaac.IsNear(p, 1f));
                PowerUp.AlivePowerUps.Add(p);
                timeSinceLastPowerUpSpawn = 0;
            }

            //expire
            for (int i = 0; i < PowerUp.AlivePowerUps.Count; )
            {
                PowerUp p = PowerUp.AlivePowerUps[i];
                if (p.AliveTimer.ElapsedMilliseconds >= p.AliveDuration)
                    PowerUp.AlivePowerUps.Remove(p);
                else
                    i++;
            }
        }

        void checkForPowerUpAcquisition()
        {
            //check for acquisition
            for (int i = 0; i < PowerUp.AlivePowerUps.Count; )
            {
                PowerUp p = PowerUp.AlivePowerUps[i];
                if (isaac.Rectangle.Intersects(p.Rectangle))
                {
                    soundEffectManager.Play(getPowerUp, .5f);
                    isaac.acquirePowerUp(p);
                    PowerUp.AlivePowerUps.Remove(p);
                }
                else
                    i++;
            }

            // update status of active powerups
            isaac.updatePowerUps();
        }

        void rotatePeanuts(GameTime gameTime)
        {
            foreach (Bullet p in Bullet.Peanuts)
                p.Rotation += Util.ScaleWithGameTime(10, gameTime);
        }

        void rotateCans(GameTime gameTime)
        {
            foreach (Bullet c in Bullet.Cans)
                c.Rotation += Util.ScaleWithGameTime(5, gameTime);
        }
    }
}