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
    class AnimatedObject : BaseObject
    {
        public Animation Animation;

        public AnimatedObject(Rectangle rectangle, Animation animation)
            : base(rectangle)
        {
            Animation = animation;
        }

        static public implicit operator Texture2D(AnimatedObject o)
        {
            return o.Animation;
        }
    }
}