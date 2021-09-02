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
using Xunit;
using System.Collections.Generic;
using eBay.ApiClient.Auth.OAuth2.Model;
using System.IO;
using OpenQA.Selenium;
using System.Threading;
using OpenQA.Selenium.Chrome;
using System.Web;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using YamlDotNet.RepresentationModel;

namespace eBay.ApiClient.Auth.OAuth2
{
    public class OAuth2ApiTest : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        private static readonly Mock<ILogger<OAuth2Api>> MockOauth2ApiLogger = new Mock<ILogger<OAuth2Api>>();

        private readonly OAuth2Api _oAuth2Api = new OAuth2Api(MockOauth2ApiLogger.Object);

        private readonly IList<string> _scopes = new List<string>()
        {
            "https://api.ebay.com/oauth/api_scope/buy.marketing",
            "https://api.ebay.com/oauth/api_scope"
        };

        private readonly IList<string> _userScopes = new List<string>()
        {
            "https://api.ebay.com/oauth/api_scope/commerce.catalog.readonly",
            "https://api.ebay.com/oauth/api_scope/buy.shopping.cart"
        };

        public OAuth2ApiTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            LoadCredentials();
        }

        public void Dispose()
        {
            // clean up test data here
        }

        [Fact]
        public void GetApplicationToken_Production_Success()
        {
            GetApplicationToken_Success(OAuthEnvironment.PRODUCTION);
        }

        [Fact]
        public void GetApplicationToken_Sandbox_Success()
        {
            GetApplicationToken_Success(OAuthEnvironment.SANDBOX);
        }

        [Fact]
        public void GetApplicationToken_ProductionCache_Success()
        {
            GetApplicationToken_Success(OAuthEnvironment.PRODUCTION);
        }

        [Fact]
        public void GetApplicationToken_SandboxCache_Success()
        {
            GetApplicationToken_Success(OAuthEnvironment.SANDBOX);
        }

        [Fact]
        public void GetApplicationToken_NullEnvironment_Failure()
        {
            Assert.Throws<ArgumentException>(() => _oAuth2Api.GetApplicationToken(null, _scopes));
        }

        [Fact]
        public void GetApplicationToken_NullScopes_Failure()
        {
            Assert.Throws<ArgumentException>(() => _oAuth2Api.GetApplicationToken(OAuthEnvironment.PRODUCTION, null));
        }

        [Fact]
        public void GenerateUserAuthorizationUrl_Success()
        {
            var yamlFile = @"../../../ebay-config-sample.yaml";
            var streamReader = new StreamReader(yamlFile);
            CredentialUtil.Load(streamReader);

            var state = "State";
            var authorizationUrl =
                _oAuth2Api.GenerateUserAuthorizationUrl(OAuthEnvironment.PRODUCTION, _userScopes, state);
            _testOutputHelper.WriteLine("======================GenerateUserAuthorizationUrl======================");
            _testOutputHelper.WriteLine("AuthorizationUrl => " + authorizationUrl);
            Assert.NotNull(authorizationUrl);
        }

        [Fact]
        public void GenerateUserAuthorizationUrl_NullEnvironment_Failure()
        {
            Assert.Throws<ArgumentException>(() => _oAuth2Api.GenerateUserAuthorizationUrl(null, _scopes, null));
        }

        [Fact]
        public void GenerateUserAuthorizationUrl_NullScopes_Failure()
        {
            Assert.Throws<ArgumentException>(() =>
                _oAuth2Api.GenerateUserAuthorizationUrl(OAuthEnvironment.PRODUCTION, null, null));
        }

        [Fact]
        public void ExchangeCodeForAccessToken_Success()
        {
            var environment = OAuthEnvironment.PRODUCTION;
            var code = "v^1.1**********************jYw";
            var oAuthResponse = _oAuth2Api.ExchangeCodeForAccessToken(environment, code);
            Assert.NotNull(oAuthResponse);
            PrintOAuthResponse(environment, "ExchangeCodeForAccessToken", oAuthResponse);
        }

        [Fact]
        public void ExchangeCodeForAccessToken_NullEnvironment_Failure()
        {
            var code = "v^1.1*********************MjYw";
            Assert.Throws<ArgumentException>(() => _oAuth2Api.ExchangeCodeForAccessToken(null, code));
        }

        [Fact]
        public void ExchangeCodeForAccessToken_NullCode_Failure()
        {
            Assert.Throws<ArgumentException>(() =>
                _oAuth2Api.ExchangeCodeForAccessToken(OAuthEnvironment.PRODUCTION, null));
        }

        [Fact]
        public void GetAccessToken_Success()
        {
            var environment = OAuthEnvironment.PRODUCTION;
            var refreshToken = "v^1.1*****************I2MA==";
            var oAuthResponse = _oAuth2Api.GetAccessToken(environment, refreshToken, _userScopes);
            Assert.NotNull(oAuthResponse);
            PrintOAuthResponse(environment, "GetAccessToken", oAuthResponse);
        }

        [Fact]
        public void GetAccessToken_EndToEnd_Production()
        {
            _testOutputHelper.WriteLine("======================GetAccessToken_EndToEnd_Production======================");
            GetAccessToken_EndToEnd(OAuthEnvironment.PRODUCTION);
        }

        [Fact]
        public void GetAccessToken_EndToEnd_Sandbox()
        {
            _testOutputHelper.WriteLine("======================GetAccessToken_EndToEnd_Sandbox======================");
            GetAccessToken_EndToEnd(OAuthEnvironment.SANDBOX);
        }


        private void GetApplicationToken_Success(OAuthEnvironment environment)
        {
            var oAuthResponse = _oAuth2Api.GetApplicationToken(environment, _scopes);
            Assert.NotNull(oAuthResponse);
            PrintOAuthResponse(environment, "GetApplicationToken", oAuthResponse);
        }

        private void LoadCredentials()
        {
            var path = @"../../../ebay-config-sample.yaml";
            CredentialUtil.Load(path);
        }

        private void PrintOAuthResponse(OAuthEnvironment environment, string methodName, OAuthResponse oAuthResponse)
        {
            _testOutputHelper.WriteLine("======================" + methodName + "======================");
            _testOutputHelper.WriteLine("Environment=> " + environment.ConfigIdentifier() + ", ErroMessage=> " +
                                        oAuthResponse.ErrorMessage);
            if (oAuthResponse.AccessToken != null)
            {
                _testOutputHelper.WriteLine("AccessToken=> " + oAuthResponse.AccessToken.Token);
            }

            if (oAuthResponse.RefreshToken != null)
            {
                _testOutputHelper.WriteLine("RefreshToken=> " + oAuthResponse.RefreshToken.Token);
            }
        }

        private void GetAccessToken_EndToEnd(OAuthEnvironment environment)
        {
            //Load user credentials
            var userCredential = ReadUserNamePassword(environment);
            if ("<sandbox-username>".Equals(userCredential.UserName) ||
                "<production-username>".Equals(userCredential.UserName) ||
                "<sandbox-user-password>".Equals(userCredential.Pwd) ||
                "<production-user-password>".Equals(userCredential.Pwd))
            {
                _testOutputHelper.WriteLine("User name and password are not specified in test-config-sample.yaml");
                return;
            }

            var authorizationUrl = _oAuth2Api.GenerateUserAuthorizationUrl(environment, _userScopes, null);
            _testOutputHelper.WriteLine("AuthorizationUrl => " + authorizationUrl);
            var authorizationCode = GetAuthorizationCode(authorizationUrl, userCredential);
            _testOutputHelper.WriteLine("AuthorizationCode => " + authorizationCode);
            var oAuthResponse = _oAuth2Api.ExchangeCodeForAccessToken(environment, authorizationCode);
            Assert.NotNull(oAuthResponse);
            Assert.NotNull(oAuthResponse.RefreshToken);
            var refreshToken = oAuthResponse.RefreshToken.Token;
            _testOutputHelper.WriteLine("RefreshToken=> " + refreshToken);
            oAuthResponse = _oAuth2Api.GetAccessToken(environment, refreshToken, _userScopes);
            Assert.NotNull(oAuthResponse);
            Assert.NotNull(oAuthResponse.AccessToken);
            _testOutputHelper.WriteLine("AccessToken=> " + oAuthResponse.AccessToken.Token);
        }


        private UserCredential ReadUserNamePassword(OAuthEnvironment environment)
        {
            var userCredential = new UserCredential();
            var yaml = new YamlStream();
            var streamReader = new StreamReader("../../../test-config-sample.yaml");
            yaml.Load(streamReader);
            var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
            foreach (var firstLevelNode in rootNode.Children)
            {
                foreach (var node in firstLevelNode.Value.AllNodes)
                {
                    var configEnvironment = ((YamlScalarNode)firstLevelNode.Key).Value;
                    if ((environment.ConfigIdentifier().Equals(OAuthEnvironment.PRODUCTION.ConfigIdentifier()) &&
                         "sandbox-user".Equals(configEnvironment))
                        || (environment.ConfigIdentifier().Equals(OAuthEnvironment.SANDBOX.ConfigIdentifier()) &&
                            "production-user".Equals(configEnvironment)))
                    {
                        continue;
                    }

                    if (node is YamlMappingNode)
                    {
                        foreach (var keyValuePair in ((YamlMappingNode)node).Children)
                        {
                            if ("username".Equals(keyValuePair.Key.ToString()))
                            {
                                userCredential.UserName = keyValuePair.Value.ToString();
                            }
                            else
                            {
                                userCredential.Pwd = keyValuePair.Value.ToString();
                            }
                        }
                    }
                }
            }

            return userCredential;
        }

        private string GetAuthorizationCode(string authorizationUrl, UserCredential userCredential)
        {
            IWebDriver driver = new ChromeDriver("./");

            //Submit login form
            driver.Navigate().GoToUrl(authorizationUrl);
            var userId = driver.FindElement(By.Id("userid"));
            var password = driver.FindElement(By.Id("pass"));
            var submit = driver.FindElement(By.Id("sgnBt"));
            userId.SendKeys(userCredential.UserName);
            password.SendKeys(userCredential.Pwd);
            submit.Click();

            //Wait for success page
            Thread.Sleep(2000);

            var successUrl = driver.Url;

            //Handle consent
            if (successUrl.Contains("/consents"))
            {
                var consent = driver.FindElement(By.Id("submit"));
                consent.Click();
                Thread.Sleep(2000);
                successUrl = driver.Url;
            }

            var iqs = successUrl.IndexOf('?');
            var querystring = (iqs < successUrl.Length - 1) ? successUrl.Substring(iqs + 1) : string.Empty;
            // Parse the query string variables into a NameValueCollection.
            var queryParams = HttpUtility.ParseQueryString(querystring);
            var code = queryParams.Get("code");
            driver.Quit();

            return code;
        }
    }
}