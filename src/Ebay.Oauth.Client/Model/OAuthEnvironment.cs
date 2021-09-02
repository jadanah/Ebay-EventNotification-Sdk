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

namespace eBay.ApiClient.Auth.OAuth2.Model
{
    public class OAuthEnvironment
    {
        public static readonly OAuthEnvironment PRODUCTION = new OAuthEnvironment("api.ebay.com",
            "https://auth.ebay.com/oauth2/authorize", "https://api.ebay.com/identity/v1/oauth2/token");

        public static readonly OAuthEnvironment SANDBOX = new OAuthEnvironment("api.sandbox.ebay.com",
            "https://auth.sandbox.ebay.com/oauth2/authorize", "https://api.sandbox.ebay.com/identity/v1/oauth2/token");

        private readonly string _configIdentifier;
        private readonly string _webEndpoint;
        private readonly string _apiEndpoint;

        private OAuthEnvironment(string configIdentifier, string webEndpoint, string apiEndpoint)
        {
            _configIdentifier = configIdentifier;
            _webEndpoint = webEndpoint;
            _apiEndpoint = apiEndpoint;
        }

        public string ConfigIdentifier() => _configIdentifier;

        public string WebEndpoint() => _webEndpoint;

        public string ApiEndpoint() => _apiEndpoint;

        /*
         * Lookup by ConfigIdentifier
         */
        public static OAuthEnvironment LookupByConfigIdentifier(string configIdentifier)
        {
            if (PRODUCTION.ConfigIdentifier().Equals(configIdentifier))
                return PRODUCTION;

            if (SANDBOX.ConfigIdentifier().Equals(configIdentifier))
                return SANDBOX;

            return null;
        }
    }
}