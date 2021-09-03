#if NETCOREAPP3_1
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
using System.Text.Json;
using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class BouncyCastleSignatureValidator : SignatureValidatorBase, ISignatureValidator
    {
        private readonly IPublicKeyCache _publicKeyCache;
        private readonly ILogger<BouncyCastleSignatureValidator> _logger;

        public BouncyCastleSignatureValidator(IPublicKeyCache publicKeyCache, IMessageSerializer serializer, ILogger<BouncyCastleSignatureValidator> logger) : base(serializer)
        {
            _publicKeyCache = publicKeyCache;
            _logger = logger;
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
                var signatureBytes = Convert.FromBase64String(xeBaySignature.Signature);

                PublicKey publicKey = await _publicKeyCache.GetPublicKeyAsync(xeBaySignature.Kid);
                var pkBytes = Convert.FromBase64String(GetRawKey(publicKey.Key));

                var messageBytes = Encoding.GetBytes(message);

                AsymmetricKeyParameter pk = PublicKeyFactory.CreateKey(pkBytes);

                var algorithm = string.Format(Constants.Constants.Algorithm, publicKey.Digest, publicKey.Algorithm);
                ISigner verifier = SignerUtilities.GetSigner(algorithm);
                verifier.Init(false, pk);
                verifier.BlockUpdate(messageBytes, 0, messageBytes.Length);

                var result = verifier.VerifySignature(signatureBytes);

                if (result == false)
                    _logger.LogError("Signature mismatch for payload: " + message + ": with signature: " + signatureHeader);
                return result;
            }
            catch (Exception ex)
            {
                throw new SignatureValidationException(ex.Message);
            }
        }
    }
}
#endif