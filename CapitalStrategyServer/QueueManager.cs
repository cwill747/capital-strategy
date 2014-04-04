using CapitalStrategyServer.Messaging;
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
        private Server s;

        public QueueManager(Server s)
        {
            clientsConnected = new List<Client>();
            clientsLookingForAGame = new List<Client>();
            this.s = s;
        }
        public void clientConnected(long identifier)
        {
            Client c = new Client(identifier);
            clientsConnected.Add(c);
        }
        public void clientConnected(Client c)
        {
            clientsConnected.Add(c);
        }
        public void newClientLookingForGame(long identifier)
        {
            Client c = clientsConnected.Find(x => x.uniqueIdentifier == identifier);
            c.lookingForGame = true;
            clientsLookingForAGame.Add(c);

            Client client1;
            Client client2;

            // TODO : We can use this later for more advanced match making
            while(clientsLookingForAGame.Count >= 2) // now we have enough people looking for a game.
            {
                client1 = clientsLookingForAGame[0];
                client1.lookingForGame = false;
                client2 = clientsLookingForAGame[1];
                client2.lookingForGame = false;
                Message sendToClient1 = new Message(msgType.Matchmaking, client2.uniqueIdentifier, client1.uniqueIdentifier);
                sendToClient1.msg = client2.username;
                sendToClient1.waitingToSend = true;
                Message sendToClient2 = new Message(msgType.Matchmaking, client1.uniqueIdentifier, client2.uniqueIdentifier);
                sendToClient2.msg = client1.username;
                sendToClient2.waitingToSend = true;
                s.msgQueue.addToOutgoingQueue(sendToClient1);
                s.msgQueue.addToOutgoingQueue(sendToClient2);
                clientsLookingForAGame.RemoveAll(x => x.lookingForGame == false);
            }
        }
    }
}
