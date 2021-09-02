using System.Collections.Generic;
using System.Threading.Tasks;
using eBay.ApiClient.Auth.OAuth2.Model;

namespace eBay.ApiClient.Auth.OAuth2
{
    public interface IEbayOAuthClient
    {
        Task<OAuthResponse> GetApplicationTokenAsync(IEnumerable<string> scopes);
        string GenerateUserAuthorizationUrl(IEnumerable<string> scopes, string state);
        Task<OAuthResponse> ExchangeCodeForAccessTokenAsync(string code);
        Task<OAuthResponse> GetAccessTokenAsync(string refreshToken, IEnumerable<string> scopes);
    }
}