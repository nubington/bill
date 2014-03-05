using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace bill
{
    class PhysicsScreen : GameState
    {
        static bool contentLoaded = false;
        DateTime startTime;
        static double clickDelay = 50, multiplierDelay = 250;
        int shiftX = 30;
        static double timeSinceLastClick = clickDelay, timeSinceMultiplierClicked = multiplierDelay;

        static Song titleTheme;
        static SpriteFont physicsFont1, titleFont2;
        static Texture2D button1Texture, button2Texture,
            upButtonTexture, downButtonTexture, 
            multiplierButtonTexture,
            toggleOnTexture,
            redRingTexture,
            einsteinTexture;

        bool multiplierOn = false;
        int multiplier = 1;

        BaseObject returnButton;
        const int RETURNBUTTONWIDTH = 80, RETURNBUTTONHEIGHT = 40;
        const int PRESETBUTTONWIDTH = 80, PRESETBUTTONHEIGHT = 40;
        const int EINSTEINWIDTH = 150, EINSTEINHEIGHT = 198;

        int startPosY = Graphics.GraphicsDevice.Viewport.Height / 7;
        int spacing;

        Rectangle
            multiplierButton, einstein,

            isaacSpeedButtonUp, isaacSpeedButtonDown,
            speedBoostButtonUp, speedBoostButtonDown,
            isaacShootSpeedButtonUp, isaacShootSpeedButtonDown,
            peanutDamageButtonUp, peanutDamageButtonDown,
            peanutSpeedButtonUp, peanutSpeedButtonDown,
            billHPButtonUp, billHPButtonDown,
            billLifeStealButtonUp, billLifeStealButtonDown,
            billSpeedButtonUp, billSpeedButtonDown,
            billShootSpeedButtonUp, billShootSpeedButtonDown,
            billSprayFireSpeedButtonUp, billSprayFireSpeedButtonDown,
            canSpeedButtonUp, canSpeedButtonDown,
            powerUpSpawnDelayButtonUp, powerUpSpawnDelayButtonDown,
            powerUpAliveButtonUp, powerUpAliveButtonDown,
            powerUpActiveButtonUp, powerUpActiveButtonDown,
            canShieldChargesButtonUp, canShieldChargesButtonDown,
            spawnSafetyButtonUp, spawnSafetyButtonDown,

            presetEasy, presetNormal, presetHard,
            
            cansBlockPeanutsToggleButton, projectileRotationCollisionToggleButton;


        public PhysicsScreen(EventHandler callback)
            : base(callback)
        {
            // initialize
            //Game1.Game.Window.Title = "The Physics";
            startTime = DateTime.Now;
            Game1.Game.IsMouseVisible = true;

            returnButton = new BaseObject(new Rectangle(0, 0, RETURNBUTTONWIDTH, RETURNBUTTONHEIGHT), new Vector2(0, 0));
            returnButton.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height - returnButton.Height/2 - 1);
            

            // load content
            if (!contentLoaded)
            {
                physicsFont1 = Content.Load<SpriteFont>("PhysicsFont1");
                titleFont2 = Content.Load<SpriteFont>("TitleFont2");
                titleTheme = Content.Load<Song>("titletheme");
                button1Texture = Content.Load<Texture2D>("titlebutton1");
                button2Texture = Content.Load<Texture2D>("titlebutton2");
                upButtonTexture = Content.Load<Texture2D>("physicsupbutton");
                downButtonTexture = Content.Load<Texture2D>("physicsdownbutton");
                multiplierButtonTexture = Content.Load<Texture2D>("physicsmultiplier");
                toggleOnTexture = Content.Load<Texture2D>("togglebuttonon");
                redRingTexture = Content.Load<Texture2D>("redring");
                einsteinTexture = Content.Load<Texture2D>("einstein");
            }

            int x = Graphics.GraphicsDevice.Viewport.Width / 4;
            einstein = new Rectangle(x / 2 - EINSTEINWIDTH / 2 - 2 + shiftX / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - EINSTEINHEIGHT / 2, EINSTEINWIDTH, EINSTEINHEIGHT);

            // create button rectangles
            spacing = (int)physicsFont1.MeasureString(" ").Y;
            int offsetY = startPosY;
            x = Graphics.GraphicsDevice.Viewport.Width / 2 + 100;
            int size = (int)(spacing * .9f);

            multiplierButton = new Rectangle(x + spacing / 3, offsetY - (int)(size * 1.5f), (int)(size * 1.4f), (int)(size * 1.35f));

            isaacSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            isaacSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            speedBoostButtonDown = new Rectangle(x, offsetY, size, size);
            speedBoostButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            isaacShootSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            isaacShootSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            peanutDamageButtonDown = new Rectangle(x, offsetY, size, size);
            peanutDamageButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            peanutSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            peanutSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            billHPButtonDown = new Rectangle(x, offsetY, size, size);
            billHPButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            billLifeStealButtonDown = new Rectangle(x, offsetY, size, size);
            billLifeStealButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            billSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            billSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            billShootSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            billShootSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            billSprayFireSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            billSprayFireSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            canSpeedButtonDown = new Rectangle(x, offsetY, size, size);
            canSpeedButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            powerUpSpawnDelayButtonDown = new Rectangle(x, offsetY, size, size);
            powerUpSpawnDelayButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            powerUpAliveButtonDown = new Rectangle(x, offsetY, size, size);
            powerUpAliveButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            powerUpActiveButtonDown = new Rectangle(x, offsetY, size, size);
            powerUpActiveButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            canShieldChargesButtonDown = new Rectangle(x, offsetY, size, size);
            canShieldChargesButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            spawnSafetyButtonDown = new Rectangle(x, offsetY, size, size);
            spawnSafetyButtonUp = new Rectangle(x + spacing, offsetY, size, size);
            offsetY += spacing;

            // create preset buttons
            x += spacing * 2;

            presetEasy = new Rectangle(x + (Graphics.GraphicsDevice.Viewport.Width - x) / 2 - PRESETBUTTONWIDTH / 2, startPosY, PRESETBUTTONWIDTH, PRESETBUTTONHEIGHT);

            presetNormal = new Rectangle(x + (Graphics.GraphicsDevice.Viewport.Width - x) / 2 - PRESETBUTTONWIDTH / 2, presetEasy.Y + PRESETBUTTONHEIGHT + (int)(spacing * .1), PRESETBUTTONWIDTH, PRESETBUTTONHEIGHT);

            presetHard = new Rectangle(x + (Graphics.GraphicsDevice.Viewport.Width - x) / 2 - PRESETBUTTONWIDTH / 2, presetNormal.Y + PRESETBUTTONHEIGHT + (int)(spacing * .1), PRESETBUTTONWIDTH, PRESETBUTTONHEIGHT);

            // create toggle buttons
            offsetY = Graphics.GraphicsDevice.Viewport.Height / 2 + size * 3;

            cansBlockPeanutsToggleButton = new Rectangle(x + (Graphics.GraphicsDevice.Viewport.Width - x) / 2 + ((int)physicsFont1.MeasureString("Cans block peanuts: ").X) / 2 - size / 2, offsetY, size, size);
            offsetY += spacing;

            projectileRotationCollisionToggleButton = new Rectangle(x + (Graphics.GraphicsDevice.Viewport.Width - x) / 2 + ((int)physicsFont1.MeasureString("Cans block peanuts: ").X) / 2 - size / 2, offsetY, size, size);
            offsetY += spacing;
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape) ||
                (returnButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed))
            {
                returnControl("title");
                return;
            }

            // mute check
            checkForMute();

            if (MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(titleTheme);

            timeSinceLastClick += gameTime.ElapsedGameTime.TotalMilliseconds;
            timeSinceMultiplierClicked += gameTime.ElapsedGameTime.TotalMilliseconds;

            // multiplier and toggle buttons
            if (timeSinceMultiplierClicked >= multiplierDelay)
            {
                if (multiplierButton.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    timeSinceMultiplierClicked = 0;
                    if (multiplierOn)
                    {
                        multiplierOn = false;
                        multiplier = 1;
                    }
                    else
                    {
                        multiplierOn = true;
                        multiplier = 10;
                    }
                }
                if (cansBlockPeanutsToggleButton.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    timeSinceMultiplierClicked = 0;
                    GameSettings.CansBlockPeanuts ^= true;
                }
                else if (projectileRotationCollisionToggleButton.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    timeSinceMultiplierClicked = 0;
                    GameSettings.ProjectileRotationCollision ^= true;
                }
            }

            //up and down buttons
            if (timeSinceLastClick >= clickDelay)
            {
                if (isaacSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.IsaacSpeed -= GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (isaacSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.IsaacSpeed += GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (speedBoostButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.SpeedBoostModifier -= .1m * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (speedBoostButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.SpeedBoostModifier += .1m * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (isaacShootSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.IsaacShootSpeed -= .1m * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (isaacShootSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.IsaacShootSpeed += .1m * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (peanutDamageButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PeanutDamage -= 1 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (peanutDamageButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PeanutDamage += 1 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (peanutSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PeanutSpeed -= GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (peanutSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PeanutSpeed += GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (billHPButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillHP -= 1 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (billHPButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillHP += 1 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (billLifeStealButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillLifeStealAmount -= 1 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (billLifeStealButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillLifeStealAmount += 1 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (billSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillSpeed -= GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (billSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillSpeed += GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (billShootSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillShootSpeed -= .1m * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (billShootSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillShootSpeed += .1m * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (billSprayFireSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillSprayFireSpeed -= .1m * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (billSprayFireSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.BillSprayFireSpeed += .1m * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (canSpeedButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.CanSpeed -= GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (canSpeedButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.CanSpeed += GameSettings.SPEEDINCREMENT * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (powerUpSpawnDelayButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PowerUpSpawnDelay -= 100 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (powerUpSpawnDelayButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PowerUpSpawnDelay += 100 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (powerUpAliveButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PowerUpAliveDuration -= 100 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (powerUpAliveButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PowerUpAliveDuration += 100 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (powerUpActiveButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PowerUpActiveDuration -= 100 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (powerUpActiveButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.PowerUpActiveDuration += 100 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (canShieldChargesButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.CanShieldCharges -= 1 * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (canShieldChargesButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.CanShieldCharges += 1 * multiplier;
                    timeSinceLastClick = 0;
                }

                else if (spawnSafetyButtonDown.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.SpawnSafetyFactor -= .1m * multiplier;
                    timeSinceLastClick = 0;
                }
                else if (spawnSafetyButtonUp.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    GameSettings.SpawnSafetyFactor += .1m * multiplier;
                    timeSinceLastClick = 0;
                }
            }

            // preset buttons
            if (presetEasy.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                presetEasyClick();
            else if (presetNormal.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                presetNormalClick();
            else if (presetHard.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                presetHardClick();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            bool returnButtonMousedOver = returnButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool presetNormalMousedOver = presetNormal.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool presetEasyMousedOver = presetEasy.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool presetHardMousedOver = presetHard.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            string returnButtonMsg = "RETURN", presetNormalMsg = "normal", presetEasyMsg = "easy", presetHardMsg = "hard";

            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Draw(einsteinTexture, einstein, Color.White);

            int offsetY = startPosY;
            Color color = Color.Black;

            string msgString = "Isaac move speed: ";
            string valueString = GameSettings.IsaacSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Speed boost mod: ";
            valueString = GameSettings.SpeedBoostModifier.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Isaac shoot speed: ";
            valueString = GameSettings.IsaacShootSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Peanut IQ (damage): ";
            valueString = GameSettings.PeanutDamage.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Peanut speed: ";
            valueString = GameSettings.PeanutSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Bill HP: ";
            valueString = GameSettings.BillHP.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Bill lifesteal: ";
            valueString = GameSettings.BillLifeStealAmount.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Bill move speed: ";
            valueString = GameSettings.BillSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Bill shoot speed: ";
            valueString = GameSettings.BillShootSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Bill spray-fire speed: ";
            valueString = GameSettings.BillSprayFireSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Can speed: ";
            valueString = GameSettings.CanSpeed.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Power-Up spawn delay: ";
            valueString = (GameSettings.PowerUpSpawnDelay / 1000m).ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Power-Up alive duration: ";
            valueString = (GameSettings.PowerUpAliveDuration / 1000m).ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Power-Up active duration: ";
            valueString = (GameSettings.PowerUpActiveDuration / 1000m).ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Can-shield charges: ";
            valueString = GameSettings.CanShieldCharges.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            msgString = "Spawn safety factor: ";
            valueString = GameSettings.SpawnSafetyFactor.ToString();
            spriteBatch.DrawString(physicsFont1, msgString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 4 + shiftX), offsetY), color);
            spriteBatch.DrawString(physicsFont1, valueString, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 + shiftX), offsetY), color);
            offsetY += spacing;

            // up and down buttons
            spriteBatch.Draw(downButtonTexture, isaacSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, isaacSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, speedBoostButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, speedBoostButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, isaacShootSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, isaacShootSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, peanutDamageButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, peanutDamageButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, peanutSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, peanutSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, billHPButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, billHPButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, billLifeStealButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, billLifeStealButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, billSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, billSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, billShootSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, billShootSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, billSprayFireSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, billSprayFireSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, canSpeedButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, canSpeedButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, powerUpSpawnDelayButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, powerUpSpawnDelayButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, powerUpAliveButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, powerUpAliveButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, powerUpActiveButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, powerUpActiveButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, canShieldChargesButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, canShieldChargesButtonUp, Color.White);

            spriteBatch.Draw(downButtonTexture, spawnSafetyButtonDown, Color.White);
            spriteBatch.Draw(upButtonTexture, spawnSafetyButtonUp, Color.White);

            // multiplier button
            spriteBatch.Draw(multiplierButtonTexture, multiplierButton, Color.White);
            if (multiplierOn)
                spriteBatch.Draw(redRingTexture, multiplierButton, Color.White);

            // presets
            if (presetNormalMousedOver)
            {
                spriteBatch.Draw(button2Texture, presetNormal, Color.White);
                color = Color.Yellow;
            }
            else
            {
                spriteBatch.Draw(button1Texture, presetNormal, Color.White);
                color = Color.Gray;
            }
            spriteBatch.DrawString(titleFont2, presetNormalMsg, new Vector2((int)(presetNormal.X + presetNormal.Width / 2 - titleFont2.MeasureString(presetNormalMsg).X / 2), (int)(presetNormal.Y + presetNormal.Height / 2 - titleFont2.MeasureString(presetNormalMsg).Y / 2)), color);

            if (presetEasyMousedOver)
            {
                spriteBatch.Draw(button2Texture, presetEasy, Color.White);
                color = Color.Green;
            }
            else
            {
                spriteBatch.Draw(button1Texture, presetEasy, Color.White);
                color = Color.Gray;
            }
            spriteBatch.DrawString(titleFont2, presetEasyMsg, new Vector2((int)(presetEasy.X + presetEasy.Width / 2 - titleFont2.MeasureString(presetEasyMsg).X / 2), (int)(presetEasy.Y + presetEasy.Height / 2 - titleFont2.MeasureString(presetEasyMsg).Y / 2)), color);

            if (presetHardMousedOver)
            {
                spriteBatch.Draw(button2Texture, presetHard, Color.White);
                color = Color.Red;
            }
            else
            {
                spriteBatch.Draw(button1Texture, presetHard, Color.White);
                color = Color.Gray;
            }
            spriteBatch.DrawString(titleFont2, presetHardMsg, new Vector2((int)(presetHard.X + presetHard.Width / 2 - titleFont2.MeasureString(presetHardMsg).X / 2), (int)(presetHard.Y + presetHard.Height / 2 - titleFont2.MeasureString(presetHardMsg).Y / 2)), color);

            spriteBatch.DrawString(physicsFont1, "PRESETS:", new Vector2((int)(presetEasy.X + presetEasy.Width / 2 - physicsFont1.MeasureString("PRESETS:").X / 2), (int)(presetEasy.Y - physicsFont1.MeasureString("PRESETS:").Y)), Color.Black);

            // toggles
            spriteBatch.DrawString(physicsFont1, "TOGGLES:", new Vector2((int)(presetEasy.X + presetEasy.Width / 2 - physicsFont1.MeasureString("TOGGLES:").X / 2), (int)(cansBlockPeanutsToggleButton.Y - physicsFont1.MeasureString("TOGGLES:").Y)), Color.Black);

            spriteBatch.DrawString(physicsFont1, "Cans block peanuts: ", new Vector2(cansBlockPeanutsToggleButton.X - physicsFont1.MeasureString("Cans block peanuts: ").X, cansBlockPeanutsToggleButton.Y), Color.Black);
            if (!GameSettings.CansBlockPeanuts)
                spriteBatch.Draw(button1Texture, cansBlockPeanutsToggleButton, Color.White);
            else
                spriteBatch.Draw(toggleOnTexture, cansBlockPeanutsToggleButton, Color.White);

            spriteBatch.DrawString(physicsFont1, "Rotation collision: ", new Vector2(projectileRotationCollisionToggleButton.X - physicsFont1.MeasureString("Rotation Collision: ").X, projectileRotationCollisionToggleButton.Y), Color.Black);
            if (!GameSettings.ProjectileRotationCollision)
                spriteBatch.Draw(button1Texture, projectileRotationCollisionToggleButton, Color.White);
            else
                spriteBatch.Draw(toggleOnTexture, projectileRotationCollisionToggleButton, Color.White);

            // exit button
            if (!returnButtonMousedOver)
            {
                spriteBatch.Draw(button1Texture, returnButton, Color.White);
                color = Color.Gray;
            }
            else
            {
                spriteBatch.Draw(button2Texture, returnButton, Color.White);
                color = Color.Red;
            }
            spriteBatch.DrawString(titleFont2, returnButtonMsg, new Vector2((int)(returnButton.X + returnButton.Width / 2 - titleFont2.MeasureString(returnButtonMsg).X / 2), (int)(returnButton.Y + returnButton.Height / 2 - titleFont2.MeasureString(returnButtonMsg).Y / 2)), color);
        }

        public static void presetEasyClick()
        {
            GameSettings.CansBlockPeanuts = false;
            GameSettings.SpawnSafetyFactor = GameSettings.SPAWNSAFETYFACTORMAX;
        }
        public static void presetNormalClick()
        {
            GameSettings.IsaacSpeed = GameSettings.DEFAULTISAACSPEED;
            GameSettings.BillSpeed = GameSettings.DEFAULTBILLSPEED;
            GameSettings.PowerUpSpawnDelay = GameSettings.DEFAULTPOWERUPSPAWNDELAY;
            GameSettings.PowerUpAliveDuration = GameSettings.DEFAULTPOWERUPALIVEDURATION;
            GameSettings.PowerUpActiveDuration = GameSettings.DEFAULTPOWERUPACTIVEDURATION;
            GameSettings.PeanutDamage = GameSettings.DEFAULTPEANUTDAMAGE;
            GameSettings.PeanutSpeed = GameSettings.DEFAULTPEANUTSPEED;
            GameSettings.BillHP = GameSettings.DEFAULTBILLHP;
            GameSettings.BillLifeStealAmount = GameSettings.DEFAULTBILLLIFESTEALAMOUNT;
            GameSettings.IsaacShootSpeed = GameSettings.DEFAULTISAACSHOOTSPEED;
            GameSettings.BillShootSpeed = GameSettings.DEFAULTBILLSHOOTSPEED;
            GameSettings.SpeedBoostModifier = GameSettings.DEFAULTSPEEDBOOSTMODIFIER;
            GameSettings.SpawnSafetyFactor = GameSettings.DEFAULTSPAWNSAFETYFACTOR;
            GameSettings.CanSpeed = GameSettings.DEFAULTCANSPEED;
            GameSettings.BillSprayFireSpeed = GameSettings.DEFAULTBILLSPRAYFIRESPEED;
            GameSettings.CanShieldCharges = GameSettings.DEFAULTCANSHIELDCHARGES;
            GameSettings.CansBlockPeanuts = true;
            GameSettings.ProjectileRotationCollision = true;
        }
        public static void presetHardClick()
        {
            GameSettings.IsaacSpeed = 750;
            GameSettings.BillSpeed = GameSettings.BILLSPEEDMIN;
            GameSettings.PeanutSpeed = GameSettings.PEANUTSPEEDMIN;
            GameSettings.IsaacShootSpeed = GameSettings.ISAACSHOOTSPEEDMAX;
            GameSettings.BillShootSpeed = GameSettings.BILLSHOOTSPEEDMAX;
            GameSettings.CanSpeed = GameSettings.CANSPEEDMIN;
            GameSettings.CansBlockPeanuts = true;
            GameSettings.ProjectileRotationCollision = true;
        }
    }
}