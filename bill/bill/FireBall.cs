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
    class Fireball : BaseObject
    {
        public static List<Fireball> Fireballs = new List<Fireball>();
        public static Texture2D FireballTexture;

        float radius;

        public Fireball(Vector2 position, Vector2 speed, int size)
            : base(new Rectangle((int)position.X, (int)position.Y, size, size), speed)
        {
            radius = size / 2;
            this.Texture = FireballTexture;
            Fireballs.Add(this);
        }

        public override bool Intersects(BaseObject o)
        {
            float angle = (float)Math.Atan2(o.CenterPoint.Y - centerPoint.Y, o.CenterPoint.X - centerPoint.X);
            Vector2 point = centerPoint + new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));
            return o.Touches(point);
        }

        public static void moveFireballs(GameTime gameTime)
        {
            for (int i = 0; i < Fireball.Fireballs.Count; )
            {
                Fireball b = Fireball.Fireballs[i];

                b.movePrecise(b.speed, gameTime, false);

                if (b.Y < b.minY - b.Height || b.Y > b.maxY + b.Height ||
                    b.X < b.minX - b.Width || b.X > b.maxX + b.Width)
                    Fireball.Fireballs.Remove(b);
                else
                    i++;
            }
        }
    }
}