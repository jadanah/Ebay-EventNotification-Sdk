using Ebay.EventNotification.Sdk.Constants;

namespace Ebay.EventNotification.Sdk.Processor
{
    public interface IMessageProcessorFactory
    {
        IMessageProcessor GetProcessor(TopicEnum topicEnum);
    }
}