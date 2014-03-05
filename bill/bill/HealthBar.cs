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
    class HealthBar
    {
        private static Texture2D texture;

        private Rectangle rectangle;

        public HealthBar(Rectangle rectangle)
        {
            this.rectangle = rectangle;
        }

        public static Texture2D Texture
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
        public Rectangle Rectangle
        {
            get
            {
                return rectangle;
            }
            set
            {
                rectangle = value;
            }
        }
        public int Width
        {
            get
            {
                return rectangle.Width;
            }
            set
            {
                rectangle.Width = value;
            }
        }
        public int Height
        {
            get
            {
                return rectangle.Height;
            }
            set
            {
                rectangle.Height = value;
            }
        }
    }
}
