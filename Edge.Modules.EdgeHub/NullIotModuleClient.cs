using Microsoft.Azure.Devices.Client;
using Serilog;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// A mocked IotModuleClient, used when there is no edgeHub running on the system. Will be used during testing.
    /// </summary>
    public class NullIotModuleClient : IIotModuleClient
    {
        private readonly ILogger _logger;

        private readonly Dictionary<string, MessageHandler> _messageHandlers;
        public List<string> MessagesSent { get; }

        public NullIotModuleClient(ILogger logger)
        {
            _logger = logger;
            _messageHandlers = new Dictionary<string, MessageHandler>();
            MessagesSent = new List<string>();
        }

        public Task SendEventAsync(string outputName, Message message)
        {
            var payload = Encoding.UTF8.GetString(message.GetBytes());
            _logger.Information("Payload to send: {Payload}", payload);

            // Set an upper limit to the number of messages to be stored
            if (MessagesSent.Count < 1000)
            {
                MessagesSent.Add(payload);
            }

            return Task.CompletedTask;
        }

        public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
        {
            _messageHandlers[inputName] = messageHandler;
            return Task.CompletedTask;
        }

        public void SimulateIncomingEvent(string inputName, string value)
        {
            var message = new Message(Encoding.UTF8.GetBytes(value));
            _messageHandlers[inputName](message, null);
        }
    }
}
