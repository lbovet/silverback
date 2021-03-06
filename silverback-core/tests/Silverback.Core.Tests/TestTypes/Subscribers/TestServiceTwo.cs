﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System.Threading.Tasks;
using Silverback.Core.Tests.TestTypes.Messages;
using Silverback.Messaging.Messages;
using Silverback.Messaging.Subscribers;

namespace Silverback.Core.Tests.TestTypes.Subscribers
{
    public class TestServiceTwo : IService
    {
        public int ReceivedMessagesCount { get; set; }

        [Subscribe]
        public void TestTwo(TestCommandTwo command) => ReceivedMessagesCount++;

        [Subscribe]
        public async Task TestTwoAsync(TestCommandTwo command) =>
            await Task.Run(async () =>
            {
                await Task.Delay(10);
                return ReceivedMessagesCount++;
            });

        [Subscribe]
        public async Task OnCommit(TransactionCommitEvent message) =>
            await Task.Run(async () =>
            {
                await Task.Delay(10);
                return ReceivedMessagesCount++;
            });

        [Subscribe]
        public void OnRollback(TransactionRollbackEvent message) => ReceivedMessagesCount++;
    }
}