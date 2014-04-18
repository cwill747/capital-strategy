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
            s.netserver = server;
            // schedule initial sending of position updates
            double nextSendUpdates = NetTime.Now;

			Console.Out.WriteLine("Server stated. Waiting for connections...");
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
                                Message connectMessage = new Message(msgType.Chat, 0, msg.SenderConnection.RemoteUniqueIdentifier);
                                connectMessage.waitingToSend = true;
                                connectMessage.msg = "SERVER HELLO";
                                s.msgQueue.addToOutgoingQueue(connectMessage);
                            }

                            break;
                        case NetIncomingMessageType.Data:
                            //
                            // The client sent input to the server
                            //
                            msgType type = (msgType) msg.ReadInt32();
                            Message m;
                            if (type == msgType.Chat)
                            {
                                long sentFrom = msg.ReadInt64();
                                long sendToUUID = msg.ReadInt64();
                                string message = msg.ReadString();
                                m = new Message(type, sentFrom, sendToUUID);
                                m.msg = message;
                            }
                            else if (type == msgType.Matchmaking)
                            {
                                long sentFrom = msg.ReadInt64();
                                long sendToUUID = msg.ReadInt64();
                                string[] arr = msg.ReadString().Split(new char[] {':'});
                                string message = arr[0];
                                m = new Message(type, sentFrom, sendToUUID);
                                if (arr.Length > 1)
                                {
                                    m.username = arr[1];
                                }
                                m.msg = message;
                            }
                            else
                            {
                                m = new Message(
                                    type,
                                    msg.ReadInt64(),
                                    msg.ReadInt64(),
                                    new int[2] { msg.ReadInt32(), msg.ReadInt32() },
                                    new int[2] { msg.ReadInt32(), msg.ReadInt32() },
                                    msg.ReadInt32(),
                                    msg.ReadInt32(),
                                    msg.ReadInt32(),
                                    msg.ReadBoolean(),
                                    msg.ReadInt32()
                                );
                            }
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
                        switch (m.type)
                        {
                            case (int) msgType.Chat:
                            case (int) msgType.Matchmaking:
                                m.handleMessage(ref om);
                                break;
                            case (int) msgType.Move:
                                m.handleMoveMessage(ref om);
                                break;
                        }
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
