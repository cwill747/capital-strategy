using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameName2
{
    /// <summary>
    /// This class is for finding the correct index in ImageAtlas[] states field for a Warrior
    /// </summary>
    class State
    {
        public const int stopped = 0;
        public const int walking = 1;
        public const int running = 2;
        public const int attack = 3;
        public const int beenHit = 4;
        public const int tippingOver = 5;
        public const int talking = 6;

    }
}
