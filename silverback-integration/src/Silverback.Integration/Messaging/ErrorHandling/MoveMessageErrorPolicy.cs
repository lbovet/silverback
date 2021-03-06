﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.ErrorHandling
{
    /// <summary>
    /// This policy moves the failed messages to the configured endpoint.
    /// </summary>
    public class MoveMessageErrorPolicy : ErrorPolicyBase
    {
        private readonly IProducer _producer;
        private readonly IEndpoint _endpoint;
        private readonly ILogger _logger;
        private Func<IMessage, Exception, IMessage> _transformationFunction;

        public MoveMessageErrorPolicy(IBroker broker, IEndpoint endpoint, ILogger<MoveMessageErrorPolicy> logger) 
            : base(logger)
        {
            _producer = broker.GetProducer(endpoint);
            _endpoint = endpoint;
            _logger = logger;
        }

        public MoveMessageErrorPolicy Transform(Func<IMessage, Exception, IMessage> transformationFunction)
        {
            _transformationFunction = transformationFunction;
            return this;
        }

        public override ErrorAction HandleError(FailedMessage failedMessage, Exception exception)
        {
            if (failedMessage.Message is BatchMessage batchMessage)
            {
                foreach (var singleFailedMessage in batchMessage.Messages)
                {
                    PublishToNewEndpoint(singleFailedMessage, exception);
                }
            }
            else
            {
                PublishToNewEndpoint(failedMessage, exception);
            }

            _logger.LogTrace("The failed message has been moved and will be skipped.", failedMessage, _endpoint);

            return ErrorAction.Skip;
        }

        private void PublishToNewEndpoint(IMessage failedMessage, Exception exception)
        {
            _producer.Produce(_transformationFunction?.Invoke(failedMessage, exception) ?? failedMessage);
        }
    }
}