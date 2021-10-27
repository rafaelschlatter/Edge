using System.Threading.Tasks;
using Serilog;
using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.IotHub.Client
{
    class IotHubClient<T> : IIotHubClient<T> where T : IIotHubConnection
    {
        private readonly T _connection;
        private readonly ILogger _logger;

        private DeviceClient _client;

        private MessageReceivedDelegate MessageReceived;

        public event DataReceivedDelegate<Message> OnDataReceived;

        public IotHubClient(T connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task SetupClient(MessageReceivedDelegate eventHandler)
        {
            MessageReceived = eventHandler;

            _client = DeviceClient.CreateFromConnectionString(_connection.ConnectionString, TransportType.Amqp);

            _client.SetConnectionStatusChangesHandler(ClientConnectionChangedHandler);

            await _client.OpenAsync();
            await _client.SetReceiveMessageHandlerAsync(async (message, ctx) =>
            {
                await MessageReceived(typeof(T), message);
            }, null);
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
            }
        }

        public async Task SendMessageAsync(Message message)
        {
            await _client.SendEventAsync(message);
        }

        public Task SendAsync(Message data)
        {
            throw new System.NotImplementedException();
        }

        public Task Connect()
        {
            throw new System.NotImplementedException();
        }
    }
}
