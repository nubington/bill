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
    class PowerUp : BaseObject
    {
        public static List<PowerUp> AlivePowerUps = new List<PowerUp>();

        public const int DEFAULTPOWERUPWIDTH = 20, DEFAULTPOWERUPHEIGHT = 20;

        private int charges, aliveDuration, activeDuration;
        private Stopwatch aliveTimer, activeTimer;
        private PowerUpType type;

        private PowerUp(Rectangle rectangle, PowerUpType type)
            : base(rectangle, new Vector2(0, 0))
        {
            this.type = type;
            this.Texture = type.Texture;
            aliveDuration = type.AliveDuration;
            if (type == PowerUpType.SledgeHammer)
                activeDuration = GameSettings.DEFAULTSLEDGEHAMMERACTIVEDURATION;
            else if (type == PowerUpType.CanShield)
                activeDuration = GameSettings.DEFAULTCANSHIELDACTIVEDURATION;
            else
                activeDuration = type.ActiveDuration;
            Charges = type.Charges;
            aliveTimer = new Stopwatch();
            activeTimer = new Stopwatch();
            aliveTimer.Start();
        }

        public Stopwatch AliveTimer
        {
            get
            {
                return aliveTimer;
            }
        }
        public Stopwatch ActiveTimer
        {
            get
            {
                return activeTimer;
            }
        }
        public int Charges
        {
            get
            {
                return charges;
            }
            set
            {
                charges = value;
            }
        }
        public int AliveDuration
        {
            get
            {
                return aliveDuration;
            }
            set
            {
                aliveDuration = value;
            }
        }
        public int ActiveDuration
        {
            get
            {
                return activeDuration;
            }
            set
            {
                activeDuration = value;
            }
        }
        public int RemainingTimeAlive
        {
            get
            {
                return AliveDuration - (int)AliveTimer.ElapsedMilliseconds;
            }
        }
        public PowerUpType Type
        {
            get
            {
                return type;
            }
        }

        public static PowerUp DoubleShot
        {
            get
            {
                return new PowerUp(new Rectangle(0, 0, DEFAULTPOWERUPWIDTH, DEFAULTPOWERUPHEIGHT), PowerUpType.DoubleShot);
            }
        }
        public static PowerUp SpeedBoost
        {
            get
            {
                return new PowerUp(new Rectangle(0, 0, DEFAULTPOWERUPWIDTH, DEFAULTPOWERUPHEIGHT), PowerUpType.SpeedBoost);
            }
        }
        public static PowerUp CanShield
        {
            get
            {
                return new PowerUp(new Rectangle(0, 0, DEFAULTPOWERUPWIDTH, DEFAULTPOWERUPHEIGHT), PowerUpType.CanShield);
            }
        }
        public static PowerUp SledgeHammer
        {
            get
            {
                return new PowerUp(new Rectangle(0, 0, DEFAULTPOWERUPWIDTH, DEFAULTPOWERUPHEIGHT), PowerUpType.SledgeHammer);
            }
        }

        public static int Count
        {
            get
            {
                return AlivePowerUps.Count;
            }
        }
        public static PowerUp Random
        {
            get
            {
                int i = rand.Next(5);
                switch (i)
                {
                    case 0:
                        return DoubleShot;
                    case 1:
                        return SpeedBoost;
                    case 2:
                    case 3:
                        return CanShield;
                    case 4:
                        return SledgeHammer;
                }
                return null;
            }
        }

        public override string ToString()
        {
            if (Type == PowerUpType.CanShield)
            {
                return Type.ToString() + ": " + Charges + " charge" + (Charges > 1 ? "s" : "");
            }
            else if (Type == PowerUpType.SledgeHammer && !ActiveTimer.IsRunning)
            {
                return Type.ToString() + ": " + Charges + " charge" + (Charges > 1 ? "s" : "");
            }
            else
            {
                int timeRemaining = ActiveDuration - (int)ActiveTimer.ElapsedMilliseconds;
                return Type.ToString() + ": " + (timeRemaining / 1000.0).ToString("0.00");
            }
        }
        public static void RemoveAlivePowerUps()
        {
            AlivePowerUps.Clear();
        }
        public static bool IsPowerUpAlive(PowerUpType type)
        {
            foreach (PowerUp p in AlivePowerUps)
                if (p.Type == type)
                    return true;
            return false;
        }
    }

    class PowerUpType
    {
        private static Texture2D doubleShotTexture, speedBoostTexture, canShieldTexture, sledgeHammerTexture;

        private static PowerUpType doubleShot = new PowerUpType(doubleShotTexture, 0),
            speedBoost = new PowerUpType(speedBoostTexture, 0), 
            canShield = new PowerUpType(canShieldTexture, GameSettings.DEFAULTCANSHIELDCHARGES),
            sledgeHammer = new PowerUpType(sledgeHammerTexture, 1);

        private static int idCounter = 0;

        private int id, charges;
        private Texture2D texture;
        private int aliveDuration, activeDuration;

        private PowerUpType(Texture2D texture, int charges)
        {
            this.id = idCounter++;
            this.texture = texture;
            this.charges = charges;
            aliveDuration = GameSettings.DEFAULTPOWERUPALIVEDURATION;
            activeDuration = GameSettings.DEFAULTPOWERUPACTIVEDURATION;
        }

        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }
        public int AliveDuration
        {
            get
            {
                return aliveDuration;
            }
            set
            {
                aliveDuration = value;
            }
        }
        public int ActiveDuration
        {
            get
            {
                return activeDuration;
            }
            set
            {
                activeDuration = value;
            }
        }
        public int Charges
        {
            get
            {
                return charges;
            }
            set
            {
                charges = value;
            }
        }

        /*public static int Count
        {
            get
            {
                return idCounter;
            }
        }*/

        public static Texture2D DoubleShotTexture
        {
            set
            {
                doubleShotTexture = value;
                doubleShot.Texture = value;
            }
        }
        public static Texture2D SpeedBoostTexture
        {
            set
            {
                speedBoostTexture = value;
                speedBoost.Texture = value;
            }
        }
        public static Texture2D CanShieldTexture
        {
            set
            {
                canShieldTexture = value;
                canShield.Texture = value;
            }
        }
        public static Texture2D SledgeHammerTexture
        {
            set
            {
                sledgeHammerTexture = value;
                sledgeHammer.Texture = value;
            }
        }

        public static PowerUpType DoubleShot
        {
            get
            {
                return doubleShot;
            }
        }
        public static PowerUpType SpeedBoost
        {
            get
            {
                return speedBoost;
            }
        }
        public static PowerUpType CanShield
        {
            get
            {
                return canShield;
            }
        }
        public static PowerUpType SledgeHammer
        {
            get
            {
                return sledgeHammer;
            }
        }

        public override bool Equals(object o)
        {
            if (!(o is PowerUpType))
                return false;
            return id == ((PowerUpType)o).id;
        }
        public override int GetHashCode()
        {
            return this.id;
        }
        public static bool operator ==(PowerUpType d1, PowerUpType d2)
        {
            return d1.Equals(d2);
        }
        public static bool operator !=(PowerUpType d1, PowerUpType d2)
        {
            return !d1.Equals(d2);
        }

        public override string ToString()
        {
            switch (id)
            {
                case 0:
                    return "Double Shot";
                case 1:
                    return "Speed Boost";
                case 2:
                    return "Can Shield";
                case 3:
                    return "Sledgehammer";
            }
            return null;
        }
    }
}