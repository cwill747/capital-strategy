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

        public QueueManager()
        {
            clientsConnected = new List<Client>();
        }
        public void clientConnected(string identifier)
        {
            Client c = new Client();
            c.uniqueIdentifier = identifier;
            clientsConnected.Add(c);
        }
    }
}
