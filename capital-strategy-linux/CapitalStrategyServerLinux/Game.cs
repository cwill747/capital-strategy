using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer
{
    class Game
    {
        public Client player1;
        public Client player2;

        public Game(Client p1, Client p2)
        {
            this.player1 = p1;
            this.player2 = p2;
        }
    }
}
