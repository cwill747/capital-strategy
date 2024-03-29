﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer.Messaging
{
    public enum msgType : int { Matchmaking, Chat, Move };

    /// <summary>
    /// Represents a message sent from client to client or client to server
    /// </summary>
    public class Message
    {
        public int type;
        public String msg { get; set; }
        public bool waitingToSend;
        public long sentFrom;
        public long sendToUUID;
        public bool processed;
        public int[] startLocation;
        public int[] endLocation;
        public int[] attackedLocation;
        public int damageDealt;
        public int attackedUnitID;
        public int attackerUnitID;
        public bool unitAttackDied;
        public int facing;
        public Client matchmadeClient;
        public string username;

        public Message()
        {

        }
        public Message(msgType msg)
        {
            this.type = (int) msg;
        }

        public Message(msgType msg, long sentFrom)
        {
            this.type = (int)msg;
            this.sentFrom = sentFrom;
        }

        public Message(msgType msg, long sentFrom, long sendToUUID)
        {
            this.type = (int)msg;
            this.sentFrom = sentFrom;
            this.sendToUUID = sendToUUID;
        }

        /// <summary>
        ///     Creates a move message from necessary parameters
        /// </summary>
        /// <param name="type">Type of message to send</param>
        /// <param name="sentFrom">The UID of the client the message was sent from</param>
        /// <param name="sendToUUID">The UID of the client the message was sent to</param>
        /// <param name="startLocation">Where the piece started on the board</param>
        /// <param name="endLocation">Where the piece ended on the board</param>
        /// <param name="attackedLocation">What location the piece attacked</param>
        /// <param name="damageDealt">How much damage it dealt</param>
        /// <param name="attackedUnitID">What piece the unit attacked</param>
        /// <param name="attackerUnitID">What the id of the attacking piece was</param>
        /// <param name="unitAttackDied">Whether the attacked piece died or not</param>
        public Message(msgType type, long sentFrom, long sendToUUID, int[] endLocation, int[] attackedLocation, int damageDealt,
            int attackedUnitID, int attackerUnitID, bool unitAttackDied, int facing)
        {
            this.type = (int)msgType.Move;
            this.sentFrom = sentFrom;
            this.sendToUUID = sendToUUID;
            this.endLocation = endLocation;
            this.attackedLocation = attackedLocation;
            this.damageDealt = damageDealt;
            this.attackedUnitID = attackedUnitID;
            this.attackerUnitID = attackerUnitID;
            this.unitAttackDied = unitAttackDied;
            this.facing = facing;
        }

        public override string ToString()
        {
            if (sendToUUID != null && msg != null)
            {
                return base.ToString() + ": [From]: " + sentFrom.ToString() + ", [To]: " + sendToUUID + ", [Type]: " + type + ", [msg]: " + msg.ToString();
            }
            else if (sendToUUID != null)
            {
                return base.ToString() + ": [From]: " + sentFrom.ToString() + ", [To]: " + sendToUUID + ", [Type]: " + type;
            }
            else
            {
                return base.ToString() + ": [From]: " + sentFrom.ToString() + ", [Type]: " + type + ", [msg]: " + msg.ToString();
            }

        }

        public void handleMessage(ref NetOutgoingMessage m)
        {
            m.Write(this.type);
            m.Write(this.sentFrom);
            if (sendToUUID != null)
            {
                m.Write(this.sendToUUID);
            }
            m.Write(msg);

        }


        public void handleMoveMessage(ref NetOutgoingMessage m)
        {
            m.Write(this.type);
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
            m.Write(this.facing);
        }
    }

}