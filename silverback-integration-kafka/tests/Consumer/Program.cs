﻿// Copyright (c) 2018 Sergio Aquilini
// This code is licensed under MIT license (see LICENSE file for details)

using System;
using System.Threading;
using Messages;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Silverback.Messaging;
using Silverback.Messaging.Broker;
using Silverback.Messaging.Messages;

namespace Consumer
{
    internal static class Program
    {
        private static IBroker _broker;
        private static IConsumer _consumer;

        private static void Main()
        {
            Console.Clear();
            
            PrintHeader();

            Connect();
            Console.CancelKeyPress += (_, e) => { Disconnect(); };

            while (true)
                Console.ReadLine();
        }

        private static void Connect()
        {
            _broker = new KafkaBroker(GetLoggerFactory());

            _consumer = _broker.GetConsumer(new KafkaConsumerEndpoint("Topic1")
            {
                Configuration = new KafkaConsumerConfig
                {
                    BootstrapServers = "PLAINTEXT://kafka:9092",
                    GroupId = "silverback-consumer",
                    AutoOffsetReset = Confluent.Kafka.AutoOffsetResetType.Earliest
                }
            });

            _consumer.Received += OnMessageReceived;

            _broker.Connect();
        }
        private static void Disconnect()
        {
            _broker.Disconnect();
        }

        private static void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            var testMessage = args.Message as TestMessage;

            if (testMessage == null)
            {
                Console.WriteLine("Received a weird message!");
                return;
            }

            Console.WriteLine($"[{testMessage.Id}] {testMessage.Text}");

            var text = testMessage.Text.ToLower().Trim();
            if (text == "bad")
            {
                Console.WriteLine("--> Bad message, throwing exception!");
                throw new Exception("Bad!");
            }
            else if (text.StartsWith("delay"))
            {
                if (int.TryParse(text.Substring(5), out int delay) && delay > 0)
                {
                    Console.WriteLine($"--> Delaying execution of {delay} seconds!");
                    Thread.Sleep(delay * 1000);
                }
            }

            _consumer.Acknowledge(args.Offset);
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"  _  __      __ _          _____                                          ");
            Console.WriteLine(@" | |/ /     / _| |        / ____|                                         ");
            Console.WriteLine(@" | ' / __ _| |_| | ____ _| |     ___  _ __  ___ _   _ _ __ ___   ___ _ __ ");
            Console.WriteLine(@" |  < / _` |  _| |/ / _` | |    / _ \| '_ \/ __| | | | '_ ` _ \ / _ \ '__|");
            Console.WriteLine(@" | . \ (_| | | |   < (_| | |___| (_) | | | \__ \ |_| | | | | | |  __/ |   ");
            Console.WriteLine(@" |_|\_\__,_|_| |_|\_\__,_|\_____\___/|_| |_|___/\__,_|_| |_| |_|\___|_|   ");
            Console.ResetColor();
            Console.WriteLine("\nCtrl-C to quit.\n");
        }

        private static ILoggerFactory GetLoggerFactory()
        {
            var loggerFactory = new LoggerFactory()
                .AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            NLog.LogManager.LoadConfiguration("nlog.config");

            return loggerFactory;
        }
    }
}
