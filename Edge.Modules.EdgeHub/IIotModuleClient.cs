using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIotModuleClient
    {
        public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext);
        public Task SendEventAsync(string outputName, Message message);

    }
}
