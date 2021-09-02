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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using eBay.ApiClient.Auth.OAuth2.Model;

namespace eBay.ApiClient.Auth.OAuth2
{
    public static class OAuth2Util
    {
        /*
         * Format scopes for request
         */
        public static string FormatScopesForRequest(IEnumerable<string> scopes)
        {
            string scopesForRequest = null;
            if (scopes == null || !scopes.Any())
                return scopesForRequest;

            foreach (var scope in scopes)
                scopesForRequest = scopesForRequest == null ? scope : scopesForRequest + "+" + scope;

            return scopesForRequest;
        }

        /*
         * Create Base64 encoded Authorization header value
         */
        public static string CreateAuthorizationHeader(CredentialUtil.Credentials credentials)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(credentials.Get(CredentialType.APP_ID)).Append(Constants.CREDENTIAL_DELIMITER);
            stringBuilder.Append(credentials.Get(CredentialType.CERT_ID));
            var plainTextBytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            var encodedText = Convert.ToBase64String(plainTextBytes);
            return Constants.HEADER_PREFIX_BASIC + encodedText;
        }

        /*
         * Create request payload for input parameters and values
         */
        public static string CreateRequestPayload(Dictionary<string, string> payloadParams)
        {
            var sb = new StringBuilder();
            foreach (var entry in payloadParams)
            {
                if (sb.Length > 0)
                    sb.Append(Constants.PAYLOAD_PARAM_DELIMITER);

                sb.Append(entry.Key).Append(Constants.PAYLOAD_VALUE_DELIMITER).Append(entry.Value);
            }

            return sb.ToString();
        }
    }
}