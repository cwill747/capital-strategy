using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer
{
    class QueueManager
    {
        public List<Client> clientsConnected { get; set; }
        public List<Client> clientsLookingForAGame { get; set; }

        public QueueManager()
        {
            clientsConnected = new List<Client>();
        }
        public void clientConnected(long identifier)
        {
            Client c = new Client();
            c.uniqueIdentifier = identifier;
            clientsConnected.Add(c);
        }
        public void newClientLookingForGame(Client c)
        {
            c.lookingForGame = true;
            clientsConnected.ForEach(delegate(Client nc)
            {
                if(nc.lookingForGame == true)
                {

                }
            });
        }
    }
}
