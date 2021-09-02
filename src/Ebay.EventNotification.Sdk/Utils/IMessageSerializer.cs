using Ebay.EventNotification.Sdk.Models;

namespace Ebay.EventNotification.Sdk.Utils
{
    public interface IMessageSerializer
    {
        string Serialize(Message message);
        string Serialize(object obj);
        
        T Deserialize<T>(object obj);
        T Deserialize<T>(string message);
    }
}