﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using Silverback.Messaging.Messages;

namespace Silverback.Integration.Kafka.Tests.TestTypes.Messages
{
    public class SingleKeyMemberMessage : IIntegrationMessage
    {
        public Guid Id { get; set; }
        [KeyMember]
        public string One { get; set; }
        public string Two { get; set; }
        public string Three { get; set; }
    }
}