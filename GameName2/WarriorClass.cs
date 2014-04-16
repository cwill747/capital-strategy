using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CapitalStrategy.Windows;
using Microsoft.Xna.Framework.Audio;

namespace CapitalStrategy
{
    public class WarriorClass
    {
        public int index { get; set; }
        public string warriorClassName { get; set; }
        public int indexOfAdvantageAgainst { get; set; }

        public WarriorClass(int index, string warriorClassName, int indexOfAdvantageAgainst)
        {
            this.index = index;
            this.warriorClassName = warriorClassName;
            this.indexOfAdvantageAgainst = indexOfAdvantageAgainst;
        }
    }
}
