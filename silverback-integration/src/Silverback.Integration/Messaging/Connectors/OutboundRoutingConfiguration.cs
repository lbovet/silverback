﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Connectors
{
    public class OutboundRoutingConfiguration : IOutboundRoutingConfiguration // TODO: Unit test this?
    {
        private readonly List<OutboundRoute> _routes = new List<OutboundRoute>();

        public IReadOnlyCollection<OutboundRoute> Routes => _routes.AsReadOnly();

        public IOutboundRoutingConfiguration Add<TMessage>(IEndpoint endpoint, Type outboundConnectorType) where TMessage : IIntegrationMessage =>
            Add(typeof(TMessage), endpoint, outboundConnectorType);

        public IOutboundRoutingConfiguration Add(Type messageType, IEndpoint endpoint, Type outboundConnectorType)
        {
            if (!typeof(IIntegrationMessage).IsAssignableFrom(messageType))
                throw new ArgumentException("The message must be an IIntegrationMessage to be configured for outbound routing.");

            _routes.Add(new OutboundRoute(messageType, endpoint, outboundConnectorType));
            return this;
        }

        public IEnumerable<IOutboundRoute> GetRoutes(IIntegrationMessage message) =>
            _routes.Where(r => r.MessageType.IsInstanceOfType(message)).ToList();

        public class OutboundRoute : IOutboundRoute
        {
            public OutboundRoute(Type messageType, IEndpoint destinationEndpoint, Type outboundConnectorType)
            {
                MessageType = messageType;
                DestinationEndpoint = destinationEndpoint;
                OutboundConnectorType = outboundConnectorType;
            }

            public Type MessageType { get; }
            public IEndpoint DestinationEndpoint { get; }
            public Type OutboundConnectorType { get; }
        }
    }
}