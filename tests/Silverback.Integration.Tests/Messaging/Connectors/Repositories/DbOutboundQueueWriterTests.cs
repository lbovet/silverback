﻿// Copyright (c) 2020 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Silverback.Database;
using Silverback.Messaging.Connectors.Repositories;
using Silverback.Messaging.Messages;
using Silverback.Tests.Integration.TestTypes;
using Silverback.Tests.Integration.TestTypes.Database;
using Silverback.Tests.Integration.TestTypes.Domain;
using Xunit;

namespace Silverback.Tests.Integration.Messaging.Connectors.Repositories
{
    public class DbOutboundQueueWriterTests : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private readonly IServiceScope _scope;

        private readonly TestDbContext _dbContext;

        private readonly DbOutboundQueueWriter _queueWriter;

        private readonly IOutboundEnvelope _sampleOutboundEnvelope = new OutboundEnvelope(
            new TestEventOne { Content = "Test" },
            null,
            TestProducerEndpoint.GetDefault());

        public DbOutboundQueueWriterTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var services = new ServiceCollection();

            services
                .AddNullLogger()
                .AddDbContext<TestDbContext>(
                    options => options
                        .UseSqlite(_connection))
                .AddSilverback()
                .UseDbContext<TestDbContext>();

            var serviceProvider = services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateScopes = true
                });

            _scope = serviceProvider.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<TestDbContext>();
            _dbContext.Database.EnsureCreated();

            _queueWriter = new DbOutboundQueueWriter(_scope.ServiceProvider.GetRequiredService<IDbContext>());
        }

        [Fact]
        public void Enqueue_Message_TableStillEmpty()
        {
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Enqueue(_sampleOutboundEnvelope);

            _dbContext.OutboundMessages.Count().Should().Be(0);
        }

        [Fact]
        public void EnqueueCommitAndSaveChanges_Message_MessageAddedToQueue()
        {
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Commit();
            _dbContext.SaveChanges();

            _dbContext.OutboundMessages.Count().Should().Be(3);
        }

        [Fact]
        public void EnqueueAndRollback_Message_TableStillEmpty()
        {
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Enqueue(_sampleOutboundEnvelope);
            _queueWriter.Rollback();

            _dbContext.OutboundMessages.Count().Should().Be(0);
        }

        public async ValueTask DisposeAsync()
        {
            await _dbContext.DisposeAsync();
            await _connection.DisposeAsync();
            _scope.Dispose();
        }
    }
}