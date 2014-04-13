using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapitalStrategy
{
    public class WarriorWrapper
    {
        public Warrior warrior { get; set; }
        public int id { get; set; }
        public WarriorWrapper(Warrior warrior, int id)
        {
            this.warrior = warrior;
            this.id = id;
        }
    }
}
