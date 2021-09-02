using System;
using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Client;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class PublicKeyCache : IPublicKeyCache
    {
        private readonly IPublicKeyClient _publicKeyClient;
        private readonly IMemoryCache _memoryCache;

        private readonly TimeSpan _cacheTimespan = TimeSpan.FromHours(24);

        public PublicKeyCache(IPublicKeyClient publicKeyClient, IMemoryCache memoryCache)
        {
            _publicKeyClient = publicKeyClient;
            _memoryCache = memoryCache;
        }

        public async Task<PublicKey> GetPublicKeyAsync(string keyId)
        {
            var cacheKey = $"public-key:{keyId}";

            var exists = _memoryCache.TryGetValue(cacheKey, out PublicKey publicKey);
            if (exists)
                return publicKey;

            try
            {
                publicKey = await _publicKeyClient.GetPublicKeyAsync(keyId);
            }
            catch (Exception ex)
            {
                throw new PublicKeyCacheException(ex.Message);
            }

            _memoryCache.Set(cacheKey, publicKey, _cacheTimespan);

            return publicKey;
        }
    }
}