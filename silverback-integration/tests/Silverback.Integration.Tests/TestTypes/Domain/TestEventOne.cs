﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Silverback.Messaging.Messages;

namespace Silverback.Tests.TestTypes.Domain
{
    public class TestEventOne : IIntegrationEvent
    {
        public string Content { get; set; }
        public Guid Id { get; set; }
    }
}
