using Microsoft.Azure.Devices.Client;
using Serilog;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// An EventHubModuleClient with an actual underlying EventHub Client.
    /// </summary>
    public class EventHubModuleClient : IEventHubModuleClient
    {
        private readonly ILogger _logger;

        public EventHubModuleClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task SendEventAsync(string outputName, Message message)
        {
            // TODO: Implement
            await Task.CompletedTask;
        }

        public async Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
        {
            // TODO: Implement
            await Task.CompletedTask;
        }


    }
}
