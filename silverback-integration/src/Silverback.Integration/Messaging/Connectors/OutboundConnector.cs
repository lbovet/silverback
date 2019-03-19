﻿// Copyright (c) 2018-2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Collections.Generic;
using System.Threading.Tasks;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Connectors
{
    public class OutboundConnector : IOutboundConnector
    {
        private readonly IBroker _broker;

        public OutboundConnector(IBroker broker)
        {
            _broker = broker;
        }

        public Task RelayMessage(object message, IEnumerable<MessageHeader> headers, IEndpoint destinationEndpoint) =>
            _broker.GetProducer(destinationEndpoint).ProduceAsync(message, headers);
    }
}