using System.Threading.Tasks;
using Serilog;
using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.IotHub.Client
{
    class IotHubClient<ConnectionType> : IIotHubClient<ConnectionType> where ConnectionType : IIotHubConnection
    {
        private readonly ConnectionType _connection;
        private readonly ILogger _logger;

        private DeviceClient _client;

        public event DataReceivedDelegate<Message> OnDataReceived;

        public IotHubClient(ConnectionType connection, ILogger logger)
        {
            _connection = connection;
            _logger = logger;
        }

        private void ClientConnectionChangedHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            switch (status)
            {
                case ConnectionStatus.Disconnected:
                    _logger.Error("Disconnected from IotHub '{IotHub}'. Reason: '{Reason}'", typeof(ConnectionType).Name, reason);
                    break;
                case ConnectionStatus.Connected:
                    _logger.Information("Connected to IotHub '{IotHub}'", typeof(ConnectionType).Name, reason);
                    break;
                case ConnectionStatus.Disabled:
                    _logger.Error("IotHub '{IotHub}' is disabled. Reason: '{Reason}'", typeof(ConnectionType).Name, reason);
                    break;
                case ConnectionStatus.Disconnected_Retrying:
                    _logger.Error("Disconnected from IotHub '{IotHub}', but trying to reconnect. Reason: '{Reason}'", typeof(ConnectionType).Name, reason);
                    break;
            }
        }

        public async Task SendAsync(Message data)
        {
            await _client.SendEventAsync(data);
        }

        public async Task Connect()
        {
            _client = DeviceClient.CreateFromConnectionString(_connection.ConnectionString, TransportType.Amqp);

            _client.SetConnectionStatusChangesHandler(ClientConnectionChangedHandler);

            await _client.OpenAsync();
            await _client.SetReceiveMessageHandlerAsync(async (message, ctx) =>
            {
                await OnDataReceived!(typeof(ConnectionType), message);
            }, null);
        }
    }
}
