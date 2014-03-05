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
    class EndBattle1 : Battle1
    {
        static bool contentLoaded = false;

        static SpriteFont endBattle1Font1, actTitleFont;

        static Song mikeTheme;

        BaseObject keys, wizard, rollerBlades;
        static readonly int WIZARDHEIGHT = 225, WIZARDWIDTH = (int)(WIZARDHEIGHT * 1.05392157f);
        static readonly int ROLLERBLADESHEIGHT = 75, ROLLERBLADESWIDTH = (int)(ROLLERBLADESHEIGHT * 1.33f);

        static Texture2D keysTexture, rollerBladesTexture, wizardTexture;

        public EndBattle1(EventHandler callback)
            : base(callback, false)
        {
            if (!contentLoaded)
            {
                endBattle1Font1 = Content.Load<SpriteFont>("EndBattle1Font1");
                actTitleFont = Content.Load<SpriteFont>("ActTitleFont");
                keysTexture = Content.Load<Texture2D>("carkeys");
                rollerBladesTexture = Content.Load<Texture2D>("rollerblades");
                wizardTexture = Content.Load<Texture2D>("mikewizard");
                mikeTheme = Content.Load<Song>("miketheme");
            }

            MediaPlayer.Stop();
            bill.speed = new Vector2(4500, 4500);
            bill.Width *= 2;
            bill.Height *= 2;
            billHealthBar.Width *= 2;
            bill.HP = 0;
            Bullet.Cans.Clear();
            PowerUp.RemoveAlivePowerUps();
            string status = "BAHUHUHUHUHUHUHUH";
            currentBillMessage = LOWHPMESSAGE;
            Game1.Game.Window.Title = status;
            soundEffectManager.Play(billDeath, 1f);

            keys = new BaseObject(new Rectangle(0, 0, 50, 50));
            keys.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height / 2);
            keys.Texture = keysTexture;

            wizard = new BaseObject(new Rectangle(0, 0, WIZARDWIDTH, WIZARDHEIGHT));
            wizard.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height * .48f);
            wizard.Texture = wizardTexture;

            rollerBlades = new BaseObject(new Rectangle(0, 0, ROLLERBLADESWIDTH, ROLLERBLADESHEIGHT));
            rollerBlades.CenterPoint = new Vector2(Graphics.GraphicsDevice.Viewport.Width / 2, Graphics.GraphicsDevice.Viewport.Height * .9f);
            rollerBlades.Texture = rollerBladesTexture;
        }

        bool part2Started = false, part3Started = false, billRunning = false, keysPickedUp = false;

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

            if (part3Started && Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Enter))
            {
                returnControl("entercarchase");
                return;
            }

            // mute check
            checkForMute();

            if (billRunning && bill.X >= Graphics.GraphicsDevice.Viewport.Width) 
            {
                if ((DateTime.Now - startTime).TotalSeconds < 4)
                    return;
                billRunning = false;
                part3Started = true;
                MediaPlayer.Play(mikeTheme);
                MediaPlayer.Volume = .5f;
            }

            if (part3Started)
            {
            }
            else
            {
                if ((DateTime.Now - startTime).TotalSeconds < 1)
                {
                    bill.moveTowards(isaac.CenterPoint, gameTime, true);
                    isaac.CalculateCorners();
                    bill.CalculateCorners();
                    checkForIsaacDeath();
                }
                else
                {
                    if (!part2Started)
                    {
                        Game1.Game.Window.Title = "";
                        part2Started = true;
                    }
                    else
                    {
                        if ((DateTime.Now - startTime).TotalSeconds < 2)
                            return;
                        if (!billRunning)
                        {
                            bill.Width /= 2;
                            bill.Height /= 2;
                            bill.CenterPoint = new Vector2(-(bill.Width / 2), Graphics.GraphicsDevice.Viewport.Height / 2);
                            billRunning = true;
                        }
                        else
                        {
                            bill.X += (int)Math.Round(Util.ScaleWithGameTime(1000, gameTime));
                            if (keys.Rectangle.Contains((int)bill.CenterPoint.X, (int)bill.CenterPoint.Y))
                                keysPickedUp = true;
                            if (keysPickedUp)
                                keys.CenterPoint = bill.CenterPoint;
                        }
                    }
                }
            }

            //SoundEffectManager.Cleanup();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GraphicsDevice.Clear(Color.Gray);

            if (part3Started)
            {
                spriteBatch.DrawString(actTitleFont, "Act II", new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - actTitleFont.MeasureString("Act II").X / 2), (int)(actTitleFont.MeasureString("Act II").Y)), Color.Black);
                spriteBatch.Draw(wizard.Texture, wizard, Color.White);
                spriteBatch.Draw(rollerBlades.Texture, rollerBlades, Color.White);

                string msg1 = "I found these roller blades. They took me to a very distinct place.";
                Vector2 msg1Size = billStatusFont.MeasureString(msg1);
                spriteBatch.DrawString(billStatusFont, msg1, new Vector2((int)(wizard.X + wizard.Width / 2 - msg1Size.X / 2), (int)(wizard.Y - msg1Size.Y * 1.5f)), Color.Black);

                string msg2 = "You'll need them.";
                Vector2 msg2Size = billStatusFont.MeasureString(msg2);
                spriteBatch.DrawString(billStatusFont, msg2, new Vector2((int)(wizard.X + wizard.Width / 2 - msg2Size.X / 2), (int)(wizard.Y + wizard.Height + msg2Size.Y * .75f)), Color.Black);

                string continueMsg = "Press enter to continue.";
                Vector2 continueMsgSize = powerUpFont.MeasureString(continueMsg);
                spriteBatch.DrawString(powerUpFont, continueMsg, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width - continueMsgSize.X - 2), (int)(Graphics.GraphicsDevice.Viewport.Height - continueMsgSize.Y)), Color.Black);
            }
            else if (part2Started)
            {
                spriteBatch.DrawString(actTitleFont, "Act II", new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - actTitleFont.MeasureString("Act II").X / 2), (int)(actTitleFont.MeasureString("Act II").Y)), Color.Black);
                if (billRunning)
                    spriteBatch.Draw(bill.Texture, bill, Color.White);
                spriteBatch.Draw(keys.Texture, keys, Color.White);
            }
            else
            {
                string msg = "MARDAR";
                Vector2 msgSize = endBattle1Font1.MeasureString(msg);
                spriteBatch.DrawString(endBattle1Font1, msg, new Vector2((int)(Graphics.GraphicsDevice.Viewport.Width / 2 - msgSize.X / 2), (int)(Graphics.GraphicsDevice.Viewport.Height / 2 - msgSize.Y / 2)), Color.Black);
                spriteBatch.Draw(isaac.Texture, isaac, Color.White);
                spriteBatch.Draw(bill.Texture, bill, Color.White);
            }
        }
    }
}