using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge;

namespace RaaLabs.Edge.Modules.IotHub.Client
{
    class IotHubClient<T> : IIotHubClient<T> where T : IIotHubConnection
    {
        private readonly T _connection;
        private readonly ILogger _logger;

        private DeviceClient _client;
        private Channel<MessageReceivedDelegate> _pendingSubscriptions;
        private Channel<Message> _pendingOutgoingMessages;

        public IotHubClient(T connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
            _pendingSubscriptions = Channel.CreateUnbounded<MessageReceivedDelegate>();
            _pendingOutgoingMessages = Channel.CreateUnbounded<Message>();
        }

        public async Task SetupClient()
        {
            _client = DeviceClient.CreateFromConnectionString(_connection.ConnectionString, TransportType.Amqp);

            _client.SetConnectionStatusChangesHandler(ClientConnectionChangedHandler);

            var setupSubscribingTask = SetupSubscribing(_client);
            var setupProducingTask = SetupProducing(_client);

            await _client.OpenAsync();

            await TaskHelpers.WhenAllWithLoggedExceptions(_logger, new List<Task> { setupSubscribingTask, setupProducingTask });
        }

        private async Task SetupSubscribing(DeviceClient client)
        {
            var subscribers = new ConcurrentBag<MessageReceivedDelegate>();

            await client.SetReceiveMessageHandlerAsync(async (message, ctx) =>
            {
                var allInvocations = subscribers.Select(async subscriber => await subscriber(message)).ToList();
                await TaskHelpers.WhenAllWithLoggedExceptions(_logger, allInvocations);
            }, null);

            while (true)
            {
                var subscription = await _pendingSubscriptions.Reader.ReadAsync();
                subscribers.Add(subscription);
            }
        }

        private async Task SetupProducing(DeviceClient client)
        {
            while (true)
            {
                var message = await _pendingOutgoingMessages.Reader.ReadAsync();
                await client.SendEventAsync(message);
            }
        }

        private void ClientConnectionChangedHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            switch (status)
            {
                case ConnectionStatus.Disconnected:
                    _logger.Error("Disconnected from IotHub '{IotHub}'. Reason: '{Reason}'", typeof(T).Name, reason);
                    break;
                case ConnectionStatus.Connected:
                    _logger.Information("Connected to IotHub '{IotHub}'", typeof(T).Name, reason);
                    break;
                case ConnectionStatus.Disabled:
                    _logger.Error("IotHub '{IotHub}' is disabled. Reason: '{Reason}'", typeof(T).Name, reason);
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    _logger.Error("Disconnected from IotHub '{IotHub}', but trying to reconnect. Reason: '{Reason}'", typeof(T).Name, reason);
                    break;
            };
        }

        public async Task SendMessageAsync(Message message)
        {
            await _pendingOutgoingMessages.Writer.WriteAsync(message);
        }

        public async Task Subscribe(MessageReceivedDelegate eventHandler)
        {
            await _pendingSubscriptions.Writer.WriteAsync(eventHandler);
        }
    }
}
