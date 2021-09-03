using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ebay.EventNotification.Sdk.Models;

namespace Ebay.EventNotification.Sdk.Tests.data
{
    public class DataProvider
    {
        private static readonly string Message = @"../../../Data/message.json";
        private static readonly string TamperedMessage = @"../../../Data/tampered_message.json";
        private static readonly string PublicKeyResponse = @"../../../Data/public_key_response.json";

        private static readonly string SignatureHeader =
            "eyJhbGciOiJlY2RzYSIsImtpZCI6Ijk5MzYyNjFhLTdkN2ItNDYyMS1hMGYxLTk2Y2NiNDI4YWY0OSIsInNpZ25hdHVyZSI6Ik1FWUNJUUNmeGZJV3V4bVdjSUJRSjljNS9YN2lHREpxczJSQ0dzQkVhQWppbnlycmZBSWhBSVY2d0djVGlCdVY1S0pVaWYyaG9reXJMK1E5c3NIa2FkK214Mm5FRTI1dyIsImRpZ2VzdCI6IlNIQTEifQ==";


        public static Message GetMockMessage()
        {
            var message = File.ReadAllText(Message);
            return JsonSerializer.Deserialize<Message>(message);
        }

        public static string GetMockMessageRaw() => JsonSerializer.Serialize(GetMockMessage(), new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        

        public static Message GetMockTamperedMessage()
        {
            var message = File.ReadAllText(TamperedMessage);
            return JsonSerializer.Deserialize<Message>(message);
        }
        
        public static string GetMockTamperedMessageRaw() => JsonSerializer.Serialize(GetMockTamperedMessage(), new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        public static PublicKey GetMockPublicKeyResponse()
        {
            var response = File.ReadAllText(PublicKeyResponse);
            return JsonSerializer.Deserialize<PublicKey>(response);
        }

        public static string GetMockXEbaySignatureHeader() => SignatureHeader;
    }
}