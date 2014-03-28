﻿using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer
{
    public class Client
    {
        public long uniqueIdentifier;
        public bool inGame;
        public bool lookingForGame;

        public Client() : this(0L, false, false)
        {
            
        }

        public Client(long uuid) : this(uuid, false, false)
        {

        }

        public Client(long uniqueIdentifier, bool inGame, bool lookingForGame)
        {
            this.uniqueIdentifier = uniqueIdentifier;
            this.inGame = inGame;
            this.lookingForGame = lookingForGame;
        }

       public override string ToString()
        {
            return NetUtility.ToHexString(uniqueIdentifier);
        }

    }
}
