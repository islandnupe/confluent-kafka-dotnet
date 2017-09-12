﻿//using Confluent.Kafka.SchemaRegistry.Serializer;
//using Moq;
//using Xunit;

//namespace Confluent.Kafka.SchemaRegistry.UnitTests.Serializer
//{
//    public class KafkaGenericAvroSerializerTest
//    {
//        [Fact]
//        public void GenericWithRecordType()
//        {
//            string topic = "test";
//            int topicId = 3;
//            string avroSchema = @"{
//	""type"": ""record"",

//    ""name"": ""sale"",
//	""namespace"": ""back"",
//	""fields"": [{
//		""name"": ""id"",
//		""type"": ""string"",
//		""doc"": ""id of the sale. Currently operaiontId or some identifier for special sales (mini site...)""
//    },
//	{
//		""name"": ""name"",
//		""type"": ""string"",
//		""doc"": ""name of the sale for display""
//	},
//	{
//		""name"": ""mediaValues"",
//		""type"": {
//			""type"": ""map"",
//			""values"": ""string""
//		},
//		""doc"": ""collection of media (external link, images, video) for dispay""
//	},
//	{
//		""name"": ""linkedCategories"",
//		""type"": {
//			""type"": ""map"",
//			""values"": ""long""
//		},
//		""doc"": ""map this sale with different category (sector, subsector, type...)""
//	}]
//}";

//            var schemaRegistry = new Mock<ISchemaRegistryClient>();
//            schemaRegistry.Setup(x => x.RegisterAsync(topic + "-value", It.IsAny<string>())).ReturnsAsync(topicId);
//            schemaRegistry.Setup(x => x.GetRegistrySubject(topic, false)).Returns(topic + "-value");
//            schemaRegistry.Setup(x => x.GetSchemaAsync(topicId)).ReturnsAsync(avroSchema);
//            int length;

//            KafkaGenericAvroSerializer serializer = new KafkaGenericAvroSerializer(schemaRegistry.Object, false);

//            var record = serializer.GenerateRecordAsync(topic, avroSchema).Result;
//            record["id"] = "test";
//            record["name"] = "currentName";

//            var mediaValues = new System.Collections.Generic.Dictionary<string, object>();
//            record["mediaValues"] = mediaValues;
//            mediaValues["test"] = "mapTest";

//            record["linkedCategories"] = new System.Collections.Generic.Dictionary<string, object>();


//            var array = serializer.Serialize(record, out length);

//            dynamic deser = serializer.Deserialize(topic, array);
//            Assert.Equal("test", deser["id"]);
//            Assert.Equal("currentName", deser["name"]);
//            Assert.Equal("mapTest", deser["mediaValues"]["test"]);
//        }

//        [Fact]
//        public void GenericWithPrimitiveType()
//        {
//            string topic = "test";
//            int topicId = 4;
//            string avroSchema = @"{""type"": ""int""}";
//            int length;

//            var schemaRegistry = new Mock<ISchemaRegistryClient>();
//            schemaRegistry.Setup(x => x.RegisterAsync(topic + "-value", It.IsAny<string>())).ReturnsAsync(topicId);
//            schemaRegistry.Setup(x => x.GetRegistrySubject(topic, false)).Returns(topic + "-value");
//            schemaRegistry.Setup(x => x.GetSchemaAsync(topicId)).ReturnsAsync(avroSchema);

//            KafkaGenericAvroSerializer serializer = new KafkaGenericAvroSerializer(schemaRegistry.Object, false);
//            var record = serializer.GenerateRecordAsync(topic, avroSchema).Result;

//            record.Value = 10;
//            var array = serializer.Serialize(record, out length);
            
//            object deser = serializer.Deserialize(topic, array);
//            Assert.Equal(10, deser);
//        }
//    }
//}
