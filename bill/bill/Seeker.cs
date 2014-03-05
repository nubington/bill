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
    class Seeker : BaseObject
    {
        public static List<Seeker> Seekers = new List<Seeker>();

        protected BaseObject target;
        protected float targetAngle, targetX, targetY;
        protected float rotationSpeed;
        protected bool seekOnce;

        public Seeker(Vector2 position, int size, BaseObject target, float speed, float rotationSpeed)
            : this(position, size, 0, speed, rotationSpeed)
        {
            this.target = target;
            Rotation = (float)Math.Atan2(target.CenterPoint.Y - CenterPoint.Y, target.CenterPoint.X - CenterPoint.X);
            LastMove = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
        }
        public Seeker(Vector2 position, int size, float angle, float speed, float rotationSpeed)
            : base(new Rectangle(0, 0, size, size), new Vector2(speed, speed))
        {
            PrecisePosition = position;
            this.rotationSpeed = rotationSpeed;
            Rotation = angle;
            LastMove = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
            //Seekers.Add(this);
        }

        public virtual void Seek(GameTime gameTime)
        {
            if (target != null)
            {
                targetAngle = MathHelper.WrapAngle((float)Math.Atan2(target.CenterPoint.Y - CenterPoint.Y, target.CenterPoint.X - CenterPoint.X));

                targetX = (float)Math.Cos(targetAngle);
                targetY = (float)Math.Sin(targetAngle);

                Vector3 oldAngleVector = new Vector3(LastMove.X, LastMove.Y, 0);
                Vector3 newAngleVector = new Vector3(targetX, targetY, 0);

                Vector3 crossProduct = Vector3.Cross(oldAngleVector, newAngleVector);

                if (crossProduct.Z > 0)
                    Rotation += Util.ScaleWithGameTime(rotationSpeed, gameTime);
                else if (crossProduct.Z < 0)
                    Rotation -= Util.ScaleWithGameTime(rotationSpeed, gameTime);

                if (seekOnce && Util.AngleDifference(Rotation, targetAngle) < Util.ScaleWithGameTime(rotationSpeed, gameTime))
                    target = null;
            }

            movePrecise(Rotation, gameTime, false);
        }

        public static void UpdateSeekers(GameTime gameTime)
        {
            foreach (Seeker s in Seekers)
                s.Seek(gameTime);
        }

        public BaseObject Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }
        public bool SeekOnce
        {
            get
            {
                return seekOnce;
            }
            set
            {
                seekOnce = value;
            }
        }
    }

    class DirectionalSeeker : Seeker
    {
        float directionAngle;

        public DirectionalSeeker(Vector2 position, int size, float angle, float directionAngle, float speed, float rotationSpeed)
            : base(position, size, angle, speed, rotationSpeed)
        {
            this.directionAngle = directionAngle;
            targetX = (float)Math.Cos(directionAngle);
            targetY = (float)Math.Sin(directionAngle);
        }

        public override void Seek(GameTime gameTime)
        {
            if (Rotation != directionAngle)
            {
                Vector3 oldAngleVector = new Vector3(LastMove.X, LastMove.Y, 0);
                Vector3 newAngleVector = new Vector3(targetX, targetY, 0);

                Vector3 crossProduct = Vector3.Cross(oldAngleVector, newAngleVector);

                if (crossProduct.Z > 0)
                    Rotation += Util.ScaleWithGameTime(rotationSpeed, gameTime);
                else if (crossProduct.Z < 0)
                    Rotation -= Util.ScaleWithGameTime(rotationSpeed, gameTime);

                if (Util.AngleDifference(Rotation, directionAngle) < Util.ScaleWithGameTime(rotationSpeed, gameTime))
                    Rotation = directionAngle;
            }

            movePrecise(Rotation, gameTime, false);
        }
    }

    class Magnet : Seeker
    {
        public Magnet(Vector2 position, int size, BaseObject target, float speed, float rotationSpeed)
            : base(position, size, target, speed, rotationSpeed)
        {
        }

        public override void Seek(GameTime gameTime)
        {
            //base.Seek(gameTime);
            targetAngle = (float)Math.Atan2(target.CenterPoint.Y - CenterPoint.Y, target.CenterPoint.X - CenterPoint.X);

            targetX = (float)Math.Cos(targetAngle);
            targetY = (float)Math.Sin(targetAngle);

            Vector2 thisEdge = new Vector2(CenterPoint.X + Width / 2 * targetX, CenterPoint.Y + Height / 2 * targetY);
            //Vector2 thisEdge = new Vector2(CenterPoint.X + Width / 2 * lastMove.X, CenterPoint.Y + Height / 2 * lastMove.Y);

            //if (!target.Rectangle.Contains((int)Math.Round(thisEdge.X), (int)Math.Round(thisEdge.Y)))
            if (!target.Touches(thisEdge))
            {
                float distance = Width / 2 + target.Width / 2;
                CenterPoint = new Vector2(target.CenterPoint.X + distance * -targetX, target.CenterPoint.Y + distance * -targetY);
                //CenterPoint = new Vector2(target.CenterPoint.X + distance * -lastMove.X, target.CenterPoint.Y + distance * -lastMove.Y);
            }
        }
    }

    class SeekerSnake
    {
        public static List<SeekerSnake> SeekerSnakes = new List<SeekerSnake>();

        protected Vector2 position;
        protected float speed, rotationSpeed;
        protected int size, length;
        //protected BaseObject target;
        protected Seeker frontBit;
        public List<Seeker> Bits = new List<Seeker>();

        public SeekerSnake(Vector2 position, float speed, int size, int length, BaseObject target, float rotationSpeed)
            : this(position, speed, size, length, 0, rotationSpeed)
        {
            Target = target;
            Angle = (float)Math.Atan2(target.CenterPoint.Y - frontBit.CenterPoint.Y, target.CenterPoint.X - frontBit.CenterPoint.X);
        }
        public SeekerSnake(Vector2 position, float speed, int size, int length, float angle, float rotationSpeed)
        {
            this.position = position;
            this.speed = speed;
            this.size = size;
            this.length = length;
            this.rotationSpeed = rotationSpeed;

            frontBit = new Seeker(position, size, angle, speed, rotationSpeed);
            frontBit.CenterPoint = position;
            Bits.Add(frontBit);

            for (int i = 1; i < length; i++)
            {
                Magnet bit = new Magnet(position, size, Bits[i - 1], speed, rotationSpeed);
                bit.CenterPoint = position;
                Bits.Add(bit);
            }

            SeekerSnakes.Add(this);
        }

        public void Seek(GameTime gameTime)
        {
            foreach (Seeker bit in Bits)
                bit.Seek(gameTime);
        }

        public virtual void Remove()
        {
            SeekerSnakes.Remove(this);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Seeker bit in Bits)
                spriteBatch.Draw(bit.Texture, new Rectangle((int)Math.Round(bit.CenterPoint.X), (int)Math.Round(bit.CenterPoint.Y), bit.Width, bit.Height), null, Color.White, bit.Rotation, bit.TextureCenterOrigin, SpriteEffects.None, 0f);
        }

        public virtual bool Update(GameTime gameTime)
        {
            foreach (Seeker bit in Bits)
                bit.Seek(gameTime);
            return true;
        }

        public static void UpdateSeekerSnakes(GameTime gameTime)
        {
            for (int i = 0; i < SeekerSnakes.Count; )
            {
                if (SeekerSnakes[i].Update(gameTime))
                    i++;
            }
        }

        public static void DrawSeekerSnakes(SpriteBatch spriteBatch)
        {
            foreach (SeekerSnake s in SeekerSnakes)
                s.Draw(spriteBatch);
        }

        public float Angle
        {
            get
            {
                return frontBit.Rotation;
            }
            set
            {
                frontBit.Rotation = value;
                frontBit.LastMove = new Vector2((float)Math.Cos(value), (float)Math.Sin(value));
            }
        }
        public BaseObject FrontBit
        {
            get
            {
                return frontBit;
            }
        }
        public BaseObject Target
        {
            get
            {
                return frontBit.Target;
            }
            set
            {
                frontBit.Target = value;
            }
        }
        public Texture2D Texture
        {
            get
            {
                return frontBit.Texture;
            }
            set
            {
                foreach (Seeker bit in Bits)
                    bit.Texture = value;
            }
        }
        public bool IsOffScreen
        {
            get
            {
                foreach (Seeker bit in Bits)
                    if (!bit.IsOffScreen(Game1.Game.GraphicsDevice.Viewport, null))
                        return false;
                return true;
            }
        }

    }

    class MouseSeeker : SeekerSnake
    {
        public static List<MouseSeeker> MouseSeekers = new List<MouseSeeker>();

        public static Texture2D MouseTexture, CordTexture, PlugTexture;

        public MouseSeeker(Vector2 position, float speed, int mouseSize, int cordSize, int length, float directionAngle, float rotationSpeed)
            : base(position, speed, mouseSize, length, directionAngle, rotationSpeed)
        {
            //Seeker.Seekers.Remove(frontBit);
            //Bits.Remove(frontBit);
            //frontBit = new DirectionalSeeker(position, mouseSize, Angle, directionAngle, speed, rotationSpeed);
            //frontBit.CenterPoint = position;
            //Bits.Insert(0, frontBit);
            Bits[0] = new DirectionalSeeker(position, mouseSize, Angle, directionAngle, speed, rotationSpeed);
            frontBit = Bits[0];
            frontBit.CenterPoint = position;
            frontBit.SeekOnce = true;
            frontBit.Texture = MouseTexture;
            Bits[1].Target = frontBit;

            for (int i = 1; i < length - 1; i++)
            {
                Bits[i].Texture = CordTexture;
                Bits[i].Width = Bits[i].Height = cordSize;
                Bits[i].CenterPoint = position;
            }

            Seeker plug = Bits[length - 1];
            plug.Texture = PlugTexture;
            plug.Width = plug.Height = cordSize * 2;
            plug.CenterPoint = position;

            MouseSeekers.Add(this);
        }

        public override void Remove()
        {
            MouseSeekers.Remove(this);
            base.Remove();
            //foreach (Seeker bit in Bits)
                //Seeker.Seekers.Remove(bit);
        }
        public static void RemoveAll()
        {
            for (int i = 0; i < MouseSeekers.Count; )
                MouseSeekers[i].Remove();
        }

        public override bool Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsOffScreen)
            {
                Remove();
                return false;
            }
            return true;
        }
    }
}