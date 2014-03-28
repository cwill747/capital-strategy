using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer.Messaging
{
    class MessageQueue
    {
        public List<Message> incomingMessages;
        public List<Message> outgoingMessages;
        public Server server;

        public MessageQueue(Server s)
        {
            incomingMessages = new List<Message>();
            outgoingMessages = new List<Message>();
            this.server = s;
        }

        public void addToIncomingQueue(Message m)
        {
            m.processed = false;
            incomingMessages.Add(m);
        }

        public void addToOutgoingQueue(Message m)
        {
            outgoingMessages.Add(m);
        }

        public void update()
        {
            outgoingMessages.RemoveAll(x => x.waitingToSend == false);
            foreach (Message m in incomingMessages)
            {
                switch(m.type)
                {
                    case 0: // message is matchmaking
                        if(m.msg == "seeking")
                        {
                            // Change the current client's looking for game property to true
                            //m.sentFrom.lookingForGame = true;
                            // Tell the server queue that we have a new client looking for a game
                            //server.qm.newClientLookingForGame(m.sentFrom);
                            m.waitingToSend = true;
                        }
                        break;
                    case 1: // message is a chat message
                        {
                            // just turn the message around and send it to the chat recipient
                            m.waitingToSend = true;
                            addToOutgoingQueue(m); 
                        }
                        break;
                    default:
                        break;
                }
                m.processed = true;

            }
            incomingMessages.RemoveAll(x => x.processed == true);

        }

    }
}
