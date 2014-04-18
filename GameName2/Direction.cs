using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapitalStrategy
{
    public class Direction
    {
        public const int N = 0;
        public const int NE = 1;
        public const int E = 2;
        public const int SE = 3;
        public const int S = 4;
        public const int SW = 5;
        public const int W = 6;
        public const int NW = 7;

        public static int flipOverX(int direction)
        {
            if (direction <= 4)
            {
                return 4 - direction;
            }
            else
            {
                return 4 + 8 - direction;
            }
            // 0 -> 4
            // 1 -> 3
            // 2 -> 2
            // 3 -> 1
            // 4 -> 0
            // 5 -> 7
            // 6 -> 6
            // 7 -> 5

        }
    }
}
