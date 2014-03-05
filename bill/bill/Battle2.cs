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
    class Battle2 : GameState
    {
        static bool contentLoaded = false;
        protected TimeSpan fpsElapsedTime;
        protected int frameCounter;
        protected bool paused, allowPause;
        Random rand = new Random();

        static SpriteFont pauseFont, powerUpFont;

        static Texture2D alexEnchanterTexture, isaacTexture, transparentTexture, powerUpBarTexture, sledgeHammerTexture;
        static Texture2D heartEmptyTexture, heartOneQuarterTexture, heartHalfTexture, heartThreeQuartersTexture, heartFullTexture;
        static Texture2D burntPeanutTexture;
        static Texture2D[] hearts;

        const int HEARTWIDTH = 19, HEARTHEIGHT = 17;

        static Song battle2Music;
        static SoundEffect[] hurtSounds;
        static SoundEffect isaacHurtBadSound, fireballHitSound, alexLaugh, 
            potionDrinkSound, getPowerUpSound, useHammerSound,
            alexEnchanterHitSound, peanutShieldDeflectSound, lowHealthBeep,
            peanutShieldBreakSound, superFireballRoll;

        LoopingSoundEffect lowHealthBeeping, superFireballRolling;

        Alex alex;
        AlexEnchanter alexEnchanter;
        static readonly int ENCHANTERHEIGHT = 100, ENCHANTERWIDTH = (int)(ENCHANTERHEIGHT * 1.07169811f);
        HealthBar alexHealthBar;

        Character isaac;
        const int ISAACWIDTH = 40, ISAACHEIGHT = 40;

        Animation isaacHurtAnimation;

        static readonly int ISAAC_MAX_HP = 20;
        static readonly int FIREBALL_DAMAGE = 1, LASER_DAMAGE = 2, ALEX_DAMAGE = 2, MOUSE_DAMAGE = 2, SUPERFIREBALL_DAMAGE = 5;

        static readonly float potionSpawnDelay = 20;
        static readonly int POTIONWIDTH = 13, POTIONHEIGHT = 15;

        static readonly float powerUpSpawnDelay = 10;
        static readonly float powerUpAliveDuration = 30;

        static BaseObject sledgeHammer, sledgeHammerOrigin;
        static bool sledgeHammerInUse;
        public const int SLEDGEHAMMERWIDTH = ISAACWIDTH / 2, SLEDGEHAMMERHEIGHT = (int)(ISAACHEIGHT * 1.75f);
        public const float SLEDGEHAMMERROTATIONSPEED = 30f;

        public Battle2(EventHandler callback)
            : base(callback)
        {
            if (!contentLoaded)
            {
                pauseFont = Content.Load<SpriteFont>("Battle1BillStatusFont");
                powerUpFont = Content.Load<SpriteFont>("Battle1PowerUpFont");
                alexEnchanterTexture = Content.Load<Texture2D>("alexenchanter");
                isaacTexture = Content.Load<Texture2D>("isaac");
                transparentTexture = Content.Load<Texture2D>("transparent");
                burntPeanutTexture = Content.Load<Texture2D>("peanutgray");
                battle2Music = Content.Load<Song>("battle2music");
                hurtSounds = new SoundEffect[3];
                hurtSounds[0] = Content.Load<SoundEffect>("hurt1");
                hurtSounds[1] = Content.Load<SoundEffect>("hurt2");
                hurtSounds[2] = Content.Load<SoundEffect>("hurt3");
                isaacHurtBadSound = Content.Load<SoundEffect>("isaachurtbad");
                alexLaugh = Content.Load<SoundEffect>("alexlaugh");
                alexEnchanterHitSound = Content.Load<SoundEffect>("OOT_Armos_Hit");
                //alexEnchanterHitSound = Content.Load<SoundEffect>("OOT_ReDead_Hump");
                peanutShieldDeflectSound = Content.Load<SoundEffect>("peanutShieldDeflect");
                potionDrinkSound = Content.Load<SoundEffect>("potiondrink");
                getPowerUpSound = Content.Load<SoundEffect>("getpowerup");
                useHammerSound = Content.Load<SoundEffect>("usehammersound");
                fireballHitSound = Content.Load<SoundEffect>("fireballhit");
                lowHealthBeep = Content.Load<SoundEffect>("lowhealthbeep");
                superFireballRoll = Content.Load<SoundEffect>("fireballrolling");
                peanutShieldBreakSound = Content.Load<SoundEffect>("peanutshieldbreaksound");
                powerUpBarTexture = Content.Load<Texture2D>("poweruptimebar");
                sledgeHammerTexture = Content.Load<Texture2D>("sledgehammer");
                hearts = new Texture2D[5];
                heartEmptyTexture = Content.Load<Texture2D>("heart_empty");
                heartOneQuarterTexture = Content.Load<Texture2D>("heart_onequarter");
                heartHalfTexture = Content.Load<Texture2D>("heart_half");
                heartThreeQuartersTexture = Content.Load<Texture2D>("heart_3quarters");
                heartFullTexture = Content.Load<Texture2D>("heart_Full");
                contentLoaded = true;
            }

            Game1.Game.IsMouseVisible = false;

            isaac = new Character(new Rectangle(Graphics.GraphicsDevice.Viewport.Width / 2 - ISAACWIDTH / 2, Graphics.GraphicsDevice.Viewport.Height - ISAACHEIGHT, ISAACWIDTH, ISAACHEIGHT), new Vector2(GameSettings.IsaacSpeed, GameSettings.IsaacSpeed));
            isaac.Texture = isaacTexture;
            isaac.HP = isaac.MaxHP = ISAAC_MAX_HP;
            isaac.ShootDelay = (int)(250 / GameSettings.IsaacShootSpeed);
            isaac.TimeSinceLastShot = isaac.ShootDelay;

            isaacHurtAnimation = new Animation(1, 50, transparentTexture, isaacTexture);
            lowHealthBeeping = new LoopingSoundEffect(lowHealthBeep, .25f);
            updateHearts();

            alexEnchanter = new AlexEnchanter(new Rectangle(Graphics.GraphicsDevice.Viewport.Width / 2 - ENCHANTERWIDTH / 2, 25, ENCHANTERWIDTH, ENCHANTERHEIGHT));
            alexEnchanter.Texture = alexEnchanterTexture;
            alex = alexEnchanter;
            alexHealthBar = new HealthBar(new Rectangle(0, 0, 200, 10));

            sledgeHammer = new BaseObject(new Rectangle(0, 0, SLEDGEHAMMERWIDTH, SLEDGEHAMMERHEIGHT), 0f);
            sledgeHammerOrigin = new BaseObject(new Rectangle(0, 0, 1, SLEDGEHAMMERHEIGHT), 0f);
            sledgeHammer.Texture = sledgeHammerTexture;

            superFireballRolling = new LoopingSoundEffect(superFireballRoll, .35f);

            soundEffectManager.Play(alexLaugh, .25f);

            MediaPlayer.Play(battle2Music);
            MediaPlayer.Volume = .25f;
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
                Game1.Game.Window.Title = "FPS: " + frameCounter;
                fpsElapsedTime -= TimeSpan.FromSeconds(1);
                frameCounter = 0;
            }

            isaacHurtAnimation.Update();
            //isaacHurtMove(gameTime);

            spawnPotions(gameTime);
            moveIsaac(gameTime);
            Bullet.moveBullets(gameTime);
            rotatePeanuts(gameTime);
            Fireball.moveFireballs(gameTime);
            rotateFireballs(gameTime);
            Laser2.UpdateLasers(gameTime);
            SeekerSnake.UpdateSeekerSnakes(gameTime);

            checkForPowerUpSpawnExpire(gameTime);

            isaac.CalculateCorners();
            alex.CalculateCorners();
            foreach (Laser2 laser in Laser2.Lasers)
                laser.CalculateCorners();
            foreach (Bullet p in Bullet.Peanuts)
                p.CalculateCorners();

            checkForPowerUpAcquisition();
            acquirePotions();

            isaacShoot(gameTime);

            checkForPeanutHits();

            checkForSledgeHammerUse(gameTime);
            checkForSledgeHammerHits();

            if (checkForIsaacHurt())
            {
                isaacDeath();
                cleanup();
                returnControl("title");
                return;
            }

            if (alex.HP == 0)
            {
                cleanup();
                returnControl("postgame");
                return;
            }

            alex.Update(gameTime, isaac);

            updateHearts();
            shrinkPeanuts(gameTime);
            burnPeanuts(gameTime);
            moveBurningPeanuts(gameTime);

            updateSuperFireballLoopingSound();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Game1.Game.DebugMonitor.AddLine(Bullet.Peanuts.Count + " peanuts.");
            Game1.Game.DebugMonitor.AddLine(Fireball.Fireballs.Count + " fireballs.");
            Game1.Game.DebugMonitor.AddLine(Laser2.Lasers.Count + " lasers.");
            Game1.Game.DebugMonitor.AddLine(MouseSeeker.MouseSeekers.Count + " mice.");

            GraphicsDevice.Clear(Color.Gray);

            // powerups
            foreach (PowerUp p in PowerUp.AlivePowerUps)
                spriteBatch.Draw(p.Texture, p, Color.White);

            // powerup time bars
            foreach (PowerUp p in PowerUp.AlivePowerUps)
                spriteBatch.Draw(powerUpBarTexture, new Rectangle(p.X, p.Y + p.Height - p.Height / 15, (int)(p.Width * p.RemainingTimeAlive / p.AliveDuration), p.Height / 15), Color.White);

            // alex
            // new Color(255, 255, 255, (byte)MathHelper.Clamp(100, 0, 255))
            spriteBatch.Draw(alex.Texture, alex, Color.White);

            // alex's peanut shield
            if (alex is AlexEnchanter && ((AlexEnchanter)alex).IsCastingPeanutShield || ((AlexEnchanter)alex).HasPeanutShield)
                spriteBatch.Draw(((AlexEnchanter)alex).PeanutShield, ((AlexEnchanter)alex).PeanutShield, Color.White);

            // isaac
            if (isaacHurtAnimation.IsRunning)
                spriteBatch.Draw(isaacHurtAnimation, isaac, Color.White);
            else
                spriteBatch.Draw(isaac.Texture, isaac, Color.White);
            
            // potions
            foreach (Potion p in Potion.Potions)
                spriteBatch.Draw(p.Texture, p, Color.White);

            // fireballs
            foreach (Fireball fireball in Fireball.Fireballs)
                spriteBatch.Draw(fireball.Texture, new Rectangle((int)fireball.CenterPoint.X, (int)fireball.CenterPoint.Y, fireball.Width, fireball.Height), null, Color.White, fireball.Rotation, fireball.TextureCenterOrigin, SpriteEffects.None, 0f);

            // lasers
            foreach (Laser2 laser in Laser2.Lasers)
                laser.Draw(spriteBatch);

            // mouse seekers
            SeekerSnake.DrawSeekerSnakes(spriteBatch);

            // staff
            if (alex is AlexEnchanter && ((AlexEnchanter)alex).IsUsingStaff)
            {
                BaseObject staff = ((AlexEnchanter)alex).MagicStaff;
                spriteBatch.Draw(staff.Texture, new Rectangle(staff.X, staff.Y, staff.Width, staff.Height), null, Color.White, staff.Rotation, new Vector2(0, staff.Texture.Height / 2), SpriteEffects.None, 0f);
            }

            // peanuts
            foreach (Bullet b in Bullet.Peanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);
            // shrinking peanuts
            foreach (Bullet b in shrinkingPeanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);
            // burning peanuts
            foreach (Bullet b in burningPeanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            // sledgehammer
            if (sledgeHammerInUse)
                spriteBatch.Draw(sledgeHammer.Texture, new Rectangle((int)isaac.CenterPoint.X, (int)isaac.CenterPoint.Y, sledgeHammer.Width, sledgeHammer.Height), null, Color.White, sledgeHammer.Rotation, new Vector2(sledgeHammer.Texture.Width / 2, sledgeHammer.Texture.Height), SpriteEffects.None, 0f);

            // hearts
            drawHearts(spriteBatch);

            // active powerup info
            int offset = HEARTHEIGHT + 10;
            foreach (PowerUp p in isaac.ActivePowerUps)
            {
                string msg = p.ToString();
                spriteBatch.DrawString(powerUpFont, msg, new Vector2(5, offset), Color.Black);
                offset += (int)powerUpFont.MeasureString(msg).Y;
            }
            
            // alex health bar
            int hpBarX = Graphics.GraphicsDevice.Viewport.Width / 2 - alexHealthBar.Width / 2;
            int hpBarY = 8;
            int borderSize = 2;
            spriteBatch.Draw(ColorTexture.Black, new Rectangle(hpBarX - borderSize, hpBarY - borderSize, alexHealthBar.Width + borderSize * 2, alexHealthBar.Height + borderSize * 2), Color.White);
            spriteBatch.Draw(ColorTexture.Gray, new Rectangle(hpBarX, hpBarY, alexHealthBar.Width, alexHealthBar.Height), Color.White);
            spriteBatch.Draw(ColorTexture.Red, new Rectangle(hpBarX, hpBarY, (alexHealthBar.Width * alex.HP / alex.MaxHP), alexHealthBar.Height), Color.White);
            //spriteBatch.Draw(HealthBar.Texture, new Rectangle(hpBarX, hpBarY, alexHealthBar.Width, alexHealthBar.Height), new Rectangle(0, 45, alexHealthBar.Width, 44), Color.Gray);
            //spriteBatch.Draw(HealthBar.Texture, new Rectangle(hpBarX, hpBarY, (alexHealthBar.Width * alex.HP / alex.MaxHP), alexHealthBar.Height), new Rectangle(0, 45, alexHealthBar.Width, 44), Color.Red);
            //spriteBatch.Draw(HealthBar.Texture, new Rectangle(hpBarX, hpBarY, alexHealthBar.Width, alexHealthBar.Height), new Rectangle(0, 0, HealthBar.Texture.Width, 44), Color.White);

            /*PrimitiveLine line = new PrimitiveLine(GraphicsDevice, 1);
            line.AddVector(new Vector2(hpBarX, hpBarY + 1));
            line.AddVector(new Vector2(hpBarX + alexHealthBar.Width, hpBarY + 1));
            line.AddVector(new Vector2(hpBarX + alexHealthBar.Width, hpBarY + alexHealthBar.Height));
            line.AddVector(new Vector2(hpBarX, hpBarY + alexHealthBar.Height));
            line.AddVector(new Vector2(hpBarX, hpBarY + 1));
            line.Colour = Color.Black;
            line.Render(spriteBatch);*/

            //pause and fps count
            Vector2 pauseStringSize = pauseFont.MeasureString("PAUSED");
            if (paused)
                spriteBatch.DrawString(pauseFont, "PAUSED", new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2 - pauseStringSize.X / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - pauseStringSize.Y / 2), Color.White);
            else
                frameCounter++;
        }

        void moveIsaac(GameTime gameTime)
        {
            //if (isaacIsHurtBad)
            //    return;

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

            float angle = (float)Math.Atan2(moveY, moveX);

            if (moveX != 0)
                moveX = speed * (float)Math.Cos(angle);
            moveY = speed * (float)Math.Sin(angle);

            isaac.movePrecise(new Vector2(moveX, moveY), gameTime, true);
        }

        //bool isaacIsHurtBad;
        float isaacHurtAngle;
        bool checkForIsaacHurt()
        {
            if (sledgeHammerInUse || isaacHurtAnimation.IsRunning)
                return false;

            if ((alex is AlexEnchanter && !((AlexEnchanter)alex).IsWarping)
                && alex.Rectangle.Intersects(isaac.Rectangle))
            {
                return isaacHurt(ALEX_DAMAGE);
            }

            // big fireball
            foreach (Fireball fireball in Fireball.Fireballs)
            {
                if (fireball.Width == AlexEnchanter.SUPERFIREBALLSIZE && fireball.Intersects(isaac))
                {
                    isaacHurtAngle = (float)Math.Atan2(isaac.CenterPoint.Y - fireball.CenterPoint.Y, isaac.CenterPoint.X - fireball.CenterPoint.X);
                    //isaacIsHurtBad = true;
                    soundEffectManager.Play(fireballHitSound, .5f);
                    return isaacHurt(SUPERFIREBALL_DAMAGE, isaacHurtBadSound, .25f);
                }
            }

            foreach (MouseSeeker mouse in MouseSeeker.MouseSeekers)
            {
                mouse.FrontBit.CalculateCorners();
                if (mouse.FrontBit.IsNear(isaac, .5f) && mouse.FrontBit.Intersects(isaac))
                {
                    return isaacHurt(MOUSE_DAMAGE);
                }
            }

            foreach (Laser2 laser in Laser2.Lasers)
            {
                if (laser.IsNear(isaac, laser.Width) &&
                    isaac.Intersects(laser))
                {
                    return isaacHurt(LASER_DAMAGE);
                }
            }

            // small fireballs
            foreach (Fireball fireball in Fireball.Fireballs)
            {
                //if (isaac.Rectangle.Intersects(fireball.Rectangle))
                if (fireball.Width != AlexEnchanter.SUPERFIREBALLSIZE && fireball.Intersects(isaac))
                {
                    return isaacHurt(FIREBALL_DAMAGE);
                }
            }

            return false;
        }

        bool isaacHurt(int damage)
        {
            return isaacHurt(damage, hurtSounds[rand.Next(3)], .25f);
        }

        bool isaacHurt(int damage, SoundEffect sound, float volume)
        {
            //soundEffectManager.Play(deathSound, 1f);
            //timeOfLastDeath = DateTime.Now;
            //isaac.removePowerUpsExcept(PowerUpType.SledgeHammer, PowerUpType.CanShield);
            //isaac.reduceCanShieldCharges();
            //Stats.PeanutShots -= Bullet.Peanuts.Count;
            //Bullet.Peanuts.Clear();
            //sledgeHammerInUse = false;

            soundEffectManager.Play(sound, volume);
            isaacHurtAnimation.Start();
            isaac.HP -= damage;

            if (isaac.HP == 0)
                return true;
            else
                return false;
        }

        /*float isaacHurtMoveSpeed = 175;
        void isaacHurtMove(GameTime gameTime)
        {
            if (isaacIsHurtBad)
            {
                if (isaacHurtAnimation.ElapsedSeconds * 3 > isaacHurtAnimation.Duration)
                {
                    isaacIsHurtBad = false;
                    return;
                }
                float moveX = isaacHurtMoveSpeed * (float)Math.Cos(isaacHurtAngle);
                float moveY = isaacHurtMoveSpeed * (float)Math.Sin(isaacHurtAngle);
                isaac.movePrecise(new Vector2(moveX, moveY), gameTime, true);
            }
        }*/

        void isaacDeath()
        {
        }

        bool isaacIsNearLaser(float factor)
        {
            foreach (Laser2 laser in Laser2.Lasers)
                if (isaac.IsNearRotated(laser, factor))
                    return true;
            return false;
        }

        bool isaacIsNearFireball(float factor)
        {
            foreach (Fireball fireball in Fireball.Fireballs)
                if (isaac.IsNear(fireball, factor))
                    return true;
            return false;
        }

        void rotateFireballs(GameTime gameTime)
        {
            foreach (Fireball fireball in Fireball.Fireballs)
            {
                //fireball.Rotation += Util.ScaleWithGameTime(-6, gameTime);
                fireball.Rotation += Util.ScaleWithGameTime(-150f / fireball.Width, gameTime);
            }
        }

        void updateHearts()
        {
            if (isaac.HP <= 4 && !lowHealthBeeping.IsPlaying)
                lowHealthBeeping.Start();
            else if (isaac.HP > 4 && lowHealthBeeping.IsPlaying)
                lowHealthBeeping.Stop();

            if (isaac.HP >= 16)
            {
                hearts[0] = hearts[1] = hearts[2] = hearts[3] = heartFullTexture;
                if (isaac.HP == 16)
                    hearts[4] = heartEmptyTexture;
                else if (isaac.HP == 17)
                    hearts[4] = heartOneQuarterTexture;
                else if (isaac.HP == 18)
                    hearts[4] = heartHalfTexture;
                else if (isaac.HP == 19)
                    hearts[4] = heartThreeQuartersTexture;
                else
                    hearts[4] = heartFullTexture;
            }
            else if (isaac.HP >= 12)
            {
                hearts[0] = hearts[1] = hearts[2] = heartFullTexture;
                if (isaac.HP == 12)
                    hearts[3] = heartEmptyTexture;
                else if (isaac.HP == 13)
                    hearts[3] = heartOneQuarterTexture;
                else if (isaac.HP == 14)
                    hearts[3] = heartHalfTexture;
                else if (isaac.HP == 15)
                    hearts[3] = heartThreeQuartersTexture;
                else
                    hearts[3] = heartFullTexture;
                hearts[4] = heartEmptyTexture;
            }
            else if (isaac.HP >= 8)
            {
                hearts[0] = hearts[1] = heartFullTexture;
                if (isaac.HP == 8)
                    hearts[2] = heartEmptyTexture;
                else if (isaac.HP == 9)
                    hearts[2] = heartOneQuarterTexture;
                else if (isaac.HP == 10)
                    hearts[2] = heartHalfTexture;
                else if (isaac.HP == 11)
                    hearts[2] = heartThreeQuartersTexture;
                else
                    hearts[2] = heartFullTexture;
                hearts[3] = hearts[4] = heartEmptyTexture;
            }
            else if (isaac.HP >= 4)
            {
                hearts[0] = heartFullTexture;
                if (isaac.HP == 4)
                    hearts[1] = heartEmptyTexture;
                else if (isaac.HP == 5)
                    hearts[1] = heartOneQuarterTexture;
                else if (isaac.HP == 6)
                    hearts[1] = heartHalfTexture;
                else if (isaac.HP == 7)
                    hearts[1] = heartThreeQuartersTexture;
                else
                    hearts[1] = heartFullTexture;
                hearts[2] = hearts[3] = hearts[4] = heartEmptyTexture;
            }
            else
            {
                if (isaac.HP == 0)
                    hearts[0] = heartEmptyTexture;
                else if (isaac.HP == 1)
                    hearts[0] = heartOneQuarterTexture;
                else if (isaac.HP == 2)
                    hearts[0] = heartHalfTexture;
                else if (isaac.HP == 3)
                    hearts[0] = heartThreeQuartersTexture;
                else
                    hearts[0] = heartFullTexture;
                hearts[1] = hearts[2] = hearts[3] = hearts[4] = heartEmptyTexture;
            }
        }

        void drawHearts(SpriteBatch spriteBatch)
        {
            int spacing = 2;

            Rectangle heart = new Rectangle(4, 5, HEARTWIDTH, HEARTHEIGHT);

            foreach (Texture2D t in hearts)
            {
                spriteBatch.Draw(t, heart, Color.White);
                heart.X += HEARTWIDTH + spacing;
            }
        }

        float timeSinceLastPotionSpawn = 0;
        void spawnPotions(GameTime gameTime)
        {
            //spawn
            timeSinceLastPotionSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastPotionSpawn >= potionSpawnDelay)
            {
                Potion p = new Potion(new Rectangle(0, 0, POTIONWIDTH, POTIONHEIGHT));
                do
                {
                    p.X = rand.Next(p.maxX + 1);
                    p.Y = rand.Next(p.maxY + 1);
                }
                while (isaac.IsNear(p, 1f));
                timeSinceLastPotionSpawn = 0;
            }
        }

        void acquirePotions()
        {
            for (int i = 0; i < Potion.Potions.Count; )
            {
                Potion p = Potion.Potions[i];
                if (isaac.Rectangle.Intersects(p.Rectangle))
                {
                    soundEffectManager.Play(potionDrinkSound, .15f);
                    isaac.HP += Potion.HealAmount;
                    p.Remove();
                }
                else
                    i++;
            }
        }

        float timeSinceLastPowerUpSpawn = 0;
        void checkForPowerUpSpawnExpire(GameTime gameTime)
        {
            //spawn
            timeSinceLastPowerUpSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastPowerUpSpawn >= powerUpSpawnDelay)
            {
                PowerUp p;
                if (((AlexEnchanter)alex).HasPeanutShield && !isaac.hasPowerUp(PowerUpType.SledgeHammer) && !PowerUp.IsPowerUpAlive(PowerUpType.SledgeHammer))
                    p = PowerUp.SledgeHammer;
                else
                {
                    do
                    {
                        p = PowerUp.Random;
                    }
                    while (p.Type == PowerUpType.CanShield);
                }
                do
                {
                    p.X = rand.Next(p.maxX + 1);
                    p.Y = rand.Next(p.maxY + 1);
                }
                while (isaac.IsNear(p, 1f));
                p.AliveDuration = (int)(powerUpAliveDuration * 1000);
                PowerUp.AlivePowerUps.Add(p);
                timeSinceLastPowerUpSpawn = 0;
            }

            //expire
            for (int i = 0; i < PowerUp.AlivePowerUps.Count; )
            {
                PowerUp p = PowerUp.AlivePowerUps[i];
                if (p.AliveTimer.Elapsed.TotalMilliseconds >= p.AliveDuration)
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
                    soundEffectManager.Play(getPowerUpSound, .5f);
                    isaac.acquirePowerUp(p);
                    PowerUp.AlivePowerUps.Remove(p);
                }
                else
                    i++;
            }

            // update status of active powerups
            isaac.updatePowerUps();
        }

        void checkForSledgeHammerUse(GameTime gameTime)
        {
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
                isaac.getActivePowerUp(PowerUpType.SledgeHammer).Charges > 0)// &&
                //!isaacHurtAnimation.IsRunning)
            {
                PowerUp p = isaac.getActivePowerUp(PowerUpType.SledgeHammer);
                p.ActiveTimer.Restart();
                soundEffectManager.Play(useHammerSound, .5f);

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
                if (alex is AlexEnchanter && ((AlexEnchanter)alex).HasPeanutShield)
                {
                    if (sledgeHammer.Intersects(alex))
                    {
                        ((AlexEnchanter)alex).BreakPeanutShield();
                        soundEffectManager.Play(peanutShieldBreakSound, .25f);
                    }
                }
            }
        }

        void isaacShoot(GameTime gameTime)
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

                isaac.shootBullet(BulletType.Peanut, direction, GameSettings.PeanutSpeed, .85f);
                //Stats.PeanutShots++;

                isaac.TimeSinceLastShot = 0;
            }
        }

        void rotatePeanuts(GameTime gameTime)
        {
            foreach (Bullet p in Bullet.Peanuts)
                p.Rotation += Util.ScaleWithGameTime(10, gameTime);
        }

        void checkForPeanutHits()
        {
            for (int i = 0; i < Bullet.Peanuts.Count; )
            {
                Bullet p = Bullet.Peanuts[i];
                if (alex.Intersects(p))
                {
                    Bullet.Peanuts.Remove(p);
                    //Stats.PeanutHits++;
                    if (alex is AlexEnchanter && ((AlexEnchanter)alex).HasPeanutShield)
                    {
                        soundEffectManager.Play(peanutShieldDeflectSound, .15f);
                        shrinkingPeanuts.Add(p);
                    }
                    else
                    {
                        if (alex is AlexEnchanter && !((AlexEnchanter)alex).IsWarping)
                        {
                            soundEffectManager.Play(alexEnchanterHitSound, .15f);
                            alex.HP -= GameSettings.PeanutDamage;
                        }
                        else
                            i++;
                    }
                }
                else
                    i++;
            }

            for (int i = 0; i < Bullet.Peanuts.Count; )
            {
                Bullet p = Bullet.Peanuts[i];
                foreach (Fireball fireball in Fireball.Fireballs)
                {
                    if (fireball.Intersects(p))
                    {
                        Bullet.Peanuts.Remove(p);
                        burningPeanuts.Add(p);
                        p.Texture = burntPeanutTexture;
                        goto nextPeanut;
                    }
                }
                i++;
            nextPeanut: ;
            }
        }

        List<Bullet> shrinkingPeanuts = new List<Bullet>();
        int shrinkPeanutDelay = 100, timeSinceLastShrinkPeanut;
        void shrinkPeanuts(GameTime gameTime)
        {
            timeSinceLastShrinkPeanut += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastShrinkPeanut < shrinkPeanutDelay)
                return;

            timeSinceLastShrinkPeanut = 0;

            for (int i = 0; i < shrinkingPeanuts.Count; )
            {
                Bullet p = shrinkingPeanuts[i];
                p.Width -= 1;
                p.Height -= 1;
                if (p.Width == 0 && p.Height == 0)
                    shrinkingPeanuts.Remove(p);
                else
                    i++;
            }
        }

        List<Bullet> burningPeanuts = new List<Bullet>();
        int burnPeanutDelay = 50, timeSinceLastBurnPeanut;
        void burnPeanuts(GameTime gameTime)
        {
            timeSinceLastBurnPeanut += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastBurnPeanut < burnPeanutDelay)
                return;

            timeSinceLastBurnPeanut = 0;

            for (int i = 0; i < burningPeanuts.Count; )
            {
                Bullet p = burningPeanuts[i];
                p.Width -= 1;
                p.Height -= 1;
                if (p.Width == 0 && p.Height == 0)
                    burningPeanuts.Remove(p);
                else
                    i++;
            }
        }

        void moveBurningPeanuts(GameTime gameTime)
        {
            for (int i = 0; i < burningPeanuts.Count; )
            {
                Bullet b = burningPeanuts[i];

                b.movePrecise(gameTime, false);

                if (b.Y < b.minY - b.Height || b.Y > b.maxY + b.Height ||
                    b.X < b.minX - b.Width || b.X > b.maxX + b.Width)
                    burningPeanuts.Remove(b);
                else
                    i++;
            }
        }

        void updateSuperFireballLoopingSound()
        {
            foreach (Fireball fireball in Fireball.Fireballs)
            {
                if (fireball.Width == AlexEnchanter.SUPERFIREBALLSIZE)
                {
                    if (!superFireballRolling.IsPlaying)
                        superFireballRolling.Start();
                    return;
                }
            }
            superFireballRolling.Stop();
        }

        void cleanup()
        {
            lowHealthBeeping.Stop();
            superFireballRolling.Stop();
            Laser2.Lasers.Clear();
            Fireball.Fireballs.Clear();
            PowerUp.AlivePowerUps.Clear();
            Potion.Potions.Clear();
            //SeekerSnake.SeekerSnakes.Clear();
            MouseSeeker.RemoveAll();
            Bullet.Peanuts.Clear();
            isaac = null;
            //alex = null;
        }
    }
}