using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventHubModuleClient
    {
        public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext);
        public Task SendEventAsync(string outputName, Message message);

    }
}
