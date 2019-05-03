﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Bogus;
using Bogus.DataSets;


namespace Confluent.Examples.StatelessProcessor
{
    class Program
    {
        async static Task Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: .. <recreate|fakegen|process|log> <instance id> <bootstrap servers>");
                Environment.Exit(1);
            }

            var command = args[0];
            var instanceId = args[1];
            var brokerAddress = args[2];

            var simulatedWeblogTopic = "simulated-weblog";
            var filteredWeblogTopic = "filtered-weblog";

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            switch (command)
            {
                case "recreate":
                    var topicSpecs = new TopicSpecification[]
                    {
                        new TopicSpecification { Name = simulatedWeblogTopic, NumPartitions = 1, ReplicationFactor = 1 },
                        new TopicSpecification { Name = filteredWeblogTopic, NumPartitions = 1, ReplicationFactor = 1 }
                    };

                    Console.WriteLine("recreating topics...");
                    using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = brokerAddress }).Build())
                    {
                        try
                        {
                            await adminClient.DeleteTopicsAsync(topicSpecs.Select(ts => ts.Name));
                        }
                        catch (DeleteTopicsException ex)
                        {
                            foreach (var deleteResult in ex.Results)
                            {
                                if (deleteResult.Error.Code != ErrorCode.UnknownTopicOrPart)
                                {
                                    throw;
                                }
                            }
                        }
                        // 
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        await adminClient.CreateTopicsAsync(topicSpecs);
                    }
                    Console.WriteLine("done.");
                    break;

                case "fakegen":                    
                    // 1. A processor that generates some fake weblog data.
                    Random r = new Random();
                    var fakeDataSourceProcessor = new Processor<Null, Null, Null, string>
                    {
                        BootstrapServers = brokerAddress,
                        OutputTopic = simulatedWeblogTopic,
                        Function = (_) =>
                        {
                            Thread.Sleep(r.Next(1000));
                            return new Message<Null, string> { Value = WebLogLine.GenerateFake() };
                        }
                    };
                    fakeDataSourceProcessor.Start(instanceId, cts.Token);
                    break;

                case "process":
                    // 2. A processor that does a (mock) geoip lookup, removes pii information
                    //    (IP address), and repartitions by country.
                    var transformProcessor = new Processor<Null, string, string, string>
                    {
                        Name = "geo-lookup-processor",
                        BootstrapServers = brokerAddress,
                        InputTopic = simulatedWeblogTopic,
                        OutputTopic = filteredWeblogTopic,
                        ConsumeErrorTolerance = ErrorTolerance.All,
                        Function = (m) => 
                        {
                            try
                            {
                                var logline = m.Value;
                                var firstSpaceIndex = logline.IndexOf(' ');
                                if (firstSpaceIndex < 0)
                                {
                                    throw new FormatException("unexpected logline format");
                                }
                                var ip = logline.Substring(0, firstSpaceIndex);
                                var country = MockGeoLookup.GetCountryFromIPAsync(ip);
                                var loglineWithoutIP = logline.Substring(firstSpaceIndex+1);
                                var dateStart = loglineWithoutIP.IndexOf('[');
                                var dateEnd = loglineWithoutIP.IndexOf(']');
                                if (dateStart < 0 || dateEnd < 0 || dateEnd < dateStart)
                                {
                                    throw new FormatException("unexpected logline format");
                                }
                                var requestInfo = loglineWithoutIP.Substring(dateEnd + 2);
                                return new Message<string, string> { Key = country, Value = requestInfo };
                            }
                            catch (Exception)
                            {
                                // Unhandled exceptions in your processing function will cause the 
                                // processor to terminate.

                                return null; // null -> filter (don't write output message corresponding to input message).
                            }
                        }
                    };
                    transformProcessor.Start(instanceId, cts.Token);
                    break;

                case "log":
                    // 3. A processor that just writes messages to stdout.
                    var consoleWriterProcessor = new Processor<string, string, Null, Null>
                    {
                        Name = "console-writer",
                        BootstrapServers = brokerAddress,
                        InputTopic = filteredWeblogTopic,
                        Function = (m) =>
                        {
                            Console.WriteLine($"{m.Key} ~~~ {m.Value}");
                            return null;
                        }
                    };
                    consoleWriterProcessor.Start(instanceId, cts.Token);
                    break;

                default:
                    Console.WriteLine($"unknown command {command}");
                    break;
            }

        }
    }
}
