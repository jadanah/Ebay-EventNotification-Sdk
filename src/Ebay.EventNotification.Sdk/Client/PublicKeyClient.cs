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
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using eBay.ApiClient.Auth.OAuth2;
using eBay.ApiClient.Auth.OAuth2.Model;
using Ebay.EventNotification.Sdk.Config;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace Ebay.EventNotification.Sdk.Client
{
    public class PublicKeyClient : IPublicKeyClient
    {
        private readonly IEbayOAuthClient _ebayOAuthClient;

        private readonly HttpClient _httpClient;

        private readonly IEventNotificationConfig _config;

        private readonly ILogger<PublicKeyClient> _logger;

        public PublicKeyClient(IEbayOAuthClient ebayOAuthClient, HttpClient httpClient, IEventNotificationConfig configuration, ILogger<PublicKeyClient> logger)
        {
            _ebayOAuthClient = ebayOAuthClient;
            _httpClient = httpClient;
            _config = configuration;
            _logger = logger;
        }

        public async Task<PublicKey> GetPublicKeyAsync(string keyId)
        {
            try
            {
                var baseUrl = ClientConstants.GetEndPoints(_config.Environment);
                var token = await FetchTokenAsync(_config.Environment);
                _httpClient.DefaultRequestHeaders.Add(ClientConstants.Authorization, token);
                return await _httpClient.GetFromJsonAsync<PublicKey>(baseUrl + keyId);
            }
            catch (Exception ex)
            {
                var message = "Public Key retrieval failed with: " + ex.Message;
                _logger.LogError(message);
                throw new ClientException(message, ex);
            }
        }


        private async Task<string> FetchTokenAsync(string environment)
        {
            try
            {
                OAuthResponse oAuthResponse = await _ebayOAuthClient.GetApplicationTokenAsync(ClientConstants.Scopes);
                return ClientConstants.Bearer + oAuthResponse.AccessToken.Token;
            }
            catch (Exception ex)
            {
                var message = "Fetch application token failed with: " + ex.Message;
                _logger.LogError(message);
                throw new OAuthTokenException(message, ex);
            }
        }
    }
}