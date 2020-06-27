﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using FluentAssertions;
using Silverback.Messaging.Broker;
using Xunit;

namespace Silverback.Tests.Integration.Kafka.Messaging.Broker
{
    public class KafkaOffsetTests
    {
        [Fact]
        public void Constructor_WithKeyValueString_ProperlyConstructed()
        {
            var offset = new KafkaOffset("test-topic[2]", "42");

            offset.Key.Should().Be("test-topic[2]");
            offset.Value.Should().Be("42");

            offset.Topic.Should().Be("test-topic");
            offset.Partition.Should().Be(2);
            offset.Offset.Should().Be(42);
        }

        [Fact]
        public void Constructor_WithTopicPartitionOffset_ProperlyConstructed()
        {
            var offset = new KafkaOffset("test-topic", 2, 42);

            offset.Key.Should().Be("test-topic[2]");
            offset.Value.Should().Be("42");

            offset.Topic.Should().Be("test-topic");
            offset.Partition.Should().Be(2);
            offset.Offset.Should().Be(42);
        }

        [Theory]
        [InlineData(5, 10, true)]
        [InlineData(5, 3, false)]
        [InlineData(5, 5, false)]
        public void LessThanOperator_SomeOffsets_ProperlyCompared(int valueA, int valueB, bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = new KafkaOffset("test-topic", 2, valueB);

            var result = offsetA < offsetB;

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(10, 5, true)]
        [InlineData(1, 3, false)]
        [InlineData(5, 5, false)]
        public void GreaterThanOperator_SomeOffsets_ProperlyCompared(int valueA, int valueB, bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = new KafkaOffset("test-topic", 2, valueB);

            var result = offsetA > offsetB;

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(5, 10, true)]
        [InlineData(5, 3, false)]
        [InlineData(5, 5, true)]
        public void LessThanOrEqualOperator_SomeOffsets_ProperlyCompared(int valueA, int valueB, bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = new KafkaOffset("test-topic", 2, valueB);

            var result = offsetA <= offsetB;

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(10, 5, true)]
        [InlineData(1, 3, false)]
        [InlineData(5, 5, true)]
        public void GreaterThanOrEqualOperator_SomeOffsets_ProperlyCompared(
            int valueA,
            int valueB,
            bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = new KafkaOffset("test-topic", 2, valueB);

            var result = offsetA >= offsetB;

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(5, 10, false)]
        [InlineData(5, 3, false)]
        [InlineData(5, 5, true)]
        [InlineData(5, null, false)]
        public void EqualityOperator_SomeOffsets_ProperlyCompared(int valueA, int? valueB, bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = valueB != null ? new KafkaOffset("test-topic", 2, valueB.Value) : null;

            var result = offsetA == offsetB!;

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(10, 5, true)]
        [InlineData(1, 3, true)]
        [InlineData(5, 5, false)]
        [InlineData(5, null, true)]
        public void InequalityOperator_SomeOffsets_ProperlyCompared(int valueA, int? valueB, bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = valueB != null ? new KafkaOffset("test-topic", 2, valueB.Value) : null;

            var result = offsetA != offsetB!;

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void ToLogString_Offset_StringReturned()
        {
            var offset = new KafkaOffset("test-topic", 2, 42);

            var logString = offset.ToLogString();

            logString.Should().Be("2@42");
        }

        [Theory]
        [InlineData(10, 5, 1)]
        [InlineData(1, 3, -1)]
        [InlineData(5, 5, 0)]
        [InlineData(5, null, 1)]
        public void CompareTo_AnotherKafkaOffset_ProperlyCompared(int valueA, int? valueB, int expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = valueB != null ? new KafkaOffset("test-topic", 2, valueB.Value) : null;

            var result = offsetA.CompareTo(offsetB);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(10, 5, 1)]
        [InlineData(1, 3, -1)]
        [InlineData(5, 5, 0)]
        [InlineData(5, null, 1)]
        public void CompareTo_AnotherOffset_ProperlyCompared(int valueA, int? valueB, int expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = valueB != null ? new KafkaOffset("test-topic", 2, valueB.Value) : null;

            var result = offsetA.CompareTo((IOffset?)offsetB);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(10, 5, false)]
        [InlineData(1, 3, false)]
        [InlineData(5, 5, true)]
        [InlineData(5, null, false)]
        public void Equals_AnotherKafkaOffset_ProperlyCompared(int valueA, int? valueB, bool expectedResult)
        {
            var offsetA = new KafkaOffset("test-topic", 2, valueA);
            var offsetB = valueB != null ? new KafkaOffset("test-topic", 2, valueB.Value) : null;

            var result = offsetA.Equals(offsetB);

            result.Should().Be(expectedResult);
        }
    }
}