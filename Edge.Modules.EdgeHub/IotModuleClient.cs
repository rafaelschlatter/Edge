using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;
using Serilog;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// An IotModuleClient with an actual underlying EdgeHub ModuleClient.
    /// </summary>
    public class IotModuleClient : IIotModuleClient
    {
        private ModuleClient _client;
        private readonly ILogger _logger;

        public event DataReceivedDelegate<(string inputName, Message message)> OnDataReceived;

        public IotModuleClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Connect()
        {
            _client = await ModuleClient.CreateFromEnvironmentAsync(TransportType.Mqtt);

            _logger.Information("Open IoT Edge ModuleClient and wait");
            await _client.OpenAsync();

            _logger.Information("Client is ready");
        }

        public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
        {
            return _client.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
        }

        public Task Subscribe(string topic)
        {
            return _client.SetInputMessageHandlerAsync(topic, (message, context) =>
            {
                // Discard task handle, because awaiting this would cause a deadlock if a task triggered by this task would try to send EdgeHub data.
                _ = OnDataReceived(null, (topic, message));

                return Task.FromResult(MessageResponse.Completed);
            }, null);
        }

        public Task SendAsync((string outputName, Message message) data)
        {
            var payload = Encoding.UTF8.GetString(data.message.GetBytes());
            _logger.Information("Sending event to EdgeHub endpoint '{Output}': {Payload}", data.outputName, payload);

            return _client.SendEventAsync(data.outputName, data.message);
        }

    }
}
