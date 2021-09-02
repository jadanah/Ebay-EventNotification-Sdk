using System;
using System.Collections.Generic;
using eBay.ApiClient.Auth.OAuth2.Model;

namespace eBay.ApiClient.Auth.OAuth2
{
    internal class AppTokenCache
    {
        private readonly Dictionary<string, AppTokenCacheModel> _envAppTokenCache =
            new Dictionary<string, AppTokenCacheModel>();

        public void UpdateValue(OAuthEnvironment environment, OAuthResponse oAuthResponse, DateTime expiresAt)
        {
            var appTokenCacheModel = new AppTokenCacheModel
            {
                OAuthResponse = oAuthResponse,

                //Setting a buffer of 5 minutes for refresh
                ExpiresAt = expiresAt.Subtract(new TimeSpan(0, 5, 0))
            };

            //Remove key if it exists
            if (_envAppTokenCache.ContainsKey(environment.ConfigIdentifier()))
            {
                _envAppTokenCache.Remove(environment.ConfigIdentifier());
            }

            _envAppTokenCache.Add(environment.ConfigIdentifier(), appTokenCacheModel);
        }

        public OAuthResponse GetValue(OAuthEnvironment environment)
        {
            var appTokenCacheModel = _envAppTokenCache.TryGetValue(environment.ConfigIdentifier(), out var value)
                ? value
                : null;

            if (appTokenCacheModel != null)
            {
                if ((appTokenCacheModel.OAuthResponse != null &&
                     appTokenCacheModel.OAuthResponse.ErrorMessage != null)
                    || (DateTime.Now.CompareTo(appTokenCacheModel.ExpiresAt) < 0))
                    return appTokenCacheModel.OAuthResponse;
            }

            //Since the value is expired, return null
            return null;
        }
    }
}