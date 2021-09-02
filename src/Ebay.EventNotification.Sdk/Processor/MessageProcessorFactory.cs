using System;
using Ebay.EventNotification.Sdk.Constants;
using Ebay.EventNotification.Sdk.Exceptions;
using Ebay.EventNotification.Sdk.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Ebay.EventNotification.Sdk.Processor
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IMessageProcessor GetProcessor(TopicEnum topicEnum)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            return topicEnum switch
            {
                TopicEnum.MARKETPLACE_ACCOUNT_DELETION => scope.ServiceProvider.GetRequiredService<IMessageProcessor<AccountDeletionData>>(),
                _ => throw new ProcessorNotDefined("Message processor not registered for " + topicEnum.ToString())
            };
        }
    }
}