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
    public partial class Game1 : Microsoft.Xna.Framework.Game
    {
        static Game1 game;
        GameState currentGameState;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        FpsCounter fpsCounter;
        SoundEffectManager soundEffectManager;
        DebugMonitor debugMonitor;

        public Game1()
        {
            game = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Direction.Init();

            fpsCounter = new FpsCounter(game);
            Components.Add(fpsCounter);
            fpsCounter.DrawOrder = 0;
            //fpsCounter.Enabled = true;

            soundEffectManager = new SoundEffectManager(game);
            Components.Add(soundEffectManager);
            soundEffectManager.DrawOrder = 1;
            //soundEffectManager.DrawDebugInfo = true;

            debugMonitor = new DebugMonitor(game);
            Components.Add(debugMonitor);
            debugMonitor.Enabled = true;
            debugMonitor.DrawBox = true;
            debugMonitor.Position = Direction.SouthEast;

            currentGameState = new TitleScreen(TitleScreenEventHandler, true);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ColorTexture.White = ColorTexture.Create(GraphicsDevice, 16, 16, Color.White);
            ColorTexture.Black = ColorTexture.Create(GraphicsDevice, 16, 16, Color.Black);
            ColorTexture.Red = ColorTexture.Create(GraphicsDevice, 16, 16, Color.Red);
            ColorTexture.Green = ColorTexture.Create(GraphicsDevice, 16, 16, Color.Green);
            ColorTexture.Gray = ColorTexture.Create(GraphicsDevice, 16, 16, Color.Gray);

            HealthBar.Texture = Content.Load<Texture2D>("HealthBar2");

            BulletType.NormalTexture = Content.Load<Texture2D>("peanut");
            BulletType.FireTexture = Content.Load<Texture2D>("fire");
            BulletType.CokeCanTexture = Content.Load<Texture2D>("cokecan");
            BulletType.CrushCanTexture = Content.Load<Texture2D>("crushcan");
            BulletType.DrPepperCanTexture = Content.Load<Texture2D>("drpeppercan");
            BulletType.PepsiCanTexture = Content.Load<Texture2D>("pepsican");
            BulletType.SierraMistCanTexture = Content.Load<Texture2D>("sierramistcan");

            PowerUpType.DoubleShotTexture = Content.Load<Texture2D>("doubleshotpowerup");
            PowerUpType.SpeedBoostTexture = Content.Load<Texture2D>("speedboostpowerup");
            PowerUpType.CanShieldTexture = Content.Load<Texture2D>("shieldpowerup");
            PowerUpType.SledgeHammerTexture = Content.Load<Texture2D>("sledgehammerpowerup");

            Laser.Texture = ColorTexture.Create(graphicsDevice, 16, 16, Color.Red);
            Laser2.Texture = ColorTexture.Create(graphicsDevice, 16, 16, Color.Red);
            Laser.InitializeLine(GraphicsDevice);
            Laser2.InitializeLine(GraphicsDevice);

            Fireball.FireballTexture = Content.Load<Texture2D>("fireball");

            AlexEnchanter.WarpTextures[0] = Content.Load<Texture2D>("alexenchanterwarp1jpg");
            AlexEnchanter.WarpTextures[1] = Content.Load<Texture2D>("alexenchanterwarp2jpg");
            AlexEnchanter.WarpTextures[2] = Content.Load<Texture2D>("alexenchanterwarp3jpg");
            AlexEnchanter.WarpTextures[3] = Content.Load<Texture2D>("alexenchanterwarp4jpg");
            AlexEnchanter.WarpTextures[4] = Content.Load<Texture2D>("alexenchanterwarp5jpg");
            AlexEnchanter.WarpTextures[5] = Content.Load<Texture2D>("alexenchanterwarp6jpg");
            AlexEnchanter.WarpTextures[6] = Content.Load<Texture2D>("alexenchanterwarp7jpg");
            AlexEnchanter.WarpTextures[7] = Content.Load<Texture2D>("alexenchanterwarp8jpg");
            AlexEnchanter.WarpTextures[8] = Content.Load<Texture2D>("alexenchanterwarp9jpg");
            AlexEnchanter.WarpTextures[9] = Content.Load<Texture2D>("alexenchanterwarp10jpg");

            AlexEnchanter.CastTextures[0] = Content.Load<Texture2D>("alexenchantercast1");
            AlexEnchanter.CastTextures[1] = Content.Load<Texture2D>("alexenchantercast2");
            AlexEnchanter.CastTextures[2] = Content.Load<Texture2D>("alexenchantercast3");
            AlexEnchanter.CastTextures[3] = Content.Load<Texture2D>("alexenchantercast4");
            AlexEnchanter.CastTextures[4] = Content.Load<Texture2D>("alexenchantercast5");
            AlexEnchanter.CastTextures[5] = Content.Load<Texture2D>("alexenchantercast6");
            AlexEnchanter.CastTextures[6] = Content.Load<Texture2D>("alexenchantercast7");
            AlexEnchanter.CastTextures[7] = Content.Load<Texture2D>("alexenchantercast8");
            AlexEnchanter.CastTextures[8] = Content.Load<Texture2D>("alexenchantercast9");
            AlexEnchanter.CastTextures[9] = Content.Load<Texture2D>("alexenchantercast10");

            AlexEnchanter.PeanutShieldTextures[0] = Content.Load<Texture2D>("peanutshield1");
            AlexEnchanter.PeanutShieldTextures[1] = Content.Load<Texture2D>("peanutshield2");
            AlexEnchanter.PeanutShieldTextures[2] = Content.Load<Texture2D>("peanutshield3");
            AlexEnchanter.PeanutShieldTextures[3] = Content.Load<Texture2D>("peanutshield4");

            AlexEnchanter.MagicStaffTexture = Content.Load<Texture2D>("Magic_staff");

            AlexEnchanter.SuperFireballEnterSound = Content.Load<SoundEffect>("superfireballenter");

            MouseSeeker.MouseTexture = Content.Load<Texture2D>("LOM");
            MouseSeeker.CordTexture = ColorTexture.Create(graphicsDevice, 16, 16, Color.DarkGray);
            MouseSeeker.PlugTexture = Content.Load<Texture2D>("usb_plug");

            Potion.RedPotionTexture = Content.Load<Texture2D>("redpotion");
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            currentGameState.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.Clear(Color.Gray);

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            spriteBatch.Begin();
            currentGameState.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static Game1 Game
        {
            get
            {
                return game;
            }
        }
        public GraphicsDeviceManager Graphics
        {
            get
            {
                return this.graphics;
            }
        }
        public GraphicsDevice graphicsDevice
        {
            get
            {
                return this.GraphicsDevice;
            }
        }
        public ContentManager content
        {
            get
            {
                return this.Content;
            }
        }
        public SoundEffectManager SoundEffectManager
        {
            get
            {
                return soundEffectManager;
            }
        }
        public DebugMonitor DebugMonitor
        {
            get
            {
                return debugMonitor;
            }
        }

        void TitleScreenEventHandler(Object sender, EventArgs e)
        {
            GameStateArgs args = (GameStateArgs)e;
            if (args.Args.Length > 0)
            {
                if (args.Args[0] == "enterbill")
                {
                    currentGameState = new EnterBill(EnterBillEventHandler);
                    //currentGameState = new EndBattle1(EndBattle1EventHandler);
                    //currentGameState = new EnterCarChase1(EnterCarChase1EventHandler);
                    //currentGameState = new PostGame(PostGameEventHandler);
                    //currentGameState = new Battle2(Battle2EventHandler);
                }
                else if (args.Args[0] == "physics")
                {
                    currentGameState = new PhysicsScreen(PhysicsScreenEventHandler);
                }
                else if (args.Args[0] == "entercarchase")
                {
                    currentGameState = new EnterCarChase1(EnterCarChase1EventHandler);
                }
                else if (args.Args[0] == "test")
                {
                    currentGameState = new Battle2(Battle2EventHandler);
                }
                else if (args.Args[0] == "rts")
                {
                    currentGameState = new Rts(RtsEventHandler);
                }
                else if (args.Args[0] == "shooter")
                {
                    currentGameState = new ShooterLevel(ShooterLevelEventHandler);
                }
            }
        }
        void PhysicsScreenEventHandler(Object sender, EventArgs e)
        {
            GameStateArgs args = (GameStateArgs)e;
            if (args.Args.Length > 0)
            {
                if (args.Args[0] == "title")
                {
                    currentGameState = new TitleScreen(TitleScreenEventHandler, false);
                }
            }
        }
        void EnterBillEventHandler(Object sender, EventArgs e)
        {
            GameStateArgs args = (GameStateArgs)e;
            if (args.Args.Length > 0 && args.Args[0] == "title")
                currentGameState = new TitleScreen(TitleScreenEventHandler, true);
            else
                currentGameState = new Battle1(Battle1EventHandler, true);
        }
        void Battle1EventHandler(Object sender, EventArgs e)
        {
            GameStateArgs args = (GameStateArgs)e;
            if (args.Args.Length > 0)
            {
                if (args.Args[0] == "title")
                    currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                else if (args.Args[0] == "endbattle1")
                    currentGameState = new EndBattle1(EndBattle1EventHandler);
            }
        }
        void EndBattle1EventHandler(Object sender, EventArgs e)
        {
            GameStateArgs args = (GameStateArgs)e;
            if (args.Args.Length > 0)
            {
                if (args.Args[0] == "title")
                {
                    currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                }
                else if (args.Args[0] == "entercarchase")
                {
                    currentGameState = new EnterCarChase1(EnterCarChase1EventHandler);
                }
            }
        }
        void EnterCarChase1EventHandler(Object sender, EventArgs e)
        {
            GameStateArgs args = (GameStateArgs)e;
            if (args.Args.Length > 0)
            {
                if (args.Args[0] == "title")
                {
                    currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                }
                else if (args.Args[0] == "carchase")
                {
                    currentGameState = new CarChase1(CarChase1EventHandler);
                }
            }
        }
        void CarChase1EventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "title")
                    {
                        currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                    }
                }
            }
            else if (e is CharacterArgs)
            {
                CharacterArgs chars = (CharacterArgs)e;
                if (chars.Chars.Length == 2)
                {
                    currentGameState = new EndCarChase1(EndCarChase1EventHandler, chars.Chars[0], chars.Chars[1]);
                }
            }
        }
        void EndCarChase1EventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "title")
                    {
                        currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                    }
                    else if (args.Args[0] == "postgame")
                    {
                        currentGameState = new PostGame(PostGameEventHandler);
                    }
                }
            }
        }
        void Battle2EventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "title")
                    {
                        currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                    }
                    else if (args.Args[0] == "postgame")
                    {
                        currentGameState = new PostGame(PostGameEventHandler);
                    }
                }
            }
        }
        void PostGameEventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "title")
                    {
                        currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                    }
                }
            }
        }
        void RtsEventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "title")
                    {
                        currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                    }
                }
            }
        }
        void ShooterLevelEventHandler(Object sender, EventArgs e)
        {
            if (e is GameStateArgs)
            {
                GameStateArgs args = (GameStateArgs)e;
                if (args.Args.Length > 0)
                {
                    if (args.Args[0] == "title")
                    {
                        currentGameState = new TitleScreen(TitleScreenEventHandler, true);
                    }
                }
            }
        }
    }
}
