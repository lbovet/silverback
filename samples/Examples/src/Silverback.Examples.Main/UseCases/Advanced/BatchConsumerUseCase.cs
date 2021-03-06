// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Examples.Common.Messages;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Configuration;
using Silverback.Messaging.Publishing;

namespace Silverback.Examples.Main.UseCases.Advanced
{
    public class BatchProcessingUseCase : UseCase
    {
        public BatchProcessingUseCase() : base("Processing in batch w/ multiple threads", 30)
        {
        }

        protected override void ConfigureServices(IServiceCollection services) => services
            .AddBus()
            .AddBroker<KafkaBroker>(options => options
                .AddOutboundConnector());

        protected override void Configure(IBrokerEndpointsConfigurationBuilder endpoints,
            IServiceProvider serviceProvider) => endpoints
            .AddOutbound<SampleBatchProcessedEvent>(CreateEndpoint())
            .Broker.Connect();

        private KafkaEndpoint CreateEndpoint() =>
            new KafkaProducerEndpoint("silverback-examples-batch")
            {
                Configuration = new KafkaProducerConfig
                {
                    BootstrapServers = "PLAINTEXT://kafka:9092",
                    ClientId = GetType().FullName
                }
            };

        protected override async Task Execute(IServiceProvider serviceProvider)
        {
            var publisher = serviceProvider.GetService<IEventPublisher>();

            for (int i = 0; i < 22; i++)
            {
                await publisher.PublishAsync(new SampleBatchProcessedEvent
                {
                    Content = (i + 1) + " -" + DateTime.Now.ToString("HH:mm:ss.fff")
                });
            }
        }
    }
}