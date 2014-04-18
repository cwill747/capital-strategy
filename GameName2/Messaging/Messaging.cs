using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CapitalStrategyServer.Messaging;
using Lidgren.Network;
using CapitalStrategyServer;

namespace CapitalStrategy.Messaging
{
    public class Messaging
    {
        private List<Message> outgoingMessages;
        private List<Message> incomingMessages;
        private NetClient client;
        private string username;
        public Game1 game;

        public Messaging(NetClient client, string username)
        {
            outgoingMessages = new List<Message>();
            incomingMessages = new List<Message>();
            this.client = client;
            this.username = username;
        }

        public void handleIncomingMessage(NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    if (status == NetConnectionStatus.Connected)
                    {
                        Console.WriteLine("Connected! UID: " + msg.SenderConnection.RemoteUniqueIdentifier + " , IP: " + msg.SenderConnection.RemoteEndPoint.ToString());
                    }
                    break;
                case NetIncomingMessageType.Data:
                    msgType type = (msgType)msg.ReadInt32();
                    Message m;
                    if (type == msgType.Chat)
                    {
                        long sentFrom = msg.ReadInt64();
                        long sendToUUID = msg.ReadInt64();
                        string message = msg.ReadString();
                        m = new Message(type, sentFrom, sendToUUID);
                        m.msg = message;

                        if (m.msg == "SERVER HELLO")
                        {
                            Message clientHello = new Message(msgType.Chat, client.UniqueIdentifier, client.ServerConnection.RemoteUniqueIdentifier);
                            clientHello.msg = "CLIENT HELLO:" + this.game.username;
                            this.addToOutgoingQueue(clientHello);
                        }
                    }
                    else if (type == msgType.Matchmaking)
                    {
                        long sentFrom = msg.ReadInt64();
                        long sentTo = msg.ReadInt64();
                        string message = msg.ReadString();
                        m = new Message(type, sentFrom, sentTo);
                        m.msg = message;
                        if (m.msg != "SEEKING")
                        {
                            // I have found a new match
                            game.otherPlayer = new Client(m.sentFrom, true, false, m.msg.Substring(0, m.msg.IndexOf(':')));
                            bool isMyTurn = Boolean.Parse(m.msg.Substring(m.msg.IndexOf(':') + 1));
                            Console.WriteLine("Opponent found: " + game.otherPlayer.username);
                            this.game.gameState = GameState.gameMatch;
                            this.game.windows[GameState.gameMatch].Initialize();
                            this.game.gameMatch.isYourTurn = isMyTurn;
                            if (isMyTurn)
                            {
                                this.game.gameMatch.yourTurnFadingMessage.show();
                            }
                            Game1.gameStates.Push(GameState.mainMenu);
                        }
                    }
                    else
                    {
                        m = new Message(
                            type,
                            msg.ReadInt64(), // UID of the client the message was sent from
                            msg.ReadInt64(), // UID of the client the message is sent to (should be us)
                            new int[2] { msg.ReadInt32(), msg.ReadInt32() }, // The end location of the piece moved
                            new int[2] { msg.ReadInt32(), msg.ReadInt32() }, // Where the piece attacked
                            msg.ReadInt32(), // The damage dealt
                            msg.ReadInt32(), //The attacked unit ID
                            msg.ReadInt32(), // The attacker unit ID
                            msg.ReadBoolean(), // Whether the unit died or not
                            msg.ReadInt32() // which way the unit is facing
                        );

                        /*
                         *             m.Write(this.type);
                                        m.Write(this.sentFrom);
                                        m.Write(this.sendToUUID);
                                        m.Write(this.endLocation[0]);
                                        m.Write(this.endLocation[1]);
                                        m.Write(this.attackedLocation[0]);
                                        m.Write(this.attackedLocation[1]);
                                        m.Write(this.damageDealt);
                                        m.Write(this.attackedUnitID);
                                        m.Write(this.attackerUnitID);
                                        m.Write(this.unitAttackDied);
                         */
                        this.game.gameMatch.waitingForTurn = false;
                        this.game.gameMatch.handleOpponentMove(m);

                    }
                    Console.WriteLine(m.ToString());

                    break;
            }
        }


        public void addToOutgoingQueue(Message m)
        {
            m.waitingToSend = true;
            this.outgoingMessages.Add(m);
        }

        public void update()
        {
            foreach (Message m in this.outgoingMessages)
            {
                if (m.waitingToSend == true)
                {
                    NetOutgoingMessage outgoingMessage = this.client.CreateMessage();
                    outgoingMessage.WritePadBits();
                    Message clientReadyForMatch = new Message(msgType.Matchmaking, this.client.UniqueIdentifier,
                        this.client.ServerConnection.RemoteUniqueIdentifier);
                    switch (m.type)
                    {
                        case (int)msgType.Chat:
                        case (int)msgType.Matchmaking:
                            m.handleMessage(ref outgoingMessage);
                            break;
                        case (int)msgType.Move:
                            m.handleMoveMessage(ref outgoingMessage);
                            break;
                    }
                    m.handleMessage(ref outgoingMessage);
                    Console.WriteLine("Sending message: " + m.ToString());
                    this.client.SendMessage(outgoingMessage, NetDeliveryMethod.ReliableUnordered);
                    m.waitingToSend = false;
                }

            }
            this.outgoingMessages.RemoveAll(x => x.waitingToSend == false);
        }

    }
}
