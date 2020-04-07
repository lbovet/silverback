// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Threading.Tasks;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Broker.Behaviors
{
    /// <summary>
    ///     Ensures that the message id property has been set, using the registered <see cref="IMessageIdProvider" />
    ///     to generate a unique value for it.
    /// </summary>
    public class MessageIdInitializerProducerBehavior : IProducerBehavior, ISorted
    {
        private readonly MessageIdProvider _messageIdProvider;

        public MessageIdInitializerProducerBehavior(MessageIdProvider messageIdProvider)
        {
            _messageIdProvider = messageIdProvider;
        }

        public async Task Handle(ProducerPipelineContext context, ProducerBehaviorHandler next)
        {
            _messageIdProvider.EnsureKeyIsInitialized(context.Envelope.Message, context.Envelope.Headers);

            await next(context);
        }

        public int SortIndex => BrokerBehaviorsSortIndexes.Producer.MessageIdInitializer;
    }
}