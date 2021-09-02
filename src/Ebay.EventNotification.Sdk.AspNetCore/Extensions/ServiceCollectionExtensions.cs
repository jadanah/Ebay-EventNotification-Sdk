using eBay.ApiClient.Auth.OAuth2;
using eBay.ApiClient.Auth.OAuth2.Model;
using Ebay.EventNotification.Sdk.Client;
using Ebay.EventNotification.Sdk.Config;
using Ebay.EventNotification.Sdk.Processor;
using Ebay.EventNotification.Sdk.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ebay.EventNotification.Sdk.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEbayNotificationSdkServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();

            services.AddMemoryCache();

            services.AddScoped<ISignatureValidator, SignatureValidator>();
            services.AddScoped<IEndPointValidator, EndpointValidator>();
            services.AddScoped<IPublicKeyCache, PublicKeyCache>();
            services.AddScoped<IEventNotificationConfig, EventNotificationConfig>();
            services.AddScoped<IMessageProcessorFactory, MessageProcessorFactory>();
            services.AddScoped<IMessageSerializer, MessageSerializer>();

            services.AddScoped<IEbayOAuthClient, EbayOAuthClient>(sp =>
            {
                var conf = sp.GetRequiredService<IEventNotificationConfig>();
                OAuthEnvironment oAuthEnv = (conf.Environment != null && conf.Environment.Equals(Constants.Constants.Sandbox))
                    ? OAuthEnvironment.SANDBOX
                    : OAuthEnvironment.PRODUCTION;

                return new EbayOAuthClient(sp.GetRequiredService<ILogger<EbayOAuthClient>>(), oAuthEnv);
            });

            services.AddHttpClient();
            services.AddHttpClient<IPublicKeyClient, PublicKeyClient>();

            var clientCredentialsFile = configuration[$"{Constants.Constants.ConfigPrefix}:ClientCredentialsFile"];
            if (clientCredentialsFile != null)
                CredentialUtil.Load(clientCredentialsFile);

            return services;
        }

        public static IServiceCollection RegisterEbayNotificationProcessor<TMessage, TProcessor>(this IServiceCollection services) where TProcessor : class, IMessageProcessor<TMessage>
        {
            services.AddScoped<IMessageProcessor<TMessage>, TProcessor>();
            return services;
        }
    }
}