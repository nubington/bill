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
    class Character : BaseObject
    {
        public List<PowerUp> ActivePowerUps = new List<PowerUp>();
        private int hp, maxHp;

        private int shootDelay = 0;
        private int timeSinceLastShot = 0;

        public Character(Rectangle rectangle, Vector2 speed)
            : base(rectangle, speed)
        { }

        public int HP
        {
            get
            {
                return hp;
            }
            set
            {
                hp = (int)MathHelper.Clamp(value, 0, MaxHP);
            }
        }
        public int MaxHP
        {
            get
            {
                return maxHp;
            }
            set
            {
                maxHp = value;
            }
        }
        public float Speed
        {
            get
            {
                return (int)speed.X;
            }
            set
            {
                speed.X = speed.Y = value;
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
            }
        }
        public int TimeSinceLastShot
        {
            get
            {
                return timeSinceLastShot;
            }
            set
            {
                timeSinceLastShot = value;
            }
        }

        // isaac shoot
        public void shootBullet(BulletType bulletType, Direction d, int speed)
        {
            shootBullet(bulletType, d, speed, 1);
        }
        public void shootBullet(BulletType bulletType, Direction d, int speed, float scale)
        {
            if (!this.hasPowerUp(PowerUpType.DoubleShot))
            {
                Bullet bullet = new Bullet(bulletType, new Vector2());
                bullet.Width = (int)(bullet.Width * scale);
                bullet.Height = (int)(bullet.Height * scale);

                if (d == Direction.North)
                {
                    bullet.X = this.X + this.Width / 2 - bullet.Width / 2;
                    bullet.Y = this.Y - bullet.Height;
                }
                else if (d == Direction.South)
                {
                    bullet.X = this.X + this.Width / 2 - bullet.Width / 2;
                    bullet.Y = this.Y + this.Height;
                }
                else if (d == Direction.West)
                {
                    bullet.X = this.X - bullet.Width;
                    bullet.Y = this.Y + this.Height / 2 - bullet.Height / 2;
                }
                else if (d == Direction.East)
                {
                    bullet.X = this.X + this.Width;
                    bullet.Y = this.Y + this.Height / 2 - bullet.Height / 2;
                }
                else if (d == Direction.NorthEast)
                {
                    bullet.X = this.X + this.Width;
                    bullet.Y = this.Y - bullet.Height;
                }
                else if (d == Direction.SouthEast)
                {
                    bullet.X = this.X + this.Width;
                    bullet.Y = this.Y + this.Height;
                }
                else if (d == Direction.NorthWest)
                {
                    bullet.X = this.X - bullet.Width;
                    bullet.Y = this.Y - bullet.Height;
                }
                else if (d == Direction.SouthWest)
                {
                    bullet.X = this.X - bullet.Width;
                    bullet.Y = this.Y + this.Height;
                }

                bullet.speed.X = speed * d.X;
                bullet.speed.Y = speed * d.Y;
            }
            else
            {
                Bullet bullet1 = new Bullet(bulletType, new Vector2());
                Bullet bullet2 = new Bullet(bulletType, new Vector2());
                bullet1.Width = (int)(bullet1.Width * scale);
                bullet1.Height = (int)(bullet1.Height * scale);
                bullet2.Width = (int)(bullet2.Width * scale);
                bullet2.Height = (int)(bullet2.Height * scale);

                if (d == Direction.North)
                {
                    bullet1.X = this.X + (int)(this.Width * .33f) - bullet1.Width / 2;
                    bullet2.X = this.X + (int)(this.Width * .66f) - bullet2.Width / 2;
                    bullet1.Y = bullet2.Y = this.Y - bullet1.Height;
                }
                else if (d == Direction.South)
                {
                    bullet1.X = this.X + (int)(this.Width * .33f) - bullet1.Width / 2;
                    bullet2.X = this.X + (int)(this.Width * .66f) - bullet2.Width / 2;
                    bullet1.Y = bullet2.Y = this.Y + this.Height;
                }
                else if (d == Direction.West)
                {
                    bullet1.X = bullet2.X = this.X - bullet1.Width;
                    bullet1.Y = this.Y + (int)(this.Height * .33f) - bullet1.Height / 2;
                    bullet2.Y = this.Y + (int)(this.Height * .66f) - bullet1.Height / 2;
                }
                else if (d == Direction.East)
                {
                    bullet1.X = bullet2.X = this.X + this.Width;
                    bullet1.Y = this.Y + (int)(this.Height * .33f) - bullet1.Height / 2;
                    bullet2.Y = this.Y + (int)(this.Height * .66f) - bullet1.Height / 2;
                }
                else if (d == Direction.NorthEast)
                {
                    bullet1.X = this.X + (int)(this.Width * .85f) - bullet1.Width / 2;
                    bullet1.Y = this.Y - bullet1.Height;
                    bullet2.X = this.X + this.Width;
                    bullet2.Y = this.Y + (int)(this.Height * .15f) - bullet2.Height / 2;
                }
                else if (d == Direction.SouthEast)
                {
                    bullet1.X = this.X + (int)(this.Width * .85f) - bullet1.Width / 2;
                    bullet1.Y = this.Y + this.Height;
                    bullet2.X = this.X + this.Width;
                    bullet2.Y = this.Y + (int)(this.Height * .85f) - bullet2.Height / 2;
                }
                else if (d == Direction.NorthWest)
                {
                    bullet1.X = this.X + (int)(this.Width * .15f) - bullet1.Width / 2;
                    bullet1.Y = this.Y - bullet1.Height;
                    bullet2.X = this.X - bullet2.Width;
                    bullet2.Y = this.Y + (int)(this.Height * .15f) - bullet2.Height / 2;
                }
                else if (d == Direction.SouthWest)
                {
                    bullet1.X = this.X + (int)(this.Height * .15f) - bullet1.Width / 2;
                    bullet1.Y = this.Y + this.Height;
                    bullet2.X = this.X - bullet2.Width;
                    bullet2.Y = this.Y + (int)(this.Height * .85f) - bullet2.Height / 2;
                }
                bullet1.speed.X = bullet2.speed.X = speed * d.X;
                bullet1.speed.Y = bullet2.speed.Y = speed * d.Y;
            }
        }
        // bill shoot
        public void shootBullet(BulletType bulletType, Vector2 target, int speed)
        {
            shootBullet(bulletType, target, speed, 0, 1);
        }
        public void shootBullet(BulletType bulletType, Vector2 target, int speed, double rotation, float scale)
        {
            float angle = (float)Math.Atan2((double)(target.Y - CenterPoint.Y), (double)(target.X - CenterPoint.X));

            float moveX = speed * (float)Math.Cos(angle);
            float moveY = speed * (float)Math.Sin(angle);

            Bullet b = new Bullet(bulletType, new Vector2(moveX, moveY));

            b.Y = (int)this.CenterPoint.Y;
            b.X = (int)this.CenterPoint.X + b.Width / 4;

            b.Rotation = (float)rotation;
            b.Width = (int)(b.Width * scale);
            b.Height = (int)(b.Height * scale);
        }

        public void acquirePowerUp(PowerUp p)
        {
            if (p.Charges == 0)
                p.ActiveTimer.Start();

            // remove previous powerup of same type or increase charges
            for (int i = 0; i < ActivePowerUps.Count; )
            {
                if (ActivePowerUps[i].Type == p.Type)
                {
                    if (p.Type == PowerUpType.SledgeHammer || p.Type == PowerUpType.CanShield)
                        ActivePowerUps[i].Charges += p.Type.Charges;
                    else
                    {
                        ActivePowerUps.RemoveAt(i);
                        ActivePowerUps.Insert(i, p);
                    }
                    return;
                }
                else
                    i++;
            }
            if (p.Charges == 0)
                p.ActiveTimer.Start();
            this.ActivePowerUps.Add(p);
        }
        public void updatePowerUps()
        {
            for (int i = 0; i < ActivePowerUps.Count; )
            {
                PowerUp p = ActivePowerUps[i];

                if (p.Type == PowerUpType.SledgeHammer)
                {
                    if (p.ActiveTimer.IsRunning)
                    {
                        if (p.ActiveTimer.ElapsedMilliseconds >= p.ActiveDuration)
                        {
                            p.ActiveTimer.Stop();
                            if (--p.Charges <= 0)
                                ActivePowerUps.Remove(p);
                            else
                            {
                                i++;
                                continue;
                            }
                        }
                        else
                        {
                            i++;
                            continue;
                        }
                    }
                    else
                    {
                        i++;
                        continue;
                    }
                }
                else
                {
                    if (p.ActiveDuration != -1 && p.ActiveTimer.ElapsedMilliseconds >= p.ActiveDuration)
                        ActivePowerUps.Remove(p);
                    else
                        i++;
                }
            }
        }
        public void removePowerUp(PowerUpType type)
        {
            for (int i = 0; i < ActivePowerUps.Count; )
            {
                PowerUp p = ActivePowerUps[i];
                if (p.Type == type)
                    ActivePowerUps.Remove(p);
                else
                    i++;
            }
        }
        public void removePowerUps()
        {
            ActivePowerUps.Clear();
        }
        public void removePowerUpsExcept(params PowerUpType[] types)
        {
            for (int i = 0; i < ActivePowerUps.Count; )
            {
                PowerUp p = ActivePowerUps[i];
                if (!types.Contains<PowerUpType>(p.Type))
                    ActivePowerUps.Remove(p);
                else
                    i++;
            }
        }
        public void reduceCanShieldCharges()
        {
            for (int i = 0; i < ActivePowerUps.Count; i++)
            {
                PowerUp p = ActivePowerUps[i];
                if (p.Type == PowerUpType.CanShield)
                {
                    if (p.Charges == 1)
                        ActivePowerUps.Remove(p);
                    else
                        p.Charges = p.Charges / 2;
                    return;
                }
            }
        }
        public bool hasPowerUp(PowerUpType type)
        {
            foreach (PowerUp p in ActivePowerUps)
                if (p.Type == type)
                    return true;
            return false;
        }
        public PowerUp getActivePowerUp(PowerUpType type)
        {
            foreach (PowerUp p in ActivePowerUps)
                if (p.Type == type)
                    return p;
            return null;
        }
    }
}
