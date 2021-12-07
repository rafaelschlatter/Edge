using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Threading.Tasks;
using Serilog;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using System.Threading.Channels;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using RaaLabs.Edge.Modules.Mqtt.Client.Authentication;

namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    class MqttBrokerClient<ConnectionType> : IMqttBrokerClient<ConnectionType> where ConnectionType : IMqttBrokerConnection
    {
        private readonly ConnectionType _connection;
        private readonly ILogger _logger;

        private IManagedMqttClient _client;
        private readonly Channel<string> _pendingSubscriptions = Channel.CreateUnbounded<string>();

        private readonly Channel<MqttApplicationMessage> _pendingOutgoingMessages;


        public event DataReceivedDelegate<MqttApplicationMessage> OnDataReceived;

        public MqttBrokerClient(ConnectionType broker, ILogger logger)
        {
            _connection = broker;
            _logger = logger;
            _pendingOutgoingMessages = Channel.CreateUnbounded<MqttApplicationMessage>();
        }

        public async Task Connect()
        {
            _logger.Information("Connecting to MQTT Broker '{Client}' on {Ip}:{Port} as '{ClientId}'...", _connection.GetType().Name, _connection.Ip, _connection.Port, _connection.ClientId);

            var clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_connection.ClientId)
                .WithTcpServer(_connection.Ip, _connection.Port);

            clientOptions = AppendAuthentication(clientOptions);

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(clientOptions.Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            _client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(ClientConnectedHandler);
            _client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(ClientDisconnectedHandler);
            _client.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(MessageReceived);
            _client.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(ConnectingFailedHandler);

            await _client.StartAsync(options);

            var setupSubscribingTask = SetupSubscribing();
            var setupProducingTask = SetupProducing();

            await Task.WhenAll(setupSubscribingTask, setupProducingTask);
        }

        private async Task SetupSubscribing()
        {
            while (true)
            {
                var topic = await _pendingSubscriptions.Reader.ReadAsync();
                await _client.SubscribeAsync(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            }
        }

        private async Task SetupProducing()
        {
            while (true)
            {
                var message = await _pendingOutgoingMessages.Reader.ReadAsync();
                await _client.PublishAsync(message);
            }
        }

        private async Task MessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            var message = args.ApplicationMessage;

            await OnDataReceived(typeof(ConnectionType), message);
        }

        private async Task ClientConnectedHandler(MqttClientConnectedEventArgs args)
        {
            _logger.Information("Connected to MQTT Broker '{Client}' on {Ip}:{Port} as '{ClientId}'", _connection.GetType().Name, _connection.Ip, _connection.Port, _connection.ClientId);
            await Task.CompletedTask;
        }

        private async Task ClientDisconnectedHandler(MqttClientDisconnectedEventArgs args)
        {
            _logger.Information("Disconnected from MQTT broker '{Client}'. Reason: {Reason}", _connection.GetType().Name, args.Reason);
            await Task.CompletedTask;
        }

        private async Task ConnectingFailedHandler(ManagedProcessFailedEventArgs args)
        {
            _logger.Information("Unable to connect to MQTT broker '{Client}'. Reason: {Reason}", _connection.GetType().Name, args.Exception.Message);
            await Task.CompletedTask;
        }

        public async Task Subscribe(string topicPattern)
        {
            await _pendingSubscriptions.Writer.WriteAsync(topicPattern);
        }

        public async Task SendAsync(MqttApplicationMessage message)
        {
            await _pendingOutgoingMessages.Writer.WriteAsync(message);
        }

        private MqttClientOptionsBuilder AppendAuthentication(MqttClientOptionsBuilder optionsBuilder)
        {
            switch (_connection.Authentication)
            {
                case Authentication.Authentication auth:
                    optionsBuilder.WithAuthentication(auth.Method, auth.Data);
                    break;
                case UsernameAndPasswordAuthentication usernameAndPassword:
                    optionsBuilder.WithCredentials(usernameAndPassword.Username, usernameAndPassword.Password);
                    break;
                default:
                    break;
            }
            return optionsBuilder;
        }

    }
}
