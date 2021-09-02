using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using eBay.ApiClient.Auth.OAuth2.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace eBay.ApiClient.Auth.OAuth2
{
    public class EbayOAuthClient : IEbayOAuthClient
    {
        private readonly ILogger<EbayOAuthClient> _logger;

        private readonly OAuthEnvironment _environment;

        private static readonly AppTokenCache AppTokenCacheInternal = new AppTokenCache();

        public EbayOAuthClient(ILogger<EbayOAuthClient> logger, OAuthEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        private void ValidateEnvironmentAndScopes(OAuthEnvironment environment, IEnumerable<string> scopes)
        {
            ValidateInput("Environment", environment);
            ValidateScopes(scopes);
        }

        private void ValidateInput(string key, object value)
        {
            if (value == null)
                throw new ArgumentException(key + " can't be null");
        }

        private void ValidateScopes(IEnumerable<string> scopes)
        {
            if (scopes == null || !scopes.Any())
                throw new ArgumentException("Scopes can't be null/empty");
        }

        private async Task<OAuthResponse> FetchTokenAsync(OAuthEnvironment environment, string requestPayload,
            TokenType tokenType)
        {
            //Get credentials
            var credentials = GetCredentials(environment);

            //Initialize client
            var client = new RestClient
            {
                BaseUrl = new Uri(environment.ApiEndpoint())
            };

            //Create request
            var request = new RestRequest(Method.POST);

            //Add headers
            request.AddHeader(Constants.HEADER_AUTHORIZATION, OAuth2Util.CreateAuthorizationHeader(credentials));

            //Set request payload
            request.AddParameter(Constants.HEADER_CONTENT_TYPE, requestPayload, ParameterType.RequestBody);

            //Call the API
            var response = await client.ExecutePostAsync(request);

            //Parse response
            var oAuthResponse = HandleApiResponse(response, tokenType);

            return oAuthResponse;
        }


        private CredentialUtil.Credentials GetCredentials(OAuthEnvironment environment)
        {
            var credentials = CredentialUtil.GetCredentials(environment);
            if (credentials == null)
                throw new ArgumentException("Credentials have not been loaded for " + environment.ConfigIdentifier());

            return credentials;
        }


        private OAuthResponse HandleApiResponse(IRestResponse response, TokenType tokenType)
        {
            var oAuthResponse = new OAuthResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                oAuthResponse.ErrorMessage = response.Content;
                _logger.LogError("Error in fetching the token. Error:" + oAuthResponse.ErrorMessage);
            }
            else
            {
                var apiResponse = JsonConvert.DeserializeObject<OAuthApiResponse>(response.Content);

                if (apiResponse != null)
                {
                    //Set AccessToken
                    var accessToken = new OAuthToken
                    {
                        Token = apiResponse.AccessToken,
                        ExpiresOn = DateTime.Now.Add(new TimeSpan(0, 0, apiResponse.ExpiresIn)),
                        TokenType = tokenType
                    };
                    oAuthResponse.AccessToken = accessToken;

                    //Set Refresh Token
                    if (apiResponse.RefreshToken != null)
                    {
                        var refreshToken = new OAuthToken
                        {
                            Token = apiResponse.RefreshToken,
                            ExpiresOn = DateTime.Now.Add(new TimeSpan(0, 0, apiResponse.RefreshTokenExpiresIn)),
                        };
                        oAuthResponse.RefreshToken = refreshToken;
                    }
                }
            }

            _logger.LogInformation("Fetched the token successfully from API");
            return oAuthResponse;
        }

        /// <summary>
        ///     Use this operation to get an OAuth access token using a client credentials grant.
        ///     The access token retrieved from this process is called an Application access token. 
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<OAuthResponse> GetApplicationTokenAsync(IEnumerable<string> scopes)
        {
            //Validate request
            ValidateEnvironmentAndScopes(_environment, scopes);
            OAuthResponse oAuthResponse = null;

            //Check for token in cache
            if (AppTokenCacheInternal != null)
            {
                oAuthResponse = AppTokenCacheInternal.GetValue(_environment);
                if (oAuthResponse != null && oAuthResponse.AccessToken != null &&
                    oAuthResponse.AccessToken.Token != null)
                {
                    _logger.LogInformation("Returning token from cache for " + _environment.ConfigIdentifier());
                    return oAuthResponse;
                }
            }

            //App token not in cache, fetch it and set into cache
            var formattedScopes = OAuth2Util.FormatScopesForRequest(scopes);

            //Prepare request payload
            var payloadParams = new Dictionary<string, string>
            {
                { Constants.PAYLOAD_GRANT_TYPE, Constants.PAYLOAD_VALUE_CLIENT_CREDENTIALS },
                { Constants.PAYLOAD_SCOPE, formattedScopes }
            };
            var requestPayload = OAuth2Util.CreateRequestPayload(payloadParams);

            oAuthResponse = await FetchTokenAsync(_environment, requestPayload, TokenType.APPLICATION);
            //Update value in cache
            if (oAuthResponse != null && oAuthResponse.AccessToken != null)
            {
                AppTokenCacheInternal?.UpdateValue(_environment, oAuthResponse, oAuthResponse.AccessToken.ExpiresOn);
            }

            return oAuthResponse;
        }

        /// <summary>
        /// Use this operation to get the Authorization URL to redirect the user to.
        /// Once the user authenticates and approves the consent, the callback need to be
        /// captured by the redirect URL setup by the app 
        /// </summary>
        /// <param name="scopes"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GenerateUserAuthorizationUrl(IEnumerable<string> scopes, string state)
        {
            //Validate request
            ValidateEnvironmentAndScopes(_environment, scopes);

            //Get credentials
            var credentials = GetCredentials(_environment);

            //Format scopes
            var formattedScopes = OAuth2Util.FormatScopesForRequest(scopes);

            //Prepare URL
            var sb = new StringBuilder();
            sb.Append(_environment.WebEndpoint()).Append("?");

            //Prepare request payload
            var queryParams = new Dictionary<string, string>
            {
                { Constants.PAYLOAD_CLIENT_ID, credentials.Get(CredentialType.APP_ID) },
                { Constants.PAYLOAD_RESPONSE_TYPE, Constants.PAYLOAD_VALUE_CODE },
                { Constants.PAYLOAD_REDIRECT_URI, credentials.Get(CredentialType.REDIRECT_URI) },
                { Constants.PAYLOAD_SCOPE, formattedScopes }
            };

            if (state != null)
                queryParams.Add(Constants.PAYLOAD_STATE, state);

            sb.Append(OAuth2Util.CreateRequestPayload(queryParams));

            _logger.LogDebug("Authorization url " + sb);
            return sb.ToString();
        }

        /// <summary>
        /// Use this operation to get the refresh and access tokens.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<OAuthResponse> ExchangeCodeForAccessTokenAsync(string code)
        {
            //Validate request
            ValidateInput("Environment", _environment);
            ValidateInput("Code", code);

            //Get credentials
            var credentials = GetCredentials(_environment);

            // Create request payload
            var payloadParams = new Dictionary<string, string>
            {
                { Constants.PAYLOAD_GRANT_TYPE, Constants.PAYLOAD_VALUE_AUTHORIZATION_CODE },
                { Constants.PAYLOAD_REDIRECT_URI, credentials.Get(CredentialType.REDIRECT_URI) },
                { Constants.PAYLOAD_CODE, code }
            };
            var requestPayload = OAuth2Util.CreateRequestPayload(payloadParams);

            var oAuthResponse = await FetchTokenAsync(_environment, requestPayload, TokenType.USER);
            return oAuthResponse;
        }

        /// <summary>
        /// Use this operation to update the access token if it has expired
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<OAuthResponse> GetAccessTokenAsync(string refreshToken, IEnumerable<string> scopes)
        {
            //Validate request
            ValidateEnvironmentAndScopes(_environment, scopes);
            ValidateInput("RefreshToken", refreshToken);

            //Get credentials
            var credentials = GetCredentials(_environment);

            //Format scopes
            var formattedScopes = OAuth2Util.FormatScopesForRequest(scopes);

            // Create request payload
            var payloadParams = new Dictionary<string, string>
            {
                { Constants.PAYLOAD_GRANT_TYPE, Constants.PAYLOAD_VALUE_REFRESH_TOKEN },
                { Constants.PAYLOAD_REFRESH_TOKEN, refreshToken },
                { Constants.PAYLOAD_SCOPE, formattedScopes }
            };
            var requestPayload = OAuth2Util.CreateRequestPayload(payloadParams);

            var oAuthResponse = await FetchTokenAsync(_environment, requestPayload, TokenType.USER);
            return oAuthResponse;
        }
    }
}