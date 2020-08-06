// Copyright 2020 Confluent Inc.
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

#pragma warning disable xUnit1026

using System;
using System.Collections.Generic;
using Xunit;


namespace Confluent.Kafka.IntegrationTests
{
    public partial class Tests
    {
        /// <summary>
        ///     Test <see cref="Consumer.IncrementalAssign" /> and <see cref="Consumer.IncrementalUnassign" />.
        /// </summary>
        [Theory, MemberData(nameof(KafkaParameters))]
        public void Consumer_Incremental_1(string bootstrapServers)
        {
            LogToFile("start Consumer_Incremental_1");

            var consumerConfig = new ConsumerConfig
            {
                GroupId = Guid.NewGuid().ToString(),
                BootstrapServers = bootstrapServers,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Error
            };

            using (var consumer = new ConsumerBuilder<byte[], byte[]>(consumerConfig).Build())
            using (var topic1 = new TemporaryTopic(bootstrapServers, 1))
            using (var topic2 = new TemporaryTopic(bootstrapServers, 1))
            {
                Util.ProduceNullStringMessages(bootstrapServers, topic1.Name, 1, 1);
                Util.ProduceNullStringMessages(bootstrapServers, topic2.Name, 1, 1);
        
                consumer.IncrementalAssign(new TopicPartitionOffset(topic1.Name, 0, Offset.Beginning));
                var cr1 = consumer.Consume(TimeSpan.FromSeconds(10));
                Assert.NotNull(cr1);
                Assert.Equal(0, cr1.Offset);
                Assert.Equal(topic1.Name, cr1.Topic);
                Assert.Equal(0, (int)cr1.Partition);
                consumer.Commit(cr1);

                consumer.IncrementalAssign(new TopicPartitionOffset(topic2.Name, 0, Offset.Beginning));
                var cr2 = consumer.Consume(TimeSpan.FromSeconds(10));
                Assert.NotNull(cr2);
                Assert.Equal(0, cr2.Offset);
                Assert.Equal(topic2.Name, cr2.Topic);
                Assert.Equal(0, (int)cr2.Partition);

                consumer.IncrementalUnassign(new TopicPartition(topic1.Name, 0));
                Util.ProduceNullStringMessages(bootstrapServers, topic1.Name, 2, 1);
                var cr4 = consumer.Consume(TimeSpan.FromSeconds(2));
                Assert.Null(cr4);

                Util.ProduceNullStringMessages(bootstrapServers, topic2.Name, 3, 1);
                var cr5 = consumer.Consume(TimeSpan.FromSeconds(10));
                Assert.NotNull(cr5);
                Assert.Equal(1, cr5.Offset);
                Assert.Equal(topic2.Name, cr5.Topic);
                Assert.Equal(0, (int)cr5.Partition);
                Assert.Equal(3, cr5.Message.Value.Length);

                consumer.IncrementalAssign(new TopicPartition(topic1.Name, 0));
                var cr6 = consumer.Consume(TimeSpan.FromSeconds(10));
                Assert.NotNull(cr1);
                Assert.Equal(1, cr1.Offset);
                Assert.Equal(topic1.Name, cr1.Topic);
                Assert.Equal(0, (int)cr1.Partition);

                consumer.IncrementalUnassign(new TopicPartition(topic1.Name, 0));
                consumer.Unassign();
            }

            Assert.Equal(0, Library.HandleCount);
            LogToFile("end   Consumer_Incremental_1");
        }
    }
}
