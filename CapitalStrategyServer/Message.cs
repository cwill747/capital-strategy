using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer
{
    public class Message
    {
        public String msgType { get; set; }
        public String msg { get; set; }

        public override string ToString()
        {
            return base.ToString() + ": " + "[Type]: " + msgType.ToString() + ", [msg]: " + msg.ToString();
        }

    }
}