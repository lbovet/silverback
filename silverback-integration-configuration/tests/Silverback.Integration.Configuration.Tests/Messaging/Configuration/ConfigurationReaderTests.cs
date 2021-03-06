﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Linq;
using System.Reflection;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Connectors;
using Silverback.Messaging.ErrorHandling;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Serialization;
using Silverback.Tests.Types;

namespace Silverback.Tests.Messaging.Configuration
{
    [TestFixture]
    public class ConfigurationReaderTests
    {
        private IServiceProvider _serviceProvider;
        private IBrokerEndpointsConfigurationBuilder _builder;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();

            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            services.AddSingleton(Substitute.For<IBroker>());

            _serviceProvider = services.BuildServiceProvider();

            _builder = Substitute.For<IBrokerEndpointsConfigurationBuilder>();
        }

        #region Read - Inbound

        [Test]
        public void Read_SimplestInbound_EndpointAdded()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.simplest"));

            Assert.AreEqual(2, reader.Inbound.Count);
            Assert.IsNotNull(reader.Inbound.First().Endpoint);
        }

        [Test]
        public void Read_SimplestInbound_CorrectEndpointType()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.simplest"));

            var endpoint = reader.Inbound.First().Endpoint;
            Assert.IsInstanceOf<KafkaConsumerEndpoint>(endpoint);
        }

        [Test]
        public void Read_SimplestInbound_EndpointNameSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.simplest"));

            var endpoint = reader.Inbound.Skip(1).First().Endpoint;
            Assert.AreEqual("inbound-endpoint2", endpoint.Name);
        }

        [Test]
        public void Read_SimplestInbound_DefaultSerializer()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.simplest"));

            var endpoint = reader.Inbound.First().Endpoint;
            Assert.IsNotNull(endpoint.Serializer);
        }

        [Test]
        public void Read_CompleteInbound_DefaultSerializerPropertiesSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var serializer = (JsonMessageSerializer)reader.Inbound.Skip(1).First().Endpoint.Serializer;
            Assert.AreEqual(MessageEncoding.Unicode, serializer.Encoding);
            Assert.AreEqual(Formatting.Indented, serializer.Settings.Formatting);
        }

        [Test]
        public void Read_CompleteInbound_SettingsSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var settings = reader.Inbound.First().Settings;
            Assert.IsNotNull(settings);
        }

        [Test]
        public void Read_CompleteInbound_SettingsBatchSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var batchSettings = reader.Inbound.First().Settings.Batch;
            Assert.IsNotNull(batchSettings);
        }

        [Test]
        public void Read_CompleteInbound_SettingsConsumersSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var settings = reader.Inbound.First().Settings;
            Assert.AreEqual(3, settings.Consumers);
        }

        [Test]
        public void Read_CompleteInbound_SettingsBatchPropertiesSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var batchSettings = reader.Inbound.First().Settings.Batch;
            Assert.AreEqual(5, batchSettings.Size);
            Assert.AreEqual(2, batchSettings.MaxDegreeOfParallelism);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), batchSettings.MaxWaitTime);
        }

        [Test]
        public void Read_CompleteInbound_EndpointSubPropertySet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var endpoint = (KafkaConsumerEndpoint)reader.Inbound.First().Endpoint;

            // Note: Confluent.Kafka currently has a bug preventing the property
            // value to be retrieved
            Assert.Throws<ArgumentException>(
                () => Assert.AreEqual(AutoOffsetResetType.Earliest, endpoint.Configuration.AutoOffsetReset),
                "Requested value 'earliest' was not found.");
        }

        [Test]
        public void Read_CompleteInbound_EndpointNameSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var endpoint = (KafkaConsumerEndpoint)reader.Inbound.First().Endpoint;

            Assert.AreEqual(new[] {"inbound-endpoint1"}, endpoint.Names);
        }

        [Test]
        public void Read_CompleteInbound_EndpointNamesSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var endpoint = (KafkaConsumerEndpoint)reader.Inbound.Skip(1).First().Endpoint;

            Assert.AreEqual(new[] { "inbound-endpoint1", "inbound-endpoint2" }, endpoint.Names);
        }

        [Test]
        public void Read_CompleteInbound_CustomSerializerSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var endpoint = reader.Inbound.First().Endpoint;
            Assert.IsInstanceOf<FakeSerializer>(endpoint.Serializer);
        }

        [Test]
        public void Read_CompleteInbound_CustomSerializerPropertySet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var serializer = (FakeSerializer) reader.Inbound.First().Endpoint.Serializer;
            Assert.AreEqual(4, serializer.Settings.Mode);
        }

        [Test]
        public void Read_CompleteInbound_ErrorPoliciesAdded()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            Assert.AreEqual(2, reader.Inbound.First().ErrorPolicies.Count());
        }

        [Test]
        public void Read_CompleteInbound_ErrorPolicyMaxFailedAttemptsSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var policy = reader.Inbound.First().ErrorPolicies.First();
            Assert.IsTrue(policy.CanHandle(new FailedMessage(null, 3), new ArgumentException()));
            Assert.IsFalse(policy.CanHandle(new FailedMessage(null, 6), new ArgumentException()));
        }
        
        [Test]
        public void Read_CompleteInbound_ErrorPolicyConstructorParameterSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var policy = (RetryErrorPolicy) reader.Inbound.First().ErrorPolicies.First();
            Assert.AreEqual(TimeSpan.FromMinutes(5), (TimeSpan)policy.GetType().GetField("_delayIncrement", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(policy));
        }

        [Test]
        public void Read_CompleteInbound_ErrorPolicyApplyToSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var policy = reader.Inbound.First().ErrorPolicies.First();
            Assert.IsTrue(policy.CanHandle(new FailedMessage(), new ArgumentException()));
            Assert.IsTrue(policy.CanHandle(new FailedMessage(), new InvalidOperationException()));
            Assert.IsFalse(policy.CanHandle(new FailedMessage(), new FormatException()));
        }

        [Test]
        public void Read_CompleteInbound_ErrorPolicyExcludeSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            var policy = reader.Inbound.First().ErrorPolicies.First();
            Assert.IsTrue(policy.CanHandle(new FailedMessage(), new ArgumentException()));
            Assert.IsFalse(policy.CanHandle(new FailedMessage(), new ArgumentNullException()));
        }

        [Test]
        public void Read_CompleteInbound_ConnectorTypeSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("inbound.complete"));

            Assert.AreEqual(typeof(LoggedInboundConnector), reader.Inbound.Skip(1).First().ConnectorType);
        }

        #endregion

        #region Read - Outbound

        [Test]
        public void Read_SimplestOutbound_EndpointAdded()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.simplest"));

            Assert.AreEqual(1, reader.Outbound.Count);
            Assert.IsNotNull(reader.Outbound.First().Endpoint);
        }

        [Test]
        public void Read_SimplestOutbound_CorrectEndpointType()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.simplest"));

            var endpoint = reader.Outbound.First().Endpoint;
            Assert.IsInstanceOf<KafkaProducerEndpoint>(endpoint);
        }

        [Test]
        public void Read_SimplestOutbound_EndpointNameSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.simplest"));

            var endpoint = reader.Outbound.First().Endpoint;
            Assert.AreEqual("outbound-endpoint1", endpoint.Name);
        }

        [Test]
        public void Read_SimplestOutbound_DefaultSerializer()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.simplest"));

            var endpoint = reader.Outbound.First().Endpoint;
            Assert.IsNotNull(endpoint.Serializer);
        }

        [Test]
        public void Read_SimpleOutbound_DefaultMessageType()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.simplest"));

            Assert.AreEqual(typeof(IIntegrationMessage), reader.Outbound.First().MessageType);
        }
        [Test]
        public void Read_CompleteOutbound_DefaultSerializerPropertiesSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.complete"));

            var serializer = (JsonMessageSerializer)reader.Outbound.Skip(1).First().Endpoint.Serializer;
            Assert.AreEqual(MessageEncoding.Unicode, serializer.Encoding);
            Assert.AreEqual(Formatting.Indented, serializer.Settings.Formatting);
        }

        [Test]
        public void Read_CompleteOutbound_EndpointSubPropertySet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.complete"));

            var endpoint = (KafkaProducerEndpoint)reader.Outbound.First().Endpoint;

            Assert.AreEqual(false, endpoint.Configuration.EnableBackgroundPoll);
        }

        [Test]
        public void Read_CompleteOutbound_CustomSerializerSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.complete"));

            var endpoint = reader.Outbound.First().Endpoint;
            Assert.IsInstanceOf<FakeSerializer>(endpoint.Serializer);
        }

        [Test]
        public void Read_CompleteOutbound_CustomSerializerPropertySet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.complete"));

            var serializer = (FakeSerializer)reader.Outbound.First().Endpoint.Serializer;
            Assert.AreEqual(4, serializer.Settings.Mode);
        }
        
        [Test]
        public void Read_CompleteOutbound_ConnectorTypeSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.complete"));

            Assert.AreEqual(typeof(DeferredOutboundConnector), reader.Outbound.Skip(1).First().ConnectorType);
        }

        [Test]
        public void Read_CompleteOutbound_MessageTypeSet()
        {
            var reader =
                new ConfigurationReader(_serviceProvider)
                    .Read(ConfigFileHelper.GetConfigSection("outbound.complete"));

            Assert.AreEqual(typeof(IIntegrationEvent), reader.Outbound.First().MessageType);
        }

        #endregion

        #region Read and Apply

        [Test]
        public void ReadAndApply_SimpleInbound_AddInboundCalled()
        {
            new ConfigurationReader(_serviceProvider)
                .Read(ConfigFileHelper.GetConfigSection("inbound.simplest"))
                .Apply(_builder);

            _builder.ReceivedWithAnyArgs(2).AddInbound(null, null, null, null);
        }

        [Test]
        public void ReadAndApply_CompleteInbound_AddInboundWithCorrectTypes()
        {
            new ConfigurationReader(_serviceProvider)
                .Read(ConfigFileHelper.GetConfigSection("inbound.complete"))
                .Apply(_builder);

            // TODO: Should check policies in first call
            _builder.Received(1).AddInbound(
                Arg.Any<KafkaConsumerEndpoint>(),
                null,
                Arg.Any<Func<ErrorPolicyBuilder, IErrorPolicy>>(),
                Arg.Any<InboundConnectorSettings>());
            _builder.Received(1).AddInbound(
                Arg.Any<KafkaConsumerEndpoint>(),
                typeof(LoggedInboundConnector),
                Arg.Is<Func<ErrorPolicyBuilder, IErrorPolicy>>(f => f.Invoke(null) == null),
                null);
        }

        [Test]
        public void ReadAndApply_SimpleOutbound_OutboundAdded()
        {
            new ConfigurationReader(_serviceProvider)
                .Read(ConfigFileHelper.GetConfigSection("outbound.simplest"))
                .Apply(_builder);

            _builder.ReceivedWithAnyArgs(1).AddOutbound(null, null, null);
        }

        [Test]
        public void ReadAndApply_CompleteOutbound_AddOutboundWithCorrectTypes()
        {
            new ConfigurationReader(_serviceProvider)
                .Read(ConfigFileHelper.GetConfigSection("outbound.complete"))
                .Apply(_builder);

            _builder.Received(1).AddOutbound(
                typeof(IIntegrationEvent),
                Arg.Any<KafkaProducerEndpoint>(),
                null);
            _builder.Received(1).AddOutbound(
                typeof(IIntegrationCommand),
                Arg.Any<KafkaProducerEndpoint>(),
                typeof(DeferredOutboundConnector));
        }
        #endregion
    }
}
