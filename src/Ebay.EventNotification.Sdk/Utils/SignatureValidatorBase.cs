using System;
using System.Text;
using System.Text.RegularExpressions;
using Ebay.EventNotification.Sdk.Models;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class SignatureValidatorBase
    {
        private readonly IMessageSerializer _serializer;

        protected readonly Encoding Encoding;

        protected SignatureValidatorBase(IMessageSerializer serializer)
        {
            _serializer = serializer;

            // Content-Type: application/json; charset=UTF-8
            Encoding = Encoding.UTF8;
        }

        protected string GetJsonString(Message message) => _serializer.Serialize(message);

        protected string GetRawKey(string key)
        {
            var regex = new Regex(Constants.Constants.KeyPattern);
            Match match = regex.Match(key);
            return match.Success ? match.Groups[1].Value : key;
        }

        protected string DecodeBase64(string value)
        {
            var valueBytes = Convert.FromBase64String(value);
            return Encoding.GetString(valueBytes);
        }
    }
}