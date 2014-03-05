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
    class PostGame : GameState
    {
        static bool contentLoaded = false;
        Random rand = new Random();
        DateTime startTime;

        BaseObject returnButton, exitButton;
        Texture2D buttonTexture1, buttonTexture2;
        const int RETURNBUTTONWIDTH = 80, RETURNBUTTONHEIGHT = 40;
        const int EXITBUTTONWIDTH = 80, EXITBUTTONHEIGHT = 40;

        Song postGameMusic;
        SpriteFont bigFont, mediumFont, smallFont, buttonFont;

        Texture2D mikeTexture, mrPeanutTexture;

        Rectangle pongo;
        static readonly int PONGOHEIGHT = (int)(Graphics.GraphicsDevice.Viewport.Height * .6f);
        static readonly int PONGOWIDTH = (int)(PONGOHEIGHT * .99f);

        Rectangle mrPeanut;
        static readonly int MRPEANUTHEIGHT = (int)(Graphics.GraphicsDevice.Viewport.Height * .6f);
        static readonly int MRPEANUTWIDTH = MRPEANUTHEIGHT / 2;

        BaseObject mike;
        static readonly int MIKEHEIGHT = 120, MIKEWIDTH = 86;

        Vector2 congratsSize;
        int congratsX, congratsY;

        int accuracyPercent;
        string accuracyMsg, peanutScoreMsg;

        public PostGame(EventHandler callback)
            : base(callback)
        {
            Game1.Game.IsMouseVisible = true;
            Game1.Game.Window.Title = "Congratulations!";
            startTime = DateTime.Now;

            if (!contentLoaded)
            {
                postGameMusic = Content.Load<Song>("postgamemusic");
                mikeTexture = Content.Load<Texture2D>("mikeglasses");
                mrPeanutTexture = Content.Load<Texture2D>("mrpeanut");
                buttonTexture1 = Content.Load<Texture2D>("titlebutton1");
                buttonTexture2 = Content.Load<Texture2D>("titlebutton2");
                bigFont = Content.Load<SpriteFont>("PostGameFont1");
                mediumFont = Content.Load<SpriteFont>("PostGameFont2");
                smallFont = Content.Load<SpriteFont>("PostGameFont3");
                buttonFont = Content.Load<SpriteFont>("TitleFont2");
            }

            pongo = new Rectangle(Graphics.GraphicsDevice.Viewport.Width / 2 - PONGOWIDTH / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - PONGOHEIGHT / 2, PONGOWIDTH, PONGOHEIGHT);
            mrPeanut = new Rectangle(Graphics.GraphicsDevice.Viewport.Width / 2 - MRPEANUTWIDTH / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - MRPEANUTHEIGHT / 2, MRPEANUTWIDTH, MRPEANUTHEIGHT);

            returnButton = new BaseObject(new Rectangle(pongo.X, Graphics.GraphicsDevice.Viewport.Height - RETURNBUTTONHEIGHT - 1, RETURNBUTTONWIDTH, RETURNBUTTONHEIGHT), new Vector2(0, 0));
            returnButton.CenterPoint = new Vector2(pongo.X + pongo.Width * .25f, Graphics.GraphicsDevice.Viewport.Height - returnButton.Height / 2 - 1);
            exitButton = new BaseObject(new Rectangle(pongo.X + pongo.Width - EXITBUTTONWIDTH, Graphics.GraphicsDevice.Viewport.Height - EXITBUTTONHEIGHT - 1, EXITBUTTONWIDTH, EXITBUTTONHEIGHT), new Vector2(0, 0));
            exitButton.CenterPoint = new Vector2(pongo.X + pongo.Width * .75f, Graphics.GraphicsDevice.Viewport.Height - exitButton.Height / 2 - 1);

            //int pongoRightX = pongo.X + pongo.Width;
            int pongoRightX = mrPeanut.X + mrPeanut.Width;
            mike = new BaseObject(new Rectangle(pongoRightX + (Graphics.GraphicsDevice.Viewport.Width - pongoRightX) / 2 - MIKEWIDTH / 2, Graphics.GraphicsDevice.Viewport.Height / 2 - MIKEHEIGHT / 2, MIKEWIDTH, MIKEHEIGHT));
            mike.Texture = mikeTexture;

            congratsSize = bigFont.MeasureString("Congratulations!");
            congratsX = (int)(Graphics.GraphicsDevice.Viewport.Width / 2 - congratsSize.X / 2);
            congratsY = (int)(pongo.Y / 2 - congratsSize.Y / 2);

            Bullet b = new Bullet(BulletType.Peanut, new Vector2(0, 0));
            b.Width = b.Height = (int)(b.Width * 1.5f);
            b.CenterPoint = new Vector2(congratsX - b.Width, congratsY + congratsSize.Y / 2);
            b = new Bullet(BulletType.Peanut, new Vector2(0, 0));
            b.Width = b.Height = (int)(b.Width * 1.5f);
            b.CenterPoint = new Vector2(congratsX + congratsSize.X + b.Width, congratsY + congratsSize.Y / 2);

            accuracyPercent = (int)Math.Round((Stats.PeanutHits / (float)Stats.PeanutShots) * 100);
            if (Stats.PeanutShots == 0)
                accuracyPercent = 0;
            if (accuracyPercent >= 50)
                accuracyMsg = "Your skills are second to none.";
            else if (accuracyPercent >= 25)
                accuracyMsg = "Don't stop believing";
            else
                accuracyMsg = "Go hit a tree with a stick.";

            if (Stats.CarChasePeanutScore >= 100)
                peanutScoreMsg = "You are the chosen one";
            else if (Stats.CarChasePeanutScore >= 90)
                peanutScoreMsg = "IMPECCABLE";
            else if (Stats.CarChasePeanutScore >= 80)
                peanutScoreMsg = "Over 10-12 years of experience";
            else if (Stats.CarChasePeanutScore >= 70)
                peanutScoreMsg = "Looks can always be deceiving";
            else if (Stats.CarChasePeanutScore >= 60)
                peanutScoreMsg = "You've always been a fast learner";
            else if (Stats.CarChasePeanutScore >= 50)
                peanutScoreMsg = "Competing with 3-4 people from your school";
            else
                peanutScoreMsg = "r u dumb?";

            MediaPlayer.Play(postGameMusic);
            MediaPlayer.Volume = .5f;
            MediaPlayer.IsRepeating = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                (exitButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed))
                Game1.Game.Exit();

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                cleanup();
                returnControl("title");
                return;
            }

            if (returnButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y) && Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                cleanup();
                returnControl("title");
                return;
            }

            // mute check
            checkForMute();

            Bullet.moveBullets(gameTime);
            rotateCans(gameTime);
            rotatePeanuts(gameTime);

            createFallingCans(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            bool returnButtonMousedOver = returnButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool exitButtonMousedOver = exitButton.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);
            bool mikeMousedOver = mike.Rectangle.Contains(Mouse.GetState().X, Mouse.GetState().Y);

            GraphicsDevice.Clear(Color.Gray);

            // falling cans
            foreach (Bullet b in Bullet.Cans)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            //if (!mikeMousedOver)
                //spriteBatch.Draw(pongoTexture, pongo, Color.White);
            //else
                spriteBatch.Draw(mrPeanutTexture, mrPeanut, Color.White);

            /*if ((DateTime.Now - startTime).TotalMilliseconds <= 500)
            {
                foreach (Bullet b in Bullet.Cans)
                    spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);
            }*/

            spriteBatch.DrawString(bigFont, "Congratulations!", new Vector2(congratsX, congratsY), Color.Black);
            string msg = "Bill has chilled out and peace has been restored.";
            spriteBatch.DrawString(mediumFont, msg, new Vector2((int)(pongo.X + pongo.Width / 2 - mediumFont.MeasureString(msg).X / 2), (int)(pongo.Y + pongo.Height + (returnButton.Y - (pongo.Y + pongo.Height)) / 2 - mediumFont.MeasureString(msg).Y / 2)), Color.Black);

            // peanuts
            foreach (Bullet b in Bullet.Peanuts)
                spriteBatch.Draw(b.Texture, new Rectangle((int)b.CenterPoint.X, (int)b.CenterPoint.Y, b.Width, b.Height), null, Color.White, b.Rotation, b.TextureCenterOrigin, SpriteEffects.None, 0f);

            // stats
            string accuracyString = "Accuracy: " + accuracyPercent + "%";
            Vector2 accuracyStringSize = mediumFont.MeasureString(accuracyString);
            Vector2 accuracyMsgSize = smallFont.MeasureString(accuracyMsg);
            int accuracyPosY = (int)(pongo.Y + pongo.Height * .25f);
            spriteBatch.DrawString(mediumFont, accuracyString, new Vector2((int)(mrPeanut.X / 2 - accuracyStringSize.X / 2), accuracyPosY), Color.Black);
            spriteBatch.DrawString(smallFont, accuracyMsg, new Vector2((int)(mrPeanut.X / 2 - accuracyMsgSize.X / 2), (int)(accuracyPosY + accuracyMsgSize.Y)), Color.Black);

            string peanutScoreString = "Peanuts Collected: " + Stats.CarChasePeanutScore;
            Vector2 peanutScoreStringSize = mediumFont.MeasureString(peanutScoreString);
            Vector2 peanutScoreMsgSize = smallFont.MeasureString(peanutScoreMsg);
            int peanutScorePosY = (int)(pongo.Y + pongo.Height * .6f);
            spriteBatch.DrawString(mediumFont, peanutScoreString, new Vector2((int)(mrPeanut.X / 2 - peanutScoreStringSize.X / 2), peanutScorePosY), Color.Black);
            spriteBatch.DrawString(smallFont, peanutScoreMsg, new Vector2((int)(mrPeanut.X / 2 - peanutScoreMsgSize.X / 2), (int)(peanutScorePosY + peanutScoreMsgSize.Y)), Color.Black);

            // "cody"
            //spriteBatch.Draw(mike.Texture, new Rectangle((int)mike.CenterPoint.X, (int)mike.CenterPoint.Y, mike.Width, mike.Height), null, Color.White, mike.Rotation, new Vector2(mike.Texture.Width * .7f, mike.Texture.Height), SpriteEffects.None, 0f);
            spriteBatch.Draw(mike.Texture, mike, Color.White);
            spriteBatch.DrawString(smallFont, "Head Programmer", new Vector2((int)(mike.X + mike.Width / 2 - smallFont.MeasureString("Head Programmer").X / 2), (int)(mike.Y - smallFont.MeasureString("Head Programmer").Y)), Color.Black);
            spriteBatch.DrawString(mediumFont, "Cody", new Vector2((int)(mike.X + mike.Width / 2 - mediumFont.MeasureString("Cody").X / 2), (int)(mike.Y + mike.Height)), Color.Black);

            // buttons
            string returnButtonMsg = "RETURN", exitButtonMsg = "EXIT";
            Color color;

            if (!returnButtonMousedOver)
            {
                spriteBatch.Draw(buttonTexture1, returnButton, Color.White);
                color = Color.Gray;
            }
            else
            {
                spriteBatch.Draw(buttonTexture2, returnButton, Color.White);
                color = Color.Red;
            }
            spriteBatch.DrawString(buttonFont, returnButtonMsg, new Vector2((int)(returnButton.X + returnButton.Width / 2 - buttonFont.MeasureString(returnButtonMsg).X / 2), (int)(returnButton.Y + returnButton.Height / 2 - buttonFont.MeasureString(returnButtonMsg).Y / 2)), color);

            if (!exitButtonMousedOver)
            {
                spriteBatch.Draw(buttonTexture1, exitButton, Color.White);
                color = Color.Gray;
            }
            else
            {
                spriteBatch.Draw(buttonTexture2, exitButton, Color.White);
                color = Color.Red;
            }
            spriteBatch.DrawString(buttonFont, exitButtonMsg, new Vector2((int)(exitButton.X + exitButton.Width / 2 - buttonFont.MeasureString(exitButtonMsg).X / 2), (int)(exitButton.Y + exitButton.Height / 2 - buttonFont.MeasureString(exitButtonMsg).Y / 2)), color);
        }

        void cleanup()
        {
            Bullet.Peanuts.Clear();
            Bullet.Cans.Clear();
        }

        int timeSinceLastCanSpawn;
        const int CANSPAWNDELAY = 250, FALLINGCANSPEED = 80;
        void createFallingCans(GameTime gameTime)
        {
            timeSinceLastCanSpawn += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastCanSpawn >= CANSPAWNDELAY)
            {
                Bullet b = new Bullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], new Vector2(0, FALLINGCANSPEED));
                b.X = rand.Next(Graphics.GraphicsDevice.Viewport.Width - b.Width);
                b.Y = -b.Height;
                timeSinceLastCanSpawn = 0;
            }
        }

        void rotateCans(GameTime gameTime)
        {
            foreach (Bullet b in Bullet.Cans)
                b.Rotation += Util.ScaleWithGameTime(5, gameTime);
        }
        void rotatePeanuts(GameTime gameTime)
        {
            foreach (Bullet b in Bullet.Peanuts)
                b.Rotation += Util.ScaleWithGameTime(9, gameTime);
        }

        void canExplosion()
        {
            Vector2 target;
            Character center = new Character(new Rectangle(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height / 2, 1, 1), new Vector2());
            for (int i = 0; i < 500; i++)
            {
                target = new Vector2(rand.Next(Graphics.GraphicsDevice.Viewport.Width), rand.Next(Graphics.GraphicsDevice.Viewport.Height));
                center.shootBullet(BulletType.Cans[rand.Next(BulletType.Cans.Length)], target, 1000);
            }
        }
    }
}