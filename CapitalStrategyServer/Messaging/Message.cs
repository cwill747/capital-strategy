using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer.Messaging
{
    public enum msgType : int { Matchmaking, Chat };

    public class Message
    {
        public String msg { get; set; }
        public int type;
        public bool waitingToSend;
        public long sentFrom;
        public long sendToUUID;
        public bool processed;

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

    }


}