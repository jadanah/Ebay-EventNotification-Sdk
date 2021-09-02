using System;
using System.Collections.Generic;
using eBay.ApiClient.Auth.OAuth2.Model;

namespace eBay.ApiClient.Auth.OAuth2
{
    [Obsolete("Replaced with IEbayOAuthClient")]
    public interface IOAuth2Api
    {
        OAuthResponse GetApplicationToken(OAuthEnvironment environment, IList<string> scopes);
        string GenerateUserAuthorizationUrl(OAuthEnvironment environment, IList<string> scopes, string state);
        OAuthResponse ExchangeCodeForAccessToken(OAuthEnvironment environment, string code);
        OAuthResponse GetAccessToken(OAuthEnvironment environment, string refreshToken, IList<string> scopes);
    }
}