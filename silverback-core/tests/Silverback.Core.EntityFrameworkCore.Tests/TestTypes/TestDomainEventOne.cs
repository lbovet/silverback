﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using Silverback.Domain;

namespace Silverback.Core.EntityFrameworkCore.Tests.TestTypes
{
    public class TestDomainEventOne : DomainEvent<TestAggregateRoot>
    {
        public string Message { get; set; }
    }
}