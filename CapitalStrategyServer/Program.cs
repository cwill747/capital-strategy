using System;
using System.Threading;
using CapitalStrategyServer.Messaging;
using Lidgren.Network;
using System.Collections.Generic;

namespace CapitalStrategyServer
{
    class Program
    {




        static void Main(string[] args)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("xnaapp");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 14242;
            // create and start server
            NetServer server = new NetServer(config);
            server.Start();
            Server s = new Server();

            // schedule initial sending of position updates
            double nextSendUpdates = NetTime.Now;

            // run until escape is pressed
            while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
            {
                NetIncomingMessage msg;
                while ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryRequest:
                            //
                            // Server received a discovery request from a client; send a discovery response (with no extra data attached)
                            //
                            //server.SendDiscoveryResponse(null, msg.SenderEndpoint);
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            //
                            // Just print diagnostic messages to console
                            //
                            Console.WriteLine(msg.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            if (status == NetConnectionStatus.Connected)
                            {
                                //
                                // A new player just connected!
                                //
                                Client c = new Client(msg.SenderConnection.RemoteUniqueIdentifier);
                                s.addConnection(c);
                                Console.WriteLine(msg.SenderConnection.RemoteUniqueIdentifier + " connected!");
                            }

                            break;
                        case NetIncomingMessageType.Data:
                            //
                            // The client sent input to the server
                            //
                            Message m = new Message();
                            msg.ReadAllFields(m);
                            s.msgQueue.addToIncomingQueue(m);
                            Console.WriteLine("Received Message: " + m.ToString());
                            break;
                    }

                    //
                    // send position updates 30 times per second
                    //
                    server.Recycle(msg);

                }


                // Yes, it's time to send position updates
                foreach (Message m in s.msgQueue.outgoingMessages)
                {
                    if (m.waitingToSend == true)
                    {
                        m.waitingToSend = false;
                        NetOutgoingMessage om = server.CreateMessage();
                        Console.WriteLine("Sending Message: " + m.ToString());
                        om.WritePadBits();
                        om.WriteAllFields(m);
                        server.SendMessage(om, server.Connections.Find(nc => nc.RemoteUniqueIdentifier.Equals(m.sendToUUID)), NetDeliveryMethod.ReliableUnordered);
                    }
                    
                }
                s.msgQueue.outgoingMessages.RemoveAll(x => x.waitingToSend == false);

                s.msgQueue.update();


                // sleep to allow other processes to run smoothly
                Thread.Sleep(1);
            }

            server.Shutdown("app exiting");
        }

        
    }
}
