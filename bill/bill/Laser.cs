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
    class Laser
    {
        public static List<Laser> Lasers = new List<Laser>();
        public static Texture2D Texture;

        public Vector2 Speed;
        public List<BaseObject> Bits = new List<BaseObject>();
        private BaseObject firstBit;
        private Vector2 position;
        private bool isFiring = true;
        private int size, length, shootDelay = (int)Math.Round(1000 / 100f), timeSinceLastShot;
        float angle;

        public Laser(Vector2 position, Vector2 speed, int size, int length)
        {
            this.position = position;
            firstBit = new BaseObject(new Rectangle((int)position.X, (int)position.Y, size, size));
            Speed = speed;
            this.size = size;
            this.length = length;
            angle = (float)Math.Atan2(speed.Y, speed.X);
            timeSinceLastShot = shootDelay;
            Lasers.Add(this);
        }

        public static void UpdateLasers(GameTime gameTime)
        {
            for (int i = 0; i < Lasers.Count; )
            {
                if (Lasers[i].Update(gameTime))
                    i++;
            }
        }

        public bool Update(GameTime gameTime)
        {
            MoveBits(gameTime);

            if (isFiring && Vector2.Distance(position, firstBit.CenterPoint) + size / 2f >= length)
            {
                isFiring = false;
                return true;
            }

            if (isFiring)
            {
                timeSinceLastShot += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (timeSinceLastShot >= shootDelay)
                {
                    Shoot();
                    timeSinceLastShot = 0;
                }
            }
            else if (Bits.Count == 1 && Bits[0] == firstBit)
            {
                Lasers.Remove(this);
                return false;
            }

            return true;
        }

        void Shoot()
        {
            BaseObject bit = new BaseObject(new Rectangle((int)position.X, (int)position.Y, size, size), Speed);
            bit.Texture = Texture;
            bit.Rotation = angle;// +(float)Math.PI / 2;
            Bits.Add(bit);

            if (Bits.Count == 1)
                firstBit = bit;
        }

        public bool IsFiring
        {
            get
            {
                return isFiring;
            }
            set
            {
                isFiring = value;
            }
        }
        public float ShotsPerSecond
        {
            set
            {
                timeSinceLastShot = shootDelay = (int)Math.Round(1000 / value);
            }
        }
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }
        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        void MoveBits(GameTime gameTime)
        {
            for (int i = 0; i < Bits.Count; )
            {
                BaseObject bit = Bits[i];

                bit.movePrecise(bit.speed, gameTime, false);

                if (bit != firstBit &&
                    (bit.Y < bit.minY - bit.Height || bit.Y > bit.maxY + bit.Height ||
                    bit.X < bit.minX - bit.Width || bit.X > bit.maxX + bit.Width))
                {
                    Bits.Remove(bit);
                }
                else
                    i++;
            }
        }

        static PrimitiveLine line;
        public static void InitializeLine(GraphicsDevice graphicsDevice)
        {
            line = new PrimitiveLine(graphicsDevice, 1);
            line.Colour = Color.Red;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Bits.Count > 0)
            {
                BaseObject frontBit = Bits[0], endBit = Bits[Bits.Count - 1];
                for (int i = 0; i < size; i++)
                {
                    line.ClearVectors();
                    line.AddVector(new Vector2(endBit.X + i, endBit.Y));
                    line.AddVector(new Vector2(frontBit.X + i, frontBit.Y));
                    line.Render(spriteBatch, angle);

                    line.ClearVectors();
                    line.AddVector(new Vector2(endBit.X, endBit.Y + i));
                    line.AddVector(new Vector2(frontBit.X, frontBit.Y + i));
                    line.Render(spriteBatch, angle);

                    line.ClearVectors();
                    line.AddVector(new Vector2(endBit.X + size, endBit.Y + i));
                    line.AddVector(new Vector2(frontBit.X + size, frontBit.Y + i));
                    line.Render(spriteBatch, angle);

                    line.ClearVectors();
                    line.AddVector(new Vector2(endBit.X + i, endBit.Y + size));
                    line.AddVector(new Vector2(frontBit.X + i, frontBit.Y + size));
                    line.Render(spriteBatch, angle);
                }
                spriteBatch.Draw(frontBit.Texture, frontBit, Color.White);
                //spriteBatch.Draw(frontBit.Texture, new Rectangle((int)frontBit.CenterPoint.X, (int)frontBit.CenterPoint.Y, frontBit.Width, frontBit.Height), null, Color.White, frontBit.Rotation, frontBit.TextureCenterOrigin, SpriteEffects.None, 0f);
            }
        }
    }

    class Laser2 : BaseObject
    {
        public static List<Laser2> Lasers = new List<Laser2>();
        new public static Texture2D Texture;

        public float Speed;
        private bool isFiring = true;
        private int size, length;
        private float preciseWidth;

        private Vector2 startPosition;

        public Laser2(Vector2 position, float speed, float angle, int size, int length)
            : base(new Rectangle(0, 0, 1, size))
        {
            startPosition = position;
            Position = position;
            Speed = speed;
            this.speed = new Vector2(speed * (float)Math.Cos(angle), speed * (float)Math.Sin(angle));
            this.size = size;
            this.length = length;
            preciseWidth = Width;
            //Rotation = (float)Math.Atan2(speed.Y, speed.X);
            Rotation = angle;
            base.Texture = Texture;
            Lasers.Add(this);
        }

        public static void UpdateLasers(GameTime gameTime)
        {
            for (int i = 0; i < Lasers.Count; )
            {
                Laser2 l = Lasers[i];
                l.Update(gameTime);
                if (l.Y < l.minY - l.Width || l.Y > l.maxY + l.Width ||
                    l.X < l.minX - l.Width || l.X > l.maxX + l.Width)
                    Lasers.Remove(l);
                else
                    i++;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (isFiring && preciseWidth >= length)
            {
                isFiring = false;
            }

            if (isFiring)
            {
                PreciseWidth += Util.ScaleWithGameTime(Speed, gameTime);
                CenterPoint = startPosition + new Vector2(Width / 2 * (float)Math.Cos(Rotation), Width / 2 * (float)Math.Sin(Rotation));
            }
            else
            {
                movePrecise(gameTime, false);
            }
        }

        public float PreciseWidth
        {
            get
            {
                return preciseWidth;
            }
            set
            {
                preciseWidth = value;
                Width = (int)Math.Round(value);
            }
        }
        new public int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                preciseWidth = value;
            }
        }
        public bool IsFiring
        {
            get
            {
                return isFiring;
            }
            set
            {
                isFiring = value;
            }
        }
        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        static PrimitiveLine line;
        public static void InitializeLine(GraphicsDevice graphicsDevice)
        {
            line = new PrimitiveLine(graphicsDevice, 1);
            line.Colour = Color.Red;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            /*spriteBatch.Draw(base.Texture, new Rectangle((int)UpperLeftCorner.X, (int)UpperLeftCorner.Y, 4, 4), Color.White);
            spriteBatch.Draw(base.Texture, new Rectangle((int)UpperRightCorner.X, (int)UpperRightCorner.Y, 4, 4), Color.White);
            spriteBatch.Draw(base.Texture, new Rectangle((int)LowerLeftCorner.X, (int)LowerLeftCorner.Y, 4, 4), Color.White);
            spriteBatch.Draw(base.Texture, new Rectangle((int)LowerRightCorner.X, (int)LowerRightCorner.Y, 4, 4), Color.White);
            */
            Vector2 vector1 = LowerLeftCorner, vector2 = LowerRightCorner;

            float angle = (float)Math.Atan2((double)(UpperLeftCorner.Y - LowerLeftCorner.Y), (double)UpperLeftCorner.X - LowerLeftCorner.X);

            //spriteBatch.Draw(Texture, new Rectangle(X, Y, Width, Height), null, Color.White, Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0f);

            line.ClearVectors();
            line.AddVector(vector1);
            line.AddVector(vector2);
            line.Render(spriteBatch, Rotation);

            while (Vector2.Distance(vector1, UpperLeftCorner) > .5f)
            {
                Vector2 adjustment = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                vector1 += adjustment;
                vector2 += adjustment;

                line.ClearVectors();
                line.AddVector(vector1);
                line.AddVector(vector2);
                line.Render(spriteBatch, Rotation);
            }

            /*line.ClearVectors();
            line.AddVector(new Vector2(UpperLeftCorner.X, UpperLeftCorner.Y));
            line.AddVector(new Vector2(UpperRightCorner.X, UpperRightCorner.Y));
            line.Render(spriteBatch, Rotation);*/
        }
    }
}