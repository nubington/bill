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
    class CrystalGun : BaseObject
    {
        BaseObject board, wheel1, wheel2;

        int shootDelay, timeSinceLastShot;
        int bulletSize;
        float bulletSpeed;
        float moveSpeed;
        float turnSpeed;
        float wheelCircumference;

        public CrystalGun(Rectangle rectangle, Rectangle boardRectangle, int wheelSize, Texture2D gunTexture, Texture2D boardTexture, Texture2D wheelTexture) 
            : base(rectangle)
        {
            Texture = gunTexture;

            board = new BaseObject(boardRectangle);
            board.CenterPoint = Position;
            board.Texture = boardTexture;

            wheel1 = new BaseObject(new Rectangle(0, 0, wheelSize, wheelSize));
            wheel1.PrecisePosition = board.CenterPoint - new Vector2(26, 0);

            wheel2 = new BaseObject(new Rectangle(0, 0, wheelSize, wheelSize));
            wheel2.PrecisePosition = board.CenterPoint + new Vector2(26, 0);

            wheel1.Texture = wheel2.Texture = wheelTexture;
            wheelCircumference = wheelSize / 2 * (float)Math.PI;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(board.Texture, board, Color.White);

            // gun wheels
            spriteBatch.Draw(wheel1.Texture, new Rectangle(wheel1.X, wheel1.Y, wheel1.Width, wheel1.Height), null, Color.White, wheel1.Rotation, wheel1.TextureCenterOrigin, SpriteEffects.None, 0f);
            spriteBatch.Draw(wheel1.Texture, new Rectangle(wheel2.X, wheel2.Y, wheel2.Width, wheel2.Height), null, Color.White, wheel2.Rotation, wheel2.TextureCenterOrigin, SpriteEffects.None, 0f);

            // gun
            spriteBatch.Draw(Texture, new Rectangle(X, Y, Width, Height), null, Color.White, Rotation, new Vector2(100, Texture.Height / 2), SpriteEffects.None, 0f);

        }

        public void CheckForShoot(GameTime gameTime, Vector2 crossHairPoint)
        {
            timeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastShot >= shootDelay)
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    if (Vector2.Distance(Position, crossHairPoint) < Width)
                        return;

                    timeSinceLastShot = 0;

                    Vector2 shootPoint = Position + new Vector2(Width * (float)Math.Cos(Rotation), Width * (float)Math.Sin(Rotation));

                    //float distance = Vector2.Distance(shootPoint, crossHair.CenterPoint);
                    float distance = Vector2.Distance(Position, crossHairPoint) - Width;

                    Vector2 target = shootPoint + new Vector2(distance * (float)Math.Cos(Rotation), distance * (float)Math.Sin(Rotation));

                    Bullet b = new ExplodingBullet(BulletType.Peanut, bulletSpeed, target);
                    b.Width = bulletSize;
                    b.Height = bulletSize;
                    b.CenterPoint = shootPoint;
                }
            }
        }

        public void MoveTowardsCrossHair(GameTime gameTime, Vector2 crossHairPoint, GraphicsDevice graphicsDevice)
        {
            Vector2 difference = crossHairPoint - Position;
            if (difference.X == 0)
                return;
            if (Math.Abs(difference.X) < Util.ScaleWithGameTime(MoveSpeed, gameTime))
            {
                Position = new Vector2(crossHairPoint.X, Position.Y);
                Wheel1Rotation += difference.X / wheelCircumference * (float)Math.PI;
                Wheel2Rotation += difference.X / wheelCircumference * (float)Math.PI;
                return;
            }

            float actualMoveSpeed = Util.ScaleWithGameTime(MoveSpeed, gameTime);

            // move right
            if (difference.X > 0)
            {
                Position += new Vector2(actualMoveSpeed, 0);

                // keep on screen - use PreciseX to prevent wheel rotation at edges
                if (wheel2.X + wheel2.Width / 2 > graphicsDevice.Viewport.Width)
                {
                    PositionX -= (wheel2.X + wheel2.Width / 2) - graphicsDevice.Viewport.Width;
                    return;
                }

                // rotate wheels
                wheel1.Rotation += actualMoveSpeed / wheelCircumference * (float)Math.PI;
                wheel2.Rotation += actualMoveSpeed / wheelCircumference * (float)Math.PI;
            }
            // move left
            else if (difference.X < 0)
            {
                Position -= new Vector2(actualMoveSpeed, 0);

                // keep on screen - use PreciseX to prevent wheel rotation at edges
                if (wheel1.X - (float)wheel1.Width / 2 < 0)
                {
                    PositionX += -(wheel1.X - (float)wheel1.Width / 2);
                    return;
                }

                // rotate wheels
                wheel1.Rotation -= actualMoveSpeed / wheelCircumference * (float)Math.PI;
                wheel2.Rotation -= actualMoveSpeed / wheelCircumference * (float)Math.PI;
            }
        }

        public new Vector2 Position
        {
            get
            {
                return PrecisePosition;
            }
            set
            {
                PrecisePosition = value;
                board.CenterPoint = value;
                wheel1.PrecisePosition = value - new Vector2(26, 0);
                wheel2.PrecisePosition = value + new Vector2(26, 0);
            }
        }
        public float PositionX
        {
            get
            {
                return PrecisePosition.X;
            }
            set
            {
                Position = new Vector2(value, PositionY);
            }
        }
        public float PositionY
        {
            get
            {
                return PrecisePosition.Y;
            }
            set
            {
                Position = new Vector2(PositionX, value);
            }
        }

        public int ShootDelay
        {
            get
            {
                return shootDelay;
            }
            set
            {
                shootDelay = value;
                timeSinceLastShot = value;
            }
        }
        public int BulletSize
        {
            get
            {
                return bulletSize;
            }
            set
            {
                bulletSize = value;
            }
        }
        public float BulletSpeed
        {
            get
            {
                return bulletSpeed;
            }
            set
            {
                bulletSpeed = value;
            }
        }
        public float TurnSpeed
        {
            get
            {
                return turnSpeed;
            }
            set
            {
                turnSpeed = value;
            }
        }
        public float MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
            set
            {
                moveSpeed = value;
                speed = new Vector2(value, 0);
            }
        }

        public Texture2D BoardTexture
        {
            get
            {
                return board.Texture;
            }
        }
        public Texture2D WheelTexture
        {
            get
            {
                return wheel1.Texture;
            }
        }
        public float Wheel1Rotation
        {
            get
            {
                return wheel1.Rotation;
            }
            set
            {
                wheel1.Rotation = value;
            }
        }
        public float Wheel2Rotation
        {
            get
            {
                return wheel2.Rotation;
            }
            set
            {
                wheel2.Rotation = value;
            }
        }
    }
}
