using System.Threading.Tasks;
using Ebay.EventNotification.Sdk.Models;

namespace Ebay.EventNotification.Sdk.Processor
{
    public interface IMessageProcessor<TMessage> : IMessageProcessor
    {
    }

    public interface IMessageProcessor
    {
        Task ProcessAsync(Message message);
    }
}