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

using System.Collections.Generic;
using System.IO;
using eBay.ApiClient.Auth.OAuth2.Model;
using YamlDotNet.RepresentationModel;
using System.Collections.Concurrent;

namespace eBay.ApiClient.Auth.OAuth2
{
    public static class CredentialUtil
    {
        // private static readonly ILog Log =
        //     LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ConcurrentDictionary<string, Credentials> EnvCredentials =
            new ConcurrentDictionary<string, Credentials>();

        public class Credentials
        {
            private readonly Dictionary<CredentialType, string> _credentialTypeLookup =
                new Dictionary<CredentialType, string>();

            public Credentials(YamlMappingNode keyValuePairs)
            {
                foreach (var keyValuePair in keyValuePairs.Children)
                {
                    var credentialType = CredentialType.LookupByConfigIdentifier(keyValuePair.Key.ToString());
                    if (credentialType != null)
                    {
                        _credentialTypeLookup.Add(credentialType, keyValuePair.Value.ToString());
                    }
                }
            }

            public string Get(CredentialType credentialType) => _credentialTypeLookup[credentialType];
        }

        /*
         * Loading StreamReader
         */
        public static void Load(string yamlFile)
        {
            //Stream the input file
            var streamReader = new StreamReader(yamlFile);
            Load(streamReader);
        }

        /*
         * Loading YAML file
         */
        public static void Load(StreamReader streamReader)
        {
            //Load the stream
            var yaml = new YamlStream();
            yaml.Load(streamReader);

            // Parse the stream
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var firstLevelNode in rootNode.Children)
            {
                var environment = OAuthEnvironment.LookupByConfigIdentifier(((YamlScalarNode)firstLevelNode.Key).Value);
                if (environment == null)
                    continue;

                foreach (var node in firstLevelNode.Value.AllNodes)
                {
                    if (node is YamlMappingNode)
                    {
                        var credentials = new Credentials((YamlMappingNode)node);
                        EnvCredentials[environment.ConfigIdentifier()] = credentials;
                    }
                }
            }

            // Log.Info("Loaded configuration for eBay oAuth Token");
        }

        /*
         * Get Credentials based on Environment
         */
        public static Credentials GetCredentials(OAuthEnvironment environment)
        {
            return EnvCredentials.TryGetValue(environment.ConfigIdentifier(), out var credentials) ? credentials : null;
        }
    }
}