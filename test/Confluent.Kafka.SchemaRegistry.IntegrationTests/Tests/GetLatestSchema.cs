using System;
using System.Collections.Generic;
using Xunit;
using Confluent.Kafka.SchemaRegistry;

namespace Confluent.Kafka.SchemaRegistry.IntegrationTests
{
    public static partial class Tests
    {
        [Theory, MemberData(nameof(SchemaRegistryParameters))]
        public static void GetLatestSchema(string server)
        {
            var topicName = Guid.NewGuid().ToString();

            var testSchema1 = 
                "{\"type\":\"record\",\"name\":\"User\",\"namespace\":\"Confluent.Kafka.Examples.AvroSpecific" +
                "\",\"fields\":[{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"favorite_number\",\"type\":[\"i" +
                "nt\",\"null\"]},{\"name\":\"favorite_color\",\"type\":[\"string\",\"null\"]}]}";

            var sr = new CachedSchemaRegistryClient(new Dictionary<string, object>{ { "schema.registry.url", server } });

            var subject = sr.ConstructValueSubjectName(topicName);
            var id = sr.RegisterAsync(subject, testSchema1).Result;

            var schema = sr.GetLatestSchemaAsync(subject).Result;

            Assert.Equal(schema.Id, id);
            Assert.Equal(schema.Subject, subject);
            Assert.Equal(schema.Version, 1);
            Assert.Equal(schema.SchemaString, testSchema1);
        }
    }
}
