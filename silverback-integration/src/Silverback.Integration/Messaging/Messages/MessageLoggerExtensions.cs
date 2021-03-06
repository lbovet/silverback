﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Silverback.Messaging.Batch;

namespace Silverback.Messaging.Messages
{
    // TODO: Review and test
    public static class MessageLoggerExtensions
    {
        public static void LogTrace(this ILogger logger, string logMessage, IMessage message, IEndpoint endpoint = null, MessageBatch batch = null) =>
            Log(logger, LogLevel.Trace, null, logMessage, message, endpoint, batch);

        public static void LogWarning(this ILogger logger, Exception exception, string logMessage, IMessage message, IEndpoint endpoint = null, MessageBatch batch = null) =>
            Log(logger, LogLevel.Warning, exception, logMessage, message, endpoint, batch);

        public static void LogCritical(this ILogger logger, Exception exception, string logMessage, IMessage message, IEndpoint endpoint = null, MessageBatch batch = null) =>
            Log(logger, LogLevel.Critical, exception, logMessage, message, endpoint, batch);

        public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, string logMessage, IMessage message, IEndpoint endpoint = null, MessageBatch batch = null)
        {
            var failedMessage = message as FailedMessage;

            var innerMessage = failedMessage?.Message ?? message;

            var properties = new List<(string, string, object)>();

            if (innerMessage is IIntegrationMessage integrationMessage)
                properties.Add(("id", "messageId", integrationMessage.Id));

            if (endpoint != null)
                properties.Add(("endpoint", "endpointName", endpoint.Name));

            properties.Add(("type", "messageType", innerMessage.GetType().Name));

            if (batch != null)
            {
                properties.Add(("batchId", "batchId", batch.CurrentBatchId));
                properties.Add(("batchSize", "batchSize", batch.CurrentSize));
            }
            else if (message is BatchMessage batchMessage)
            {
                properties.Add(("batchId", "batchId", batchMessage.BatchId));
                properties.Add(("batchSize", "batchSize", batchMessage.BatchSize));
            }

            if (failedMessage != null)
                properties.Add(("failedAttempts", "failedAttempts", failedMessage.FailedAttempts));

                logger.Log(
                        logLevel, exception,
                        logMessage + " {{" + string.Join(", ", properties.Select(p => $"\"{p.Item1}\"={{{p.Item2}}}")) + "}}",
                        properties.Select(p => p.Item3).ToArray());
        }
    }
}
