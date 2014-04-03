using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer.Messaging
{
    public enum msgType : int { Matchmaking, Chat, Move };

    public class Message
    {
        public int type;
        public String msg { get; set; }
        public bool waitingToSend;
        public long sentFrom;
        public long sendToUUID;
        public bool processed;
        private int[] startLocation;
        private int[] endLocation;
        private int[] attackedLocation;
        private int damageDealt;
        private int attackedUnitID;
        private int attackerUnitID;
        private bool unitAttackDied;
        private Client matchmadeClient;

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
        /// <param name="type"></param>
        /// <param name="sentFrom"></param>
        /// <param name="sendToUUID"></param>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <param name="attackedLocation"></param>
        /// <param name="damageDealt"></param>
        /// <param name="attackedUnitID"></param>
        /// <param name="attackerUnitID"></param>
        /// <param name="unitAttackDied"></param>
        public Message(msgType type, long sentFrom, long sendToUUID, int[] startLocation, int[] endLocation, int[] attackedLocation, int damageDealt,
            int attackedUnitID, int attackerUnitID, bool unitAttackDied)
        {
            this.type = (int)msgType.Move;
            this.startLocation = startLocation;
            this.endLocation = endLocation;
            this.attackedLocation = attackedLocation;
            this.damageDealt = damageDealt;
            this.attackedUnitID = attackedUnitID;
            this.attackerUnitID = attackerUnitID;
            this.unitAttackDied = unitAttackDied;
            this.sentFrom = sentFrom;
            this.sendToUUID = sendToUUID;
        }

        public override string ToString()
        {
            if (sendToUUID != null)
            {
                return base.ToString() + ": [From]: " + sentFrom.ToString() + ", [To]: " + sendToUUID + ", [Type]: " + type + ", [msg]: " + msg.ToString();
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
            m.Write(this.startLocation[0]);
            m.Write(this.startLocation[1]);
            m.Write(this.endLocation[0]);
            m.Write(this.endLocation[1]);
            m.Write(this.attackedLocation[0]);
            m.Write(this.attackedLocation[1]);
            m.Write(this.damageDealt);
            m.Write(this.attackedUnitID);
            m.Write(this.attackerUnitID);
            m.Write(this.unitAttackDied);
        }
    }

}