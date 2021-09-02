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

namespace eBay.ApiClient.Auth.OAuth2
{
    public static class Constants
    {
        //Config
        public static readonly string DEV_ID = "devid";
        public static readonly string APP_ID = "appid";
        public static readonly string CERT_ID = "certid";
        public static readonly string REDIRECT_URI = "redirecturi";
        public static readonly string CREDENTIAL_DELIMITER = ":";

        //API request headers
        public static readonly string HEADER_AUTHORIZATION = "Authorization";
        public static readonly string HEADER_PREFIX_BASIC = "Basic ";
        public static readonly string HEADER_CONTENT_TYPE = "application/x-www-form-urlencoded";

        //API request payload
        public static readonly string PAYLOAD_GRANT_TYPE = "grant_type";
        public static readonly string PAYLOAD_CLIENT_ID = "client_id";
        public static readonly string PAYLOAD_RESPONSE_TYPE = "response_type";
        public static readonly string PAYLOAD_REDIRECT_URI = "redirect_uri";
        public static readonly string PAYLOAD_SCOPE = "scope";
        public static readonly string PAYLOAD_STATE = "state";
        public static readonly string PAYLOAD_CODE = "code";
        public static readonly string PAYLOAD_REFRESH_TOKEN = "refresh_token";
        public static readonly string PAYLOAD_VALUE_CLIENT_CREDENTIALS = "client_credentials";
        public static readonly string PAYLOAD_VALUE_CODE = "code";
        public static readonly string PAYLOAD_VALUE_AUTHORIZATION_CODE = "authorization_code";
        public static readonly string PAYLOAD_VALUE_REFRESH_TOKEN = "refresh_token";
        public static readonly string PAYLOAD_PARAM_DELIMITER = "&";
        public static readonly string PAYLOAD_VALUE_DELIMITER = "=";
    }
}