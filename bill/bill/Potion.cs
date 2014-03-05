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
    class Potion : BaseObject
    {
        public static List<Potion> Potions = new List<Potion>();
        public static Texture2D RedPotionTexture;

        public static int HealAmount = 4;

        public Potion(Rectangle rectangle)
            : base(rectangle)
        {
            Texture = RedPotionTexture;
            Potions.Add(this);
        }

        public void Remove()
        {
            Potions.Remove(this);
        }
    }
}