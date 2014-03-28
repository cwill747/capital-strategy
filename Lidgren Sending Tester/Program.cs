using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using CapitalStrategyServer.Messaging;
using CapitalStrategyServer;

namespace Lidgren_Sending_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("xnaapp");
            NetClient client = new NetClient(config);
            NetClient client2 = new NetClient(config);
            client.Start();
            client.Connect("127.0.0.1", 14242);

            client2.Start();
            client2.Connect("127.0.0.1", 14242);

            string line;
            Console.WriteLine("Client 1: " + client.UniqueIdentifier.ToString());
            Console.WriteLine("Client 2: " + client2.UniqueIdentifier.ToString());

            do {
                Console.WriteLine("Message type: ");
                line = Console.ReadLine();
                if (line == "chat")
                {
                    Message mTest = new Message(msgType.Chat, client.UniqueIdentifier, client2.UniqueIdentifier);
                    mTest.msg = "Test Chat";
                    mTest.sendToUUID = client2.UniqueIdentifier;
                    NetOutgoingMessage msg = client.CreateMessage();
                    msg.WritePadBits();
                    msg.WriteAllFields(mTest);
                    Console.WriteLine("Sending message: " + mTest.ToString());
                    client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
                    client.FlushSendQueue();
                }

                NetIncomingMessage inc;
                while ((inc = client.ReadMessage()) != null)
                {
                    switch (inc.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            Message m = new CapitalStrategyServer.Messaging.Message();
                            inc.ReadAllFields(m);
                            Console.WriteLine("Client 1 got message " + m.ToString());
                            break;
                    }
                }

                NetIncomingMessage inc2;
                while ((inc2 = client2.ReadMessage()) != null)
                {
                    switch (inc2.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            Message m = new CapitalStrategyServer.Messaging.Message();
                            inc2.ReadAllFields(m);
                            Console.WriteLine("Client 2 got message " + m.ToString());
                            break;
                    }
                }

            } while (line != null);



        }
    }
}
