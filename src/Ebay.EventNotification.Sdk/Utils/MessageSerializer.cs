using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ebay.EventNotification.Sdk.Models;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class MessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerOptions _options;

        public MessageSerializer()
        {
            _options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            _options.Converters.Add(new EbayDateTimeConverter());
        }

        public string Serialize(Message message) => JsonSerializer.Serialize(message, _options);
        public string Serialize(object obj) => JsonSerializer.Serialize(obj, _options);
        public T Deserialize<T>(object obj) => JsonSerializer.Deserialize<T>(Serialize(obj), _options);
        public T Deserialize<T>(string message) => JsonSerializer.Deserialize<T>(message, _options);

        private class EbayDateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetDateTime();
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }
        }
    }
}