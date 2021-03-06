﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;

namespace Silverback.Tests.TestTypes
{
    public class TestProducer : Producer
    {
        public List<TestBroker.ProducedMessage> ProducedMessages { get; }

        public TestProducer(TestBroker broker, IEndpoint endpoint)
            : base(broker, endpoint, new NullLogger<TestProducer>())
        {
            ProducedMessages = broker.ProducedMessages;
        }

        protected override void Produce(IMessage message, byte[] serializedMessage)
        {
            ProducedMessages.Add(new TestBroker.ProducedMessage(serializedMessage, Endpoint));
        }

        protected override Task ProduceAsync(IMessage message, byte[] serializedMessage)
        {
            Produce(message, serializedMessage);
            return Task.CompletedTask;
        }
    }
}