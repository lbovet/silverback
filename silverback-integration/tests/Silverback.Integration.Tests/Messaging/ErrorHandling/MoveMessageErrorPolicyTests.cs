﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Publishing;
using Silverback.Tests.TestTypes;
using Silverback.Tests.TestTypes.Domain;

namespace Silverback.Tests.Messaging.ErrorHandling
{
    [TestFixture]
    public class MoveMessageErrorPolicyTests
    {
        private ErrorPolicyBuilder _errorPolicyBuilder;
        private IBroker _broker;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            services.AddSingleton<IPublisher, Publisher>();

            services.AddBroker<TestBroker>(options => { });

            var serviceProvider = services.BuildServiceProvider();

            _errorPolicyBuilder = new ErrorPolicyBuilder(serviceProvider, NullLoggerFactory.Instance);

            _broker = serviceProvider.GetRequiredService<IBroker>();
            _broker.Connect();
        }

        [Test]
        public void TryHandleMessage_Failed_MessageMoved()
        {
            var policy = _errorPolicyBuilder.Move(TestEndpoint.Default);

            policy.HandleError(new FailedMessage(new TestEventOne()), new Exception("test"));

            var producer = (TestProducer)_broker.GetProducer(TestEndpoint.Default);

            Assert.That(producer.ProducedMessages.Count, Is.EqualTo(1));
        }

        [Test]
        public void Transform_Failed_MessageTranslated()
        {
            var policy = _errorPolicyBuilder.Move(TestEndpoint.Default)
                .Transform((msg, ex) => new TestEventTwo());

            policy.HandleError(new FailedMessage(new TestEventOne()), new Exception("test"));

            var producer = (TestProducer)_broker.GetProducer(TestEndpoint.Default);
            var producedMessage = producer.Endpoint.Serializer.Deserialize(producer.ProducedMessages[0].Message);
            Assert.That(producedMessage, Is.InstanceOf<TestEventTwo>());
        }
    }
}