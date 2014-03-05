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
    class Bullet : BaseObject
    {
        public static List<Bullet> Peanuts = new List<Bullet>();
        public static List<Bullet> Cans = new List<Bullet>();

        private BulletType bulletType;
        private bool isCan = false, isPeanut = false;

        public Bullet(BulletType bulletType, Vector2 speed)
            : base(new Rectangle(0, 0, (int)bulletType.Size.X, (int)bulletType.Size.Y), speed)
        {
            //this.X = (int)bulletType.Size.X;
            //this.Y = (int)bulletType.Size.Y;
            this.Texture = bulletType.Texture;
            this.bulletType = bulletType;
            if (bulletType == BulletType.Peanut)
            {
                isPeanut = true;
                Peanuts.Add(this);
            }
            else if (BulletType.Cans.Contains<BulletType>(bulletType))
            {
                isCan = true;
                Cans.Add(this);
            }
        }

        // returns true if it should be removed
        public virtual bool Move(GameTime gameTime)
        {
            movePrecise(gameTime, false);

            return IsOffScreen(Game1.Game.GraphicsDevice.Viewport, null);
        }

        public BulletType Type
        {
            get
            {
                return this.bulletType;
            }
        }

        public bool IsCan
        {
            get
            {
                return isCan;
            }
        }
        public bool IsPeanut
        {
            get
            {
                return isPeanut;
            }
        }

        public static void moveBullets(GameTime gameTime)
        {
            // peanuts
            for (int i = 0; i < Bullet.Peanuts.Count; i++)
            {
                Bullet b = Bullet.Peanuts[i];

                if (b.Move(gameTime))
                {
                    Bullet.Peanuts.Remove(b);
                    i--;
                }

            }
            // cans
            for (int i = 0; i < Bullet.Cans.Count; i++)
            {
                Bullet b = Bullet.Cans[i];

                if (b.Move(gameTime))
                {
                    Bullet.Cans.Remove(b);
                    i--;
                }
            }
        }
    }

    class ExplodingBullet : Bullet
    {
        public static List<Vector2> Explosions = new List<Vector2>();

        Vector2 target;

        public ExplodingBullet(BulletType type, float speed, Vector2 target)
            : base(type, new Vector2(speed, speed))
        {
            this.target = target;
        }

        public override bool Move(GameTime gameTime)
        {
            moveTowardsPrecise(target, gameTime, false);

            if (centerPoint == target)
            {
                Explosions.Add(target);
                return true;
            }

            return IsOffScreen(Game1.Game.GraphicsDevice.Viewport, null);
        }
    }

    class BulletType
    {
        private static Texture2D normalTexture, fireTexture, cokeCanTexture, crushCanTexture,
            drPepperCanTexture, pepsiCanTexture, sierraMistCanTexture;

        private static BulletType peanut = new BulletType(new Vector2(10, 10), normalTexture),
            fireBall = new BulletType(new Vector2(15, 15), fireTexture),
            cokeCan = new BulletType(new Vector2(18, 24), cokeCanTexture),
            crushCan = new BulletType(new Vector2(18, 24), crushCanTexture),
            drPepperCan = new BulletType(new Vector2(18, 24), drPepperCanTexture),
            pepsiCan = new BulletType(new Vector2(18, 24), pepsiCanTexture),
            sierraMistCan = new BulletType(new Vector2(18, 24), sierraMistCanTexture);

        private static BulletType[] cans = new BulletType[] { cokeCan, crushCan, drPepperCan, pepsiCan, sierraMistCan };

        private static int idCounter = 0;

        private int id;
        private Vector2 size;
        private Texture2D texture;

        private BulletType(Vector2 size, Texture2D texture)
        {
            this.size = size;
            this.texture = texture;
            this.id = idCounter++;
        }

        public static BulletType Peanut
        {
            get
            {
                return peanut;
            }
        }
        public static BulletType FireBall
        {
            get
            {
                return fireBall;
            }
        }
        public static Texture2D NormalTexture
        {
            set
            {
                normalTexture = value;
                peanut.Texture = value;
            }
        }
        public static Texture2D FireTexture
        {
            set
            {
                fireTexture = value;
                fireBall.Texture = value;
            }
        }
        public static Texture2D CokeCanTexture
        {
            set
            {
                cokeCanTexture = value;
                cokeCan.Texture = value;
            }
        }
        public static Texture2D CrushCanTexture
        {
            set
            {
                crushCanTexture = value;
                crushCan.Texture = value;
            }
        }
        public static Texture2D DrPepperCanTexture
        {
            set
            {
                drPepperCanTexture = value;
                drPepperCan.Texture = value;
            }
        }
        public static Texture2D PepsiCanTexture
        {
            set
            {
                pepsiCanTexture = value;
                pepsiCan.Texture = value;
            }
        }
        public static Texture2D SierraMistCanTexture
        {
            set
            {
                sierraMistCanTexture = value;
                sierraMistCan.Texture = value;
            }
        }
        public static BulletType[] Cans
        {
            get
            {
                return cans;
            }
        }
        public Vector2 Size
        {
            get
            {
                return size;
            }
        }
        public Texture2D Texture
        {
            set
            {
                this.texture = value;
            }
            get
            {
                return this.texture;
            }
        }

        public override bool Equals(object o)
        {
            if (!(o is BulletType))
                return false;
            return this.id == ((BulletType)o).id;
        }
        public override int GetHashCode()
        {
            return (int)(this.Size.X + this.Size.Y);
        }

        public static bool operator ==(BulletType d1, BulletType d2)
        {
            return d1.Equals(d2);
        }
        public static bool operator !=(BulletType d1, BulletType d2)
        {
            return !d1.Equals(d2);
        }
    }
}