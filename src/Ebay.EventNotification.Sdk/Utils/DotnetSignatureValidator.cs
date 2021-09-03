#if NET5_0_OR_GREATER

using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class DotnetSignatureValidator : SignatureValidatorBase, ISignatureValidator
    {
        private readonly IPublicKeyCache _publicKeyCache;
        private readonly ILogger<DotnetSignatureValidator> _logger;

        public DotnetSignatureValidator(IPublicKeyCache publicKeyCache, IMessageSerializer serializer, ILogger<DotnetSignatureValidator> logger) : base(serializer)
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

                var publicKey = await _publicKeyCache.GetPublicKeyAsync(xeBaySignature.Kid);
                var pkBytes = Convert.FromBase64String(GetRawKey(publicKey.Key));

                var messageBytes = Encoding.GetBytes(message);

                using var ecdsa = ECDsa.Create();
                if (ecdsa == null) return false;
                
                ecdsa.ImportSubjectPublicKeyInfo(pkBytes, out _);

                var algorithm = string.Format(Constants.Constants.Algorithm, publicKey.Digest, publicKey.Algorithm);
                var result = ecdsa.VerifyData(messageBytes, signatureBytes, new HashAlgorithmName(publicKey.Digest), DSASignatureFormat.Rfc3279DerSequence);

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