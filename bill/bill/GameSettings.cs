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
    class GameSettings
    {
        // defaults
        public const bool
            DEFAULTCANSBLOCKPEANUTS = true,
            DEFAULTPROJECTILEROTATIONCOLLISION = true;
        public const int
            DEFAULTISAACSPEED = 180,
            DEFAULTBILLSPEED = 500,
            DEFAULTPOWERUPSPAWNDELAY = 7500,
            DEFAULTPOWERUPALIVEDURATION = 20000,
            DEFAULTPOWERUPACTIVEDURATION = 5000,
            DEFAULTSLEDGEHAMMERACTIVEDURATION = 1000,
            DEFAULTCANSHIELDACTIVEDURATION = -1,
            DEFAULTCANSHIELDCHARGES = 2,
            DEFAULTPEANUTDAMAGE = 1,
            DEFAULTPEANUTSPEED = 450,
            DEFAULTCANSPEED = 260,
            DEFAULTBILLHP = 100,
            DEFAULTBILLLIFESTEALAMOUNT = 5;
        public const decimal
            DEFAULTISAACSHOOTSPEED = 1.0m,
            DEFAULTBILLSHOOTSPEED = 1.0m,
            DEFAULTBILLSPRAYFIRESPEED = 1.0m,
            DEFAULTSPEEDBOOSTMODIFIER = 1.5m,
            DEFAULTSPAWNSAFETYFACTOR = 4.0m;

        // min and max values
        public const int 
            ISAACSPEEDMIN = 80, ISAACSPEEDMAX = 3000,
            BILLSPEEDMIN = 80, BILLSPEEDMAX = 3000,
            POWERUPSPAWNDELAYMIN = 0, POWERUPSPAWNDELAYMAX = int.MaxValue,
            POWERUPALIVEDURATIONMIN = 0, POWERUPALIVEDURATIONMAX = int.MaxValue,
            POWERUPACTIVEDURATIONMIN = 0, POWERUPACTIVEDURATIONMAX = int.MaxValue,
            CANSHIELDCHARGESMIN = 1, CANSHIELDCHARGESMAX = 100,
            PEANUTDAMAGEMIN = 1, PEANUTDAMAGEMAX = int.MaxValue,
            PEANUTSPEEDMIN = 80, PEANUTSPEEDMAX = 3000,
            CANSPEEDMIN = 100, CANSPEEDMAX = 3000,
            BILLHPMIN = 1, BILLHPMAX = int.MaxValue,
            BILLLIFESTEALAMOUNTMIN = 0, BILLLIFESTEALAMOUNTMAX = int.MaxValue;
        public const decimal
            ISAACSHOOTSPEEDMIN = .1m, ISAACSHOOTSPEEDMAX = 100m,
            BILLSHOOTSPEEDMIN = .1m, BILLSHOOTSPEEDMAX = 100m,
            BILLSPRAYFIRESPEEDMIN = .1m, BILLSPRAYFIRESPEEDMAX = 100m,
            SPEEDBOOSTMODIFIERMIN = 1.1m, SPEEDBOOSTMODIFIERMAX = 10m,
            SPAWNSAFETYFACTORMIN = 0m, SPAWNSAFETYFACTORMAX = 20m;

        // increment values
        public const int
            SPEEDINCREMENT = 5,
            TIMEINCREMENT = 100;

        // values
        private static bool
            cansBlockPeanuts = DEFAULTCANSBLOCKPEANUTS,
            projectileRotationCollision = DEFAULTPROJECTILEROTATIONCOLLISION;
        private static int 
            isaacSpeed = DEFAULTISAACSPEED,
            billSpeed = DEFAULTBILLSPEED,
            powerUpSpawnDelay = DEFAULTPOWERUPSPAWNDELAY,
            powerUpAliveDuration = DEFAULTPOWERUPALIVEDURATION,
            powerUpActiveDuration = DEFAULTPOWERUPACTIVEDURATION,
            canShieldCharges = DEFAULTCANSHIELDCHARGES,
            peanutDamage = DEFAULTPEANUTDAMAGE,
            peanutSpeed = DEFAULTPEANUTSPEED,
            canSpeed = DEFAULTCANSPEED,
            billHP = DEFAULTBILLHP,
            billLifeStealAmount = DEFAULTBILLLIFESTEALAMOUNT;
        private static decimal
            isaacShootSpeed = DEFAULTISAACSHOOTSPEED,
            billShootSpeed = DEFAULTBILLSHOOTSPEED,
            billSprayFireSpeed = DEFAULTBILLSPRAYFIRESPEED,
            speedBoostModifier = DEFAULTSPEEDBOOSTMODIFIER,
            spawnSafetyFactor = DEFAULTSPAWNSAFETYFACTOR;

        // value properties
        public static bool CansBlockPeanuts
        {
            get
            {
                return cansBlockPeanuts;
            }
            set
            {
                cansBlockPeanuts = value;
            }
        }
        public static bool ProjectileRotationCollision
        {
            get
            {
                return projectileRotationCollision;
            }
            set
            {
                projectileRotationCollision = value;
            }
        }

        public static int IsaacSpeed
        {
            get
            {
                return isaacSpeed;
            }
            set
            {
                isaacSpeed = (int)MathHelper.Clamp(value, ISAACSPEEDMIN, ISAACSPEEDMAX);
            }
        }
        public static int BillSpeed
        {
            get
            {
                return billSpeed;
            }
            set
            {
                billSpeed = (int)MathHelper.Clamp(value, BILLSPEEDMIN, BILLSPEEDMAX);
            }
        }

        public static decimal IsaacShootSpeed
        {
            get
            {
                return isaacShootSpeed;
            }
            set
            {
                isaacShootSpeed = Util.DecimalClamp(value, ISAACSHOOTSPEEDMIN, ISAACSHOOTSPEEDMAX);
            }
        }
        public static decimal BillShootSpeed
        {
            get
            {
                return billShootSpeed;
            }
            set
            {
                billShootSpeed = Util.DecimalClamp(value, BILLSHOOTSPEEDMIN, BILLSHOOTSPEEDMAX);
            }
        }
        public static decimal BillSprayFireSpeed
        {
            get
            {
                return billSprayFireSpeed;
            }
            set
            {
                billSprayFireSpeed = Util.DecimalClamp(value, BILLSPRAYFIRESPEEDMIN, BILLSPRAYFIRESPEEDMAX);
            }
        }

        public static int PowerUpSpawnDelay
        {
            get
            {
                return powerUpSpawnDelay;
            }
            set
            {
                powerUpSpawnDelay = (int)MathHelper.Clamp(value, POWERUPSPAWNDELAYMIN, POWERUPSPAWNDELAYMAX);
            }
        }
        public static int PowerUpAliveDuration
        {
            get
            {
                return powerUpAliveDuration;
            }
            set
            {
                powerUpAliveDuration = SpeedBoostAliveDuration = DoubleShotAliveDuration = CanShieldAliveDuration = SledgeHammerAliveDuration = (int)MathHelper.Clamp(value, POWERUPALIVEDURATIONMIN, POWERUPALIVEDURATIONMAX);
            }
        }
        public static int PowerUpActiveDuration
        {
            get
            {
                return powerUpActiveDuration;
            }
            set
            {
                powerUpActiveDuration = SpeedBoostActiveDuration = DoubleShotActiveDuration = (int)MathHelper.Clamp(value, POWERUPACTIVEDURATIONMIN, POWERUPACTIVEDURATIONMAX);
            }
        }

        public static decimal SpeedBoostModifier
        {
            get
            {
                return speedBoostModifier;
            }
            set
            {
                speedBoostModifier = Util.DecimalClamp(value, SPEEDBOOSTMODIFIERMIN, SPEEDBOOSTMODIFIERMAX);
            }
        }
        public static int SpeedBoostActiveDuration
        {
            get
            {
                return PowerUpType.SpeedBoost.ActiveDuration;
            }
            set
            {
                PowerUpType.SpeedBoost.ActiveDuration = value;
            }
        }
        public static int SpeedBoostAliveDuration
        {
            get
            {
                return PowerUpType.SpeedBoost.AliveDuration;
            }
            set
            {
                PowerUpType.SpeedBoost.AliveDuration = value;
            }
        }

        public static int DoubleShotActiveDuration
        {
            get
            {
                return PowerUpType.DoubleShot.ActiveDuration;
            }
            set
            {
                PowerUpType.DoubleShot.ActiveDuration = value;
            }
        }
        public static int DoubleShotAliveDuration
        {
            get
            {
                return PowerUpType.DoubleShot.AliveDuration;
            }
            set
            {
                PowerUpType.DoubleShot.AliveDuration = value;
            }
        }

        public static int CanShieldActiveDuration
        {
            get
            {
                return PowerUpType.CanShield.ActiveDuration;
            }
            set
            {
                PowerUpType.CanShield.ActiveDuration = value;
            }
        }
        public static int CanShieldAliveDuration
        {
            get
            {
                return PowerUpType.CanShield.AliveDuration;
            }
            set
            {
                PowerUpType.CanShield.AliveDuration = value;
            }
        }
        public static int CanShieldCharges
        {
            get
            {
                return canShieldCharges;
            }
            set
            {
                canShieldCharges = PowerUpType.CanShield.Charges = (int)MathHelper.Clamp(value, CANSHIELDCHARGESMIN, CANSHIELDCHARGESMAX);
            }
        }

        public static int SledgeHammerActiveDuration
        {
            get
            {
                return PowerUpType.SledgeHammer.ActiveDuration;
            }
            set
            {
                PowerUpType.SledgeHammer.ActiveDuration = value;
            }
        }
        public static int SledgeHammerAliveDuration
        {
            get
            {
                return PowerUpType.SledgeHammer.AliveDuration;
            }
            set
            {
                PowerUpType.SledgeHammer.AliveDuration = value;
            }
        }

        public static decimal SpawnSafetyFactor
        {
            get
            {
                return spawnSafetyFactor;
            }
            set
            {
                spawnSafetyFactor = Util.DecimalClamp(value, SPAWNSAFETYFACTORMIN, SPAWNSAFETYFACTORMAX);
            }
        }

        public static int PeanutDamage
        {
            get
            {
                return peanutDamage;
            }
            set
            {
                peanutDamage = (int)MathHelper.Clamp(value, PEANUTDAMAGEMIN, PEANUTDAMAGEMAX);
            }
        }
        public static int PeanutSpeed
        {
            get
            {
                return peanutSpeed;
            }
            set
            {
                peanutSpeed = (int)MathHelper.Clamp(value, PEANUTSPEEDMIN, PEANUTSPEEDMAX);
            }
        }
        public static int CanSpeed
        {
            get
            {
                return canSpeed;
            }
            set
            {
                canSpeed = (int)MathHelper.Clamp(value, CANSPEEDMIN, CANSPEEDMAX);
            }

        }

        public static int BillHP
        {
            get
            {
                return billHP;
            }
            set
            {
                billHP = (int)MathHelper.Clamp(value, BILLHPMIN, BILLHPMAX);
            }
        }
        public static int BillLifeStealAmount
        {
            get
            {
                return billLifeStealAmount;
            }
            set
            {
                billLifeStealAmount = (int)MathHelper.Clamp(value, BILLLIFESTEALAMOUNTMIN, BILLLIFESTEALAMOUNTMAX);
            }
        }
    }
}