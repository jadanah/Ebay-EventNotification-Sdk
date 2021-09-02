/*
 * *
 *  * Copyright 2019 eBay Inc.
 *  *
 *  * Licensed under the Apache License, Version 2.0 (the "License");
 *  * you may not use this file except in compliance with the License.
 *  * You may obtain a copy of the License at
 *  *
 *  *  http://www.apache.org/licenses/LICENSE-2.0
 *  *
 *  * Unless required by applicable law or agreed to in writing, software
 *  * distributed under the License is distributed on an "AS IS" BASIS,
 *  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  * See the License for the specific language governing permissions and
 *  * limitations under the License.
 *  *
 */

using System;
using System.Collections.Generic;
using eBay.ApiClient.Auth.OAuth2.Model;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;

namespace eBay.ApiClient.Auth.OAuth2
{
    [Obsolete("Replaced with EbayOAuthClient")]
    public class OAuth2Api : IOAuth2Api
    {
        private readonly ILogger<OAuth2Api> _logger;
        // private static readonly ILog Log =
        //     LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly AppTokenCache AppTokenCacheInternal = new AppTokenCache();

        public OAuth2Api(ILogger<OAuth2Api> logger)
        {
            _logger = logger;
        }

        /*
         * Use this operation to get an OAuth access token using a client credentials grant. 
         * The access token retrieved from this process is called an Application access token. 
         */
        public OAuthResponse GetApplicationToken(OAuthEnvironment environment, IList<string> scopes)
        {
            //Validate request
            ValidateEnvironmentAndScopes(environment, scopes);
            OAuthResponse oAuthResponse = null;

            //Check for token in cache
            if (AppTokenCacheInternal != null)
            {
                oAuthResponse = AppTokenCacheInternal.GetValue(environment);
                if (oAuthResponse != null && oAuthResponse.AccessToken != null &&
                    oAuthResponse.AccessToken.Token != null)
                {
                    _logger.LogInformation("Returning token from cache for " + environment.ConfigIdentifier());
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

            oAuthResponse = FetchToken(environment, requestPayload, TokenType.APPLICATION);
            //Update value in cache
            if (oAuthResponse != null && oAuthResponse.AccessToken != null)
            {
                AppTokenCacheInternal?.UpdateValue(environment, oAuthResponse, oAuthResponse.AccessToken.ExpiresOn);
            }

            return oAuthResponse;
        }

        /*
         * Use this operation to get the Authorization URL to redirect the user to. 
         * Once the user authenticates and approves the consent, the callback need to be 
         * captured by the redirect URL setup by the app 
         */
        public string GenerateUserAuthorizationUrl(OAuthEnvironment environment, IList<string> scopes, string state)
        {
            //Validate request
            ValidateEnvironmentAndScopes(environment, scopes);

            //Get credentials
            var credentials = GetCredentials(environment);

            //Format scopes
            var formattedScopes = OAuth2Util.FormatScopesForRequest(scopes);

            //Prepare URL
            var sb = new StringBuilder();
            sb.Append(environment.WebEndpoint()).Append("?");

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

        /*
         * Use this operation to get the refresh and access tokens.
         */
        public OAuthResponse ExchangeCodeForAccessToken(OAuthEnvironment environment, string code)
        {
            //Validate request
            ValidateInput("Environment", environment);
            ValidateInput("Code", code);

            //Get credentials
            var credentials = GetCredentials(environment);

            // Create request payload
            var payloadParams = new Dictionary<string, string>
            {
                { Constants.PAYLOAD_GRANT_TYPE, Constants.PAYLOAD_VALUE_AUTHORIZATION_CODE },
                { Constants.PAYLOAD_REDIRECT_URI, credentials.Get(CredentialType.REDIRECT_URI) },
                { Constants.PAYLOAD_CODE, code }
            };
            var requestPayload = OAuth2Util.CreateRequestPayload(payloadParams);

            var oAuthResponse = FetchToken(environment, requestPayload, TokenType.USER);
            return oAuthResponse;
        }

        /*
         * Use this operation to update the access token if it has expired
         */
        public OAuthResponse GetAccessToken(OAuthEnvironment environment, string refreshToken, IList<string> scopes)
        {
            //Validate request
            ValidateEnvironmentAndScopes(environment, scopes);
            ValidateInput("RefreshToken", refreshToken);

            //Get credentials
            var credentials = GetCredentials(environment);

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

            var oAuthResponse = FetchToken(environment, requestPayload, TokenType.USER);
            return oAuthResponse;
        }

        private void ValidateEnvironmentAndScopes(OAuthEnvironment environment, IList<string> scopes)
        {
            ValidateInput("Environment", environment);
            ValidateScopes(scopes);
        }

        private void ValidateInput(string key, object value)
        {
            if (value == null)
                throw new ArgumentException(key + " can't be null");
        }

        private void ValidateScopes(IList<string> scopes)
        {
            if (scopes == null || scopes.Count == 0)
                throw new ArgumentException("Scopes can't be null/empty");
        }

        private OAuthResponse FetchToken(OAuthEnvironment environment, string requestPayload, TokenType tokenType)
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
            var response = client.Execute(request);

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

            _logger.LogInformation("Fetched the token successfully from API");
            return oAuthResponse;
        }
    }
}