using Microsoft.Azure.Devices.Client;
using Serilog;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// A mocked IotModuleClient, used when there is no edgeHub running on the system. Will be used during testing.
    /// </summary>
    public class NullIotModuleClient : IIotModuleClient
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, Channel<Message>> _messagesToSend;
        public List<(string, string)> MessagesSent { get; }

        public NullIotModuleClient(ILogger logger)
        {
            _logger = logger;
            MessagesSent = new List<(string, string)>();
            _messagesToSend = new ConcurrentDictionary<string, Channel<Message>>();
        }

        public Task SendEventAsync(string outputName, Message message)
        {
            var payload = Encoding.UTF8.GetString(message.GetBytes());
            _logger.Information("Payload to send: {Payload}", payload);

            // Set an upper limit to the number of messages to be stored
            if (MessagesSent.Count < 1000)
            {
                MessagesSent.Add((outputName, payload));
            }

            return Task.CompletedTask;
        }

        public async Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext)
        {
            var channel = Channel.CreateUnbounded<Message>();
            _messagesToSend[inputName] = channel;
            var channelReader = channel.Reader;

            while (true)
            {
                var message = await channelReader.ReadAsync();
                await messageHandler(message, null);
            }
        }

        public void SimulateIncomingEvent(string inputName, string value)
        {
            var message = new Message(Encoding.UTF8.GetBytes(value));
            var messageChannel = _messagesToSend[inputName];
            if (messageChannel != null)
            {
                messageChannel.Writer.WriteAsync(message).AsTask().Wait();
            }
        }

        public async Task SimulateIncomingEventAsync(string inputName, string value)
        {
            var message = new Message(Encoding.UTF8.GetBytes(value));
            var messageChannel = _messagesToSend[inputName];
            if (messageChannel != null)
            {
                await messageChannel.Writer.WriteAsync(message).AsTask();
            }
        }
    }
}
