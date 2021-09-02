using System.Text.Json;
using Ebay.EventNotification.Sdk.Models;
using Ebay.EventNotification.Sdk.Tests.data;
using Ebay.EventNotification.Sdk.Utils;
using Xunit;

namespace Ebay.EventNotification.Sdk.Tests
{
    public class SerializationTest
    {
        [Fact]
        public void Should_serialize_deserialize_message_without_change()
        {
            var serializer = new MessageSerializer();
            var messageRaw = DataProvider.GetMockMessageRaw();
            var message = JsonSerializer.Deserialize<Message>(messageRaw);

            var deserializedMessage = serializer.Serialize(message);

            Assert.Equal(messageRaw, deserializedMessage);
        }
    }
}