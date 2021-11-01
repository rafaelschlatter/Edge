using Microsoft.Azure.Devices.Client;
using Serilog;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// A mocked IotModuleClient, used when there is no edgeHub running on the system. Will be used during testing.
    /// </summary>
    public class NullIotModuleClient : IIotModuleClient
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, Channel<Message>> _messagesToSend;

        public event DataReceivedDelegate<(string inputName, Message message)> OnDataReceived;

        public List<(string, string)> MessagesSent { get; }

        public NullIotModuleClient(ILogger logger)
        {
            _logger = logger;
            MessagesSent = new List<(string, string)>();
            _messagesToSend = new ConcurrentDictionary<string, Channel<Message>>();
        }

        public Task SendAsync((string outputName, Message message) data)
        {
            var payload = Encoding.UTF8.GetString(data.message.GetBytes());
            _logger.Information("Payload to send: {Payload}", payload);

            // Set an upper limit to the number of messages to be stored
            if (MessagesSent.Count < 1000)
            {
                MessagesSent.Add((data.outputName, payload));
            }

            return Task.CompletedTask;
        }

        public async Task Subscribe(string inputName)
        {
            var channel = Channel.CreateUnbounded<Message>();
            _messagesToSend[inputName] = channel;
            var channelReader = channel.Reader;

            while (true)
            {
                var message = await channelReader.ReadAsync();
                _ = OnDataReceived(null, (inputName, message));
            }
        }

        public Task Connect()
        {
            return Task.CompletedTask;
        }
    }
}
