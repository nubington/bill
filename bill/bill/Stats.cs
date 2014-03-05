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
    class Stats
    {
        public static int
            PeanutShots,
            PeanutHits,
            CarChasePeanutScore;

        public static void Clear()
        {
            PeanutShots = PeanutHits = CarChasePeanutScore = 0;
        }
    }
}
