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
    abstract class Alex : BaseObject
    {
        private int hp, maxHp;

        public Alex(Rectangle rectangle)
            : base(rectangle)
        {
        }

        public virtual void Update(GameTime gameTime, Character isaac)
        {
        }

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
    }

    class AlexEnchanter : Alex
    {
        static readonly int DEFAULTMAXHP = 350;
        static readonly int HEIGHT = 100, WIDTH = (int)(HEIGHT * 1.07169811f);
        private int laserSize = 6, laserLength = 100, laserDelay = 1000, timeSinceLastLaser;
        private float laserSpeed = 500;
        private int fireballSize = 25, fireballDelay = 500, fireballSpeed = 250, timeSinceLastFireball;
        private Vector2 fireballPointAdjustment = new Vector2(0, 5);

        static readonly float warpAnimationDuration = .75f, warpDelay = 5, castAnimationDuration = .75f;
        static readonly int warpShrinkAmountX = 0, warpShrinkAmountY = 0;
        public static Texture2D[] WarpTextures = new Texture2D[10];
        public static Texture2D[] CastTextures = new Texture2D[10];
        public static Texture2D[] PeanutShieldTextures = new Texture2D[4];
        public static Texture2D MagicStaffTexture;
        private Animation warpOutAnimation, warpInAnimation, castAnimation;
        public AnimatedObject PeanutShield;
        private float timeSinceLastWarp;
        private bool warpingOut = false, warpingIn = false;
        private Vector2[] warpLocations = new Vector2[4];

        private bool usingStaff, staffIsTurning;
        private BaseObject magicStaff;
        static readonly int magicStaffWidth = 60, magicStaffHeight = 25;
        private float magicStaffRotationSpeed = 10f;
        private Vector2 magicStaffPointAdjustment = new Vector2(0, HEIGHT * .4f);

        static readonly float spellDelay = 3;
        private float timeSinceLastSpell;

        private bool castingMice, miceCasted;
        private float miceDelayTime;
        static readonly float miceSpeed = 300, micePreDelay = .5f, micePostDelay = .5f;
        static readonly int miceSize = 15, cordSize = 2, miceLength = 25;

        private bool castingCircularLaser;
        private float circularLaserLeftAngle = -90f, circularLaserRightAngle = -90f;
        private float circularLaserRotation = 600, circularLaserDelay = .11f;
        private float timeSinceLastCircularLaserShot;

        private bool castingSuperFireball;
        public static readonly int SUPERFIREBALLSIZE = 150, SUPERFIREBALLSPEED = 200;
        public static SoundEffect SuperFireballEnterSound;

        private bool castingPeanutShield, hasPeanutShield;

        public AlexEnchanter(Rectangle rectangle)
            : base(rectangle)
        {
            speed = new Vector2(500, 500);
            warpOutAnimation = new Animation(warpAnimationDuration, WarpTextures);
            warpInAnimation = new Animation(warpAnimationDuration, WarpTextures[9], WarpTextures[8],
                WarpTextures[7], WarpTextures[6], WarpTextures[5], WarpTextures[4], WarpTextures[3],
                WarpTextures[2], WarpTextures[1], WarpTextures[0]);
            castAnimation = new Animation(castAnimationDuration, CastTextures[0],  CastTextures[1], 
                CastTextures[2], CastTextures[3], CastTextures[4], CastTextures[5], CastTextures[6],
                CastTextures[7], CastTextures[8], CastTextures[9], CastTextures[8], CastTextures[7],
                CastTextures[6], CastTextures[5], CastTextures[4], CastTextures[3], CastTextures[2], 
                CastTextures[1], CastTextures[0]);
            PeanutShield = new AnimatedObject(new Rectangle(0, 0, 0, 0), new Animation(-1, 45, PeanutShieldTextures[0], PeanutShieldTextures[1],
                PeanutShieldTextures[2], PeanutShieldTextures[3], PeanutShieldTextures[2], 
                PeanutShieldTextures[1], PeanutShieldTextures[0]));
            createWarpLocations();
            magicStaff = new BaseObject(new Rectangle(0, 0, magicStaffWidth, magicStaffHeight));
            magicStaff.Texture = MagicStaffTexture;

            HP = MaxHP = DEFAULTMAXHP;
        }
        public AlexEnchanter()
            : this(new Rectangle(0, 0, WIDTH, HEIGHT))
        { }

        public override void Update(GameTime gameTime, Character isaac)
        {
            if (!IsCasting)
                checkForWarp(gameTime);

            if (!IsWarping)
            {
                checkForSpells(gameTime, isaac);
                if (!IsCasting)
                {
                    checkForShootLaser(gameTime, isaac);
                    checkForShootFireball(gameTime, isaac);
                }
            }

            if (castingPeanutShield || HasPeanutShield)
            {
                PeanutShield.CenterPoint = CenterPoint;
                PeanutShield.Animation.Update();
            }
        }

        // warp stuff
        void checkForWarp(GameTime gameTime)
        {
            if (!IsWarping)
                timeSinceLastWarp += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastWarp >= warpDelay)
            {
                warpOutAnimation.Start();
                warpingOut = true;
                timeSinceLastWarp = 0;
            }

            if (warpingOut)
            {
                Shrink(warpShrinkAmountX, warpShrinkAmountY);
                warpOutAnimation.Update();
                if (!warpOutAnimation.IsRunning)
                {
                    randomWarp();
                    warpInAnimation.Start();
                    warpingOut = false;
                    warpingIn = true;
                }
            }
            if (warpingIn)
            {
                Grow(warpShrinkAmountX, warpShrinkAmountY, WIDTH, HEIGHT);
                warpInAnimation.Update();
                if (!warpInAnimation.IsRunning)
                {
                    Width = WIDTH;
                    Height = HEIGHT;
                    warpingIn = false;
                }
            }
        }
        void createWarpLocations()
        {
            warpLocations[0] = new Vector2(graphics.GraphicsDevice.Viewport.Width * .25f, graphics.GraphicsDevice.Viewport.Height * .25f);
            warpLocations[1] = new Vector2(graphics.GraphicsDevice.Viewport.Width * .75f, graphics.GraphicsDevice.Viewport.Height * .25f);
            warpLocations[2] = new Vector2(graphics.GraphicsDevice.Viewport.Width * .25f, graphics.GraphicsDevice.Viewport.Height * .75f);
            warpLocations[3] = new Vector2(graphics.GraphicsDevice.Viewport.Width * .75f, graphics.GraphicsDevice.Viewport.Height * .75f);
        }
        void randomWarp()
        {
            Vector2 oldLocation = this.CenterPoint;
            while (this.CenterPoint == oldLocation)
                this.CenterPoint = warpLocations[rand.Next(warpLocations.Length)];
        }

        // spells
        void checkForSpells(GameTime gameTime, Character isaac)
        {
            if (!IsCasting)
                timeSinceLastSpell += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastSpell >= spellDelay)
            {
                int n;
                do
                {
                    n = rand.Next(4);

                    float hpPercent = HP / (float)MaxHP;

                    if (n == 1 && hpPercent > .9f)
                        continue;

                    if (n == 2 && hpPercent > .75f)
                        continue;

                    if (n == 3 && hpPercent > .5f)
                        continue;
                    if (hasPeanutShield && n == 3)
                        continue;

                    break;
                }
                while (true);
                //int n = 3;
                if (n == 0)
                {
                    castingMice = true;
                    startUsingStaff();
                }
                else if (n == 1)
                {
                    castingSuperFireball = true;
                    castAnimation.Start();
                }
                else if (n == 2)
                {
                    castingCircularLaser = true;
                    timeSinceLastCircularLaserShot = circularLaserDelay;
                }
                else if (n == 3)
                {
                    PeanutShield.Width = PeanutShield.Height = 0;
                    castingPeanutShield = true;
                    castAnimation.Start();
                }
                timeSinceLastSpell = 0;
            }

            if (castingMice)
                castMice(gameTime, isaac);
            else if (castingCircularLaser)
                castCircularLaser(gameTime, isaac);
            else if (castingSuperFireball)
                castSuperFireball(gameTime, isaac);
            else if (castingPeanutShield)
                castPeanutShield(gameTime);
        }
        void castMice(GameTime gameTime, Character isaac)
        {
            if (staffIsTurning)
            {
                updateStaff(gameTime, isaac);
                return;
            }

            miceDelayTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!miceCasted)
            {
                if (miceDelayTime >= micePreDelay)
                {
                    shootMice(isaac);

                    miceCasted = true;
                    miceDelayTime = 0;
                }
            }
            else
            {
                if (miceDelayTime >= micePostDelay)
                {
                    castingMice = false;
                    timeSinceLastLaser = 0;
                    usingStaff = false;
                    miceCasted = false;
                    miceDelayTime = 0;
                }
            }
        }
        void castCircularLaser(GameTime gameTime, Character isaac)
        {
            if (circularLaserLeftAngle <= -270.5f)
            {
                circularLaserLeftAngle = circularLaserRightAngle = -90f;
                castingCircularLaser = false;
                timeSinceLastLaser = 0;
                return;
            }

            timeSinceLastCircularLaserShot += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastCircularLaserShot >= circularLaserDelay)
            {
                timeSinceLastCircularLaserShot = 0;
                float oldSpeed = laserSpeed;
                laserSpeed = 250;

                shootLaserLeft(circularLaserLeftAngle);
                shootLaserRight(circularLaserRightAngle);

                laserSpeed = oldSpeed;

                circularLaserLeftAngle -= Util.ScaleWithGameTime(circularLaserRotation, gameTime);
                circularLaserRightAngle += Util.ScaleWithGameTime(circularLaserRotation, gameTime);
            }
        }
        void castSuperFireball(GameTime gameTime, Character isaac)
        {
            castAnimation.Update();
            if (!castAnimation.IsRunning)
            {
                Vector2 position = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2 - SUPERFIREBALLSIZE / 2, -SUPERFIREBALLSIZE);
                float angle = (float)Math.Atan2((double)(isaac.CenterPoint.Y - position.Y), (double)(isaac.CenterPoint.X - position.X));

                float moveX = SUPERFIREBALLSPEED * (float)Math.Cos(angle);
                float moveY = SUPERFIREBALLSPEED * (float)Math.Sin(angle);

                new Fireball(position, new Vector2(moveX, moveY), SUPERFIREBALLSIZE);

                Game1.Game.SoundEffectManager.Play(SuperFireballEnterSound, .15f);

                castingSuperFireball = false;
            }
        }
        void castPeanutShield(GameTime gameTime)
        {
            castAnimation.Update();
            if (!castAnimation.IsRunning)
            {
                PeanutShield.Animation.Start();
                //castingPeanutShield = false;
            }
            if (PeanutShield.Animation.IsRunning)
            {
                if (PeanutShield.Width == Width && PeanutShield.Height == Height)
                {
                    castingPeanutShield = false;
                    hasPeanutShield = true;
                    return;
                }
                PeanutShield.Width = (int)MathHelper.Min(PeanutShield.Width + 7, Width);
                PeanutShield.Height = (int)MathHelper.Min(PeanutShield.Height + 7, Height);
            }
        }

        void shootMice(Character isaac)
        {
            Vector2 mousePoint = MagicStaffPoint + new Vector2(magicStaff.Width * (float)Math.Cos(magicStaff.Rotation), magicStaff.Width * (float)Math.Sin(magicStaff.Rotation));
            BaseObject target = new BaseObject(new Rectangle((int)isaac.CenterPoint.X, (int)isaac.CenterPoint.Y, 1, 1));

            float beginAngle = magicStaff.Rotation - (float)Math.PI / 2;
            float endAngle = magicStaff.Rotation + (float)Math.PI / 2;

            int numberOfMice = 10;
            //float rotation = (float)Math.PI / numberOfMice;
            float rotation = MathHelper.TwoPi / numberOfMice;

            for (int i = 0; i < numberOfMice; i++, beginAngle += rotation)
            {
                //MouseSeeker m = new MouseSeeker(mousePoint, 300, 15, 2, 25, isaac, 2);
                MouseSeeker m = new MouseSeeker(mousePoint, miceSpeed, miceSize, cordSize, miceLength, magicStaff.Rotation, 5);
                m.Angle = beginAngle;
            }
        }
        void startUsingStaff()
        {
            magicStaff.PrecisePosition = MagicStaffPoint;
            magicStaff.Rotation = -(float)Math.PI / 2;
            usingStaff = true;
            staffIsTurning = true;
        }
        void updateStaff(GameTime gameTime, Character isaac)
        {
            if (staffIsTurning)
            {
                float targetAngle = (float)Math.Atan2(MagicStaffPoint.Y - isaac.CenterPoint.Y, MagicStaffPoint.X - isaac.CenterPoint.X) - (float)Math.PI;

                float targetX = (float)Math.Cos(targetAngle);
                float targetY = (float)Math.Sin(targetAngle);

                Vector3 oldAngleVector = new Vector3((float)Math.Cos(magicStaff.Rotation), (float)Math.Sin(magicStaff.Rotation), 0);
                Vector3 newAngleVector = new Vector3(targetX, targetY, 0);

                Vector3 crossProduct = Vector3.Cross(oldAngleVector, newAngleVector);

                if (crossProduct.Z > 0)
                    magicStaff.Rotation += Util.ScaleWithGameTime(magicStaffRotationSpeed, gameTime);
                else if (crossProduct.Z < 0)
                    magicStaff.Rotation -= Util.ScaleWithGameTime(magicStaffRotationSpeed, gameTime);
                
                if (Util.AngleDifference(magicStaff.Rotation, targetAngle) < Util.ScaleWithGameTime(magicStaffRotationSpeed, gameTime))
                {
                    magicStaff.Rotation = targetAngle;
                    staffIsTurning = false;
                    //timeSinceStaffStoppedTurning = 0;
                }
            }
        }

        // laser stuff
        void checkForShootLaser(GameTime gameTime, Character isaac)
        {
            timeSinceLastLaser += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastLaser >= laserDelay)
            {
                shootLaserBoth(isaac.CenterPoint);
                timeSinceLastLaser = 0;
            }
        }
        void shootLaserLeft(Vector2 target)
        {
            //shootLaser(this.CenterPoint - new Vector2(10, 20), target);
            shootLaser(this.CenterPoint - new Vector2(7, 17), target);
        }
        void shootLaserRight(Vector2 target)
        {
            //shootLaser(this.CenterPoint - new Vector2(-5, 20), target);
            shootLaser(this.CenterPoint - new Vector2(-9, 17), target);
        }
        void shootLaserBoth(Vector2 target)
        {
            shootLaserLeft(target);
            shootLaserRight(target);
        }
        void shootLaserLeft(float angle)
        {
            //shootLaser(this.CenterPoint - new Vector2(10, 20), angle);
            shootLaser(this.CenterPoint - new Vector2(7, 17), angle);
        }
        void shootLaserRight(float angle)
        {
            //shootLaser(this.CenterPoint - new Vector2(-5, 20), angle);
            shootLaser(this.CenterPoint - new Vector2(-9, 17), angle);
        }
        void shootLaserBoth(float angle)
        {
            shootLaserLeft(angle);
            shootLaserRight(angle);
        }
        void shootLaser(Vector2 position, Vector2 target)
        {
            float angle = (float)Math.Atan2((double)(target.Y - CenterPoint.Y), (double)(target.X - CenterPoint.X));

            /*float moveX = laserSpeed * (float)Math.Cos(angle);
            float moveY = laserSpeed * (float)Math.Sin(angle);

            new Laser(position, new Vector2(moveX, moveY), laserSize, laserLength);*/

            new Laser2(position, laserSpeed, angle, laserSize, laserLength);
        }
        void shootLaser(Vector2 position, float angle)
        {
            angle = MathHelper.ToRadians(angle);

            /*float moveX = laserSpeed * (float)Math.Cos(angle);
            float moveY = laserSpeed * (float)Math.Sin(angle);

            new Laser(position, new Vector2(moveX, moveY), laserSize, laserLength);*/

            new Laser2(position, laserSpeed, angle, laserSize, laserLength);
        }

        // fireball stuff
        void checkForShootFireball(GameTime gameTime, Character isaac)
        {
            timeSinceLastFireball += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastFireball >= fireballDelay)
            {
                //shootFireball(isaac.CenterPoint);
                shootFireballPredict(isaac);
                timeSinceLastFireball = 0;
            }
        }
        void shootFireball(Vector2 target)
        {
            float angle = (float)Math.Atan2((double)(target.Y - CenterPoint.Y), (double)(target.X - CenterPoint.X));

            float moveX = fireballSpeed * (float)Math.Cos(angle);
            float moveY = fireballSpeed * (float)Math.Sin(angle);

            new Fireball(FireballPoint - new Vector2(fireballSize / 2, fireballSize / 2), new Vector2(moveX, moveY), fireballSize);
        }
        void shootFireballPredict(Character isaac)
        {
            if (!isaac.IsMoving)
            {
                shootFireball(isaac.CenterPoint);
                return;
            }

            float isaacMoveAngle = (float)Math.Atan2(isaac.LastMove.Y, isaac.LastMove.X);

            /*float distance = Vector2.Distance(FireballPoint, isaac.CenterPoint + new Vector2(isaac.speed.X * (float)Math.Cos(isaacMoveAngle), isaac.speed.Y * (float)Math.Sin(isaacMoveAngle)));
            float distanceToSpeedRatio = distance / fireballSpeed;

            Vector2 predictedPoint = isaac.CenterPoint + new Vector2(isaac.speed.X * distanceToSpeedRatio * (float)Math.Cos(isaacMoveAngle), isaac.speed.Y * distanceToSpeedRatio * (float)Math.Sin(isaacMoveAngle));
            */

            //Vector2 predictedPoint = isaac.CenterPoint + new Vector2((float)Math.Cos(isaacMoveAngle), (float)Math.Sin(isaacMoveAngle));
           
            Vector2 fireballPoint = FireballPoint;
            Vector2 predictedPoint = isaac.CenterPoint;
            float fireballDistance, isaacDistance, fireballRatio, isaacRatio;

            do
            {
                fireballDistance = Vector2.Distance(fireballPoint, predictedPoint);
                isaacDistance = Vector2.Distance(isaac.CenterPoint, predictedPoint);
                fireballRatio = fireballDistance / fireballSpeed;
                isaacRatio = isaacDistance / isaac.speed.X;

                predictedPoint += new Vector2((float)Math.Cos(isaacMoveAngle), (float)Math.Sin(isaacMoveAngle));
            }
            //while (fireballRatio - isaacRatio > .001f);
            while (fireballRatio - isaacRatio > .5f);

            shootFireball(predictedPoint);
            //shootFireball(predictedPoint - new Vector2(isaacDistance * (float)Math.Cos(isaacMoveAngle) / 2, isaacDistance * (float)Math.Sin(isaacMoveAngle) / 2));
        }

        public void BreakPeanutShield()
        {
            hasPeanutShield = false;
            PeanutShield.Animation.Stop();
        }

        // properties
        public int LaserSize
        {
            get
            {
                return laserSize;
            }
            set
            {
                laserSize = value;
            }
        }
        public float LaserSpeed
        {
            get
            {
                return laserSpeed;
            }
            set
            {
                laserSpeed = value;
            }
        }
        public int LaserDuration
        {
            get
            {
                return laserLength;
            }
            set
            {
                laserLength = value;
            }
        }
        public int LaserDelay
        {
            get
            {
                return laserDelay;
            }
            set
            {
                laserDelay = value;
            }
        }
        public Vector2 FireballPoint
        {
            get
            {
                return CenterPoint + fireballPointAdjustment;
            }
        }
        public Vector2 MagicStaffPoint
        {
            get
            {
                return CenterPoint + magicStaffPointAdjustment;
            }
        }
        public BaseObject MagicStaff
        {
            get
            {
                return magicStaff;
            }
        }
        public override Texture2D Texture
        {
            get
            {
                if (warpingOut)
                    return warpOutAnimation;
                if (warpingIn)
                    return warpInAnimation;
                if (castAnimation.IsRunning)
                    return castAnimation;
                return base.Texture;
            }
            set
            {
                base.Texture = value;
            }
        }
        public Animation WarpOutAnimation
        {
            get
            {
                return warpOutAnimation;
            }
            set
            {
                warpOutAnimation = value;
            }
        }
        public Animation WarpInAnimation
        {
            get
            {
                return warpInAnimation;
            }
            set
            {
                warpInAnimation = value;
            }
        }
        public bool IsWarping
        {
            get
            {
                return (warpingOut || warpingIn);
            }
        }
        public bool IsCasting
        {
            get
            {
                return (castingMice || castingCircularLaser || castingSuperFireball || castingPeanutShield);
            }
        }
        public bool IsUsingStaff
        {
            get
            {
                return usingStaff;
            }
        }
        public bool IsCastingPeanutShield
        {
            get
            {
                return castingPeanutShield;
            }
        }
        public bool HasPeanutShield
        {
            get
            {
                return hasPeanutShield;
            }
        }
    }
}