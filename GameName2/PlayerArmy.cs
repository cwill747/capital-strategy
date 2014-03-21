using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapitalStrategy
{
    class PlayerArmy
    {
        private List<WarriorType> availableWarriors;
        private int orientation;

        public PlayerArmy(int o)
        {
            this.orientation = o;
            availableWarriors = new List<WarriorType>();
        }

        public void AddWarrior(WarriorType warrior)
        {
            this.availableWarriors.Add(warrior);
        }

        public void AddWarrior(List<WarriorType> warriors)
        {
            availableWarriors.AddRange(warriors);
        }
    }
}
