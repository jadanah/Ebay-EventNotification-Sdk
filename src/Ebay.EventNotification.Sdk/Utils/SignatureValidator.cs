/*
 * Copyright (c) 2021 eBay Inc.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class SignatureValidator : ISignatureValidator
    {
        private readonly IPublicKeyCache _publicKeyCache;
        private readonly IMessageSerializer _serializer;

        private readonly ILogger<SignatureValidator> _logger;
        private readonly Encoding _encoding;


        public SignatureValidator(IPublicKeyCache publicKeyCache, IMessageSerializer serializer, ILogger<SignatureValidator> logger)
        {
            _publicKeyCache = publicKeyCache;
            _serializer = serializer;
            _logger = logger;
            
            // Content-Type: application/json; charset=UTF-8
            _encoding = Encoding.UTF8;
        }


        public async Task<bool> ValidateAsync(Message message, string signatureHeader) => await ValidateAsync(GetJsonString(message), signatureHeader);

        public async Task<bool> ValidateAsync(string message, string signatureHeader)
        {
            try
            {
                var jsonString = DecodeBase64(signatureHeader);
                var xeBaySignature = JsonSerializer.Deserialize<XeBaySignature>(jsonString);
                if (xeBaySignature == null)
                    throw new NullReferenceException();
                
                PublicKey publicKey = await _publicKeyCache.GetPublicKeyAsync(xeBaySignature.Kid);
                var plainTextBytes = _encoding.GetBytes(message);

                AsymmetricKeyParameter pk = PublicKeyFactory.CreateKey(Convert.FromBase64String(GetRawKey(publicKey.Key)));
                ISigner verifier = SignerUtilities.GetSigner(string.Format(Constants.Constants.Algorithm, publicKey.Digest, publicKey.Algorithm));
                verifier.Init(false, pk);
                verifier.BlockUpdate(plainTextBytes, 0, plainTextBytes.Length);
                var result = verifier.VerifySignature(Convert.FromBase64String(xeBaySignature.Signature));

                if (result == false)
                    _logger.LogError("Signature mismatch for payload: " + message + ": with signature: " + signatureHeader);
                return result;
            }
            catch (Exception ex)
            {
                throw new SignatureValidationException(ex.Message);
            }
        }

        private string GetJsonString(Message message) => _serializer.Serialize(message);


        private string GetRawKey(string key)
        {
            var regex = new Regex(Constants.Constants.KeyPattern);
            Match match = regex.Match(key);
            return match.Success ? match.Groups[1].Value : key;
        }

        private string DecodeBase64(string value)
        {
            var valueBytes = Convert.FromBase64String(value);
            return _encoding.GetString(valueBytes);
        }
    }
}