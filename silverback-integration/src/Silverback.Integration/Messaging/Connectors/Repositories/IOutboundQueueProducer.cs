﻿// Copyright (c) 2018-2019 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Collections.Generic;
using System.Threading.Tasks;
using Silverback.Messaging.Messages;

namespace Silverback.Messaging.Connectors.Repositories
{
    public interface IOutboundQueueProducer
    {
        Task Enqueue(object message, IEnumerable<MessageHeader> headers, IEndpoint endpoint);

        Task Commit();

        Task Rollback();
    }
}