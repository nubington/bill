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
    class Blast : BaseObject
    {
        public static List<Blast> Blasts = new List<Blast>();

        int size;
        int growthDelay, timeSinceLastGrowth;

        public Blast(Vector2 position, int size, int growthDelay)
            : base(new Rectangle((int)position.X, (int)position.Y, 1, 1))
        {
            this.size = size;
            this.growthDelay = growthDelay;
            Blasts.Add(this);
        }

        // returns true if reached size
        public bool Update(GameTime gameTime)
        {
            timeSinceLastGrowth += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeSinceLastGrowth >= growthDelay)
            {
                timeSinceLastGrowth -= growthDelay;

                Grow(1, 1, size, size);
            }

            if (Width == size)
                return true;
            return false;
        }

        // updates blasts and removes them from list when size reached
        public static void UpdateBlasts(GameTime gameTime)
        {
            for (int i = 0; i < Blasts.Count; i++)
            {
                Blast b = Blasts[i];
                if (b.Update(gameTime))
                {
                    Blasts.Remove(b);
                    i--;
                }
            }
        }
    }
}
