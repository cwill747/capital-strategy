﻿using CapitalStrategyServer.Messaging;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapitalStrategyServer
{
    class Server
    {
        public QueueManager qm;
        public MessageQueue msgQueue;
        public List<Client> connections;
        public NetServer netserver;

        public Server()
        {
            qm = new QueueManager(this);
            msgQueue = new MessageQueue(this);
            connections = new List<Client>();
        }

        public void addConnection(Client c)
        {
            connections.Add(c);
            this.qm.clientConnected(c);
        }
    }
}
