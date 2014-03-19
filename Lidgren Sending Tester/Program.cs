using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;
using CapitalStrategyServer;

namespace Lidgren_Sending_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("xnaapp");
            NetClient client = new NetClient(config);
            client.Start();
            client.Connect("127.0.0.1", 14242);
            string line;
            do {
                CapitalStrategyServer.Message m = new CapitalStrategyServer.Message();
                Console.WriteLine("Message type: ");
                line = Console.ReadLine();
                m.msgType = line;
                Console.WriteLine("Message information: ");
                line = Console.ReadLine();
                m.msg = line;
                NetOutgoingMessage msg = client.CreateMessage();
                msg.WriteAllFields(m);
                client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
                client.FlushSendQueue();
            } while (line != null);   
         

        }
    }
}
