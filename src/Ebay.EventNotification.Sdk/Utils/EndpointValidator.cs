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
using System.Security.Cryptography;
using System.Text;
using Ebay.EventNotification.Sdk.Config;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;

namespace Ebay.EventNotification.Sdk.Utils
{
    public class EndpointValidator : IEndPointValidator
    {
        private readonly IEventNotificationConfig _config;

        public EndpointValidator(IEventNotificationConfig configuration)
        {
            _config = configuration;
        }

        public ChallengeResponse GenerateChallengeResponse(string challengeCode)
        {
            if (string.IsNullOrEmpty(_config.Endpoint) || string.IsNullOrEmpty(_config.VerificationToken))
            {
                throw new MissingEndpointValidationConfig("Endpoint and verificationToken is required");
            }

            try
            {
                var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                sha256.AppendData(Encoding.UTF8.GetBytes(challengeCode));
                sha256.AppendData(Encoding.UTF8.GetBytes(_config.VerificationToken));
                sha256.AppendData(Encoding.UTF8.GetBytes(_config.Endpoint));
                var bytes = sha256.GetHashAndReset();
                return new ChallengeResponse(BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower());
            }
            catch (Exception ex)
            {
                throw new EndpointValidationException("End point validation failed", ex);
            }
        }
    }
}