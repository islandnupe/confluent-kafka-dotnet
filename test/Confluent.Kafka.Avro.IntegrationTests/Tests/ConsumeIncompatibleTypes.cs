﻿// Copyright 2018 Confluent Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Refer to LICENSE for more information.

using System;
using System.Collections.Generic;
using Confluent.SchemaRegistry;
using Confluent.Kafka.AvroSerdes;
using Confluent.Kafka.Examples.AvroSpecific;
using Xunit;


namespace Confluent.Kafka.Avro.IntegrationTests
{
    public static partial class Tests
    {
        /// <summary>
        ///     Test that consuming a key/value with schema incompatible with
        ///     the strongly typed consumer instance results in an appropriate
        ///     consume error event.
        /// </summary>
        [Theory, MemberData(nameof(TestParameters))]
        public static void ConsumeIncompatibleTypes(string bootstrapServers, string schemaRegistryServers)
        {
            string topic = Guid.NewGuid().ToString();

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = Guid.NewGuid().ToString(),
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };
            
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                SchemaRegistryUrl = schemaRegistryServers
            };

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var producer = new AvroProducer(producerConfig))
            {
                producer.RegisterAvroSerializer(new AvroSerializer<string>(schemaRegistry));
                producer.RegisterAvroSerializer(new AvroSerializer<User>(schemaRegistry));

                var user = new User
                {
                    name = "username",
                    favorite_number = 107,
                    favorite_color = "orange"
                };

                producer
                    .ProduceAsync(topic, new Message<string, User> { Key = user.name, Value = user },SerdeType.Avro, SerdeType.Avro)
                    .Wait();
            }

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var consumer = new AvroConsumer(consumerConfig))
            {
                consumer.RegisterAvroDeserializer(new AvroDeserializer<User>(schemaRegistry));

                consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 0) });

                bool hadError = false;
                try
                {
                    consumer
                        .ConsumeAsync<User, User>(SerdeType.Avro, SerdeType.Avro, TimeSpan.FromSeconds(10))
                        .Wait();
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.GetType() != typeof(global::Avro.AvroException))
                    {
                        throw e.InnerException;
                    }
                    hadError = true;
                }

                Assert.True(hadError);
            }

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var consumer = new AvroConsumer(consumerConfig))
            {
                consumer.RegisterAvroDeserializer(new AvroDeserializer<string>(schemaRegistry));

                consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(topic, 0, 0) });

                bool hadError = false;
                try
                {
                    consumer
                        .ConsumeAsync<string, string>(SerdeType.Avro, SerdeType.Avro, TimeSpan.FromSeconds(10))
                        .Wait();
                }
                catch (AggregateException e)
                {
                    if (e.InnerException.GetType() != typeof(global::Avro.AvroException))
                    {
                        throw e.InnerException;
                    }
                    hadError = true;
                }

                Assert.True(hadError);
            }

        }
    }
}
