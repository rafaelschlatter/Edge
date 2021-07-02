using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using RaaLabs.Edge.Modules.Mqtt.Client.Authentication;

namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    class MqttBrokerClient<T> : IMqttBrokerClient<T> where T : IMqttBrokerConnection
    {
        private readonly T _connection;
        private readonly ILogger _logger;

        private IManagedMqttClient _client;
        private Channel<(string topic, MessageReceivedDelegate handler)> _pendingSubscriptions;
        private Channel<MqttApplicationMessage> _pendingOutgoingMessages;

        private ConcurrentBag<(string topicPattern, MessageReceivedDelegate handler)> _routes;

        public MqttBrokerClient(T broker, ILogger logger)
        {
            _connection = broker;
            _logger = logger;
            _routes = new ConcurrentBag<(string topicPattern, MessageReceivedDelegate handler)>();
            _pendingSubscriptions = Channel.CreateUnbounded<(string, MessageReceivedDelegate)>();
            _pendingOutgoingMessages = Channel.CreateUnbounded<MqttApplicationMessage>();
        }

        public async Task SetupClient()
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

            await _client.StartAsync(options);

            var setupSubscribingTask = SetupSubscribing();
            var setupProducingTask = SetupProducing();

            await Task.WhenAll(setupSubscribingTask, setupProducingTask);
        }

        private async Task SetupSubscribing()
        {
            while (true)
            {
                var (topic, handler) = await _pendingSubscriptions.Reader.ReadAsync();
                _routes.Add((topic, handler));
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
            string clientId = args.ClientId;
            var message = args.ApplicationMessage;
            string topic = message.Topic;
            var matchedHandlers = _routes
                .Where(route => TopicMatches(topic, route.topicPattern))
                .Select(route => route.handler);

            var handlerCalledTasks = matchedHandlers.Select(async handler => await handler(clientId, message)).ToList();
            if (handlerCalledTasks.Count > 0)
            {
                await Task.WhenAll(handlerCalledTasks);
            }
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

        public async Task SubscribeToTopic(string topicPattern, MessageReceivedDelegate eventHandler)
        {
            await _pendingSubscriptions.Writer.WriteAsync((topicPattern, eventHandler));
        }

        public async Task SendMessageAsync(MqttApplicationMessage message)
        {
            await _pendingOutgoingMessages.Writer.WriteAsync(message);
        }

        private static bool TopicMatches(string topic, string pattern)
        {
            if (topic == pattern) return true;
            var topicLevels = topic.Split("/");
            var patternLevels = topic.Split("/");

            foreach (var (t, p) in topicLevels.Zip(patternLevels))
            {
                if (p == "#") return true;
                if (p == "+" || t == p) continue;
                return false;
            }

            return true;
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
