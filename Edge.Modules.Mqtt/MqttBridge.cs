using RaaLabs.Edge.Modules.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Autofac;
using RaaLabs.Edge.Modules.Mqtt.Client;
using System;
using MQTTnet;
using System.Text.RegularExpressions;

namespace RaaLabs.Edge.Modules.Mqtt
{
    public class MqttBridge : IBridgeIncomingEvent<IMqttIncomingEvent>, IBridgeOutgoingEvent<IMqttOutgoingEvent>
    {
        private readonly Dictionary<Type, IMqttBrokerClient> _brokerClients;

        private readonly IMqttMessageConverter _messageConverter;

        private EventHandling.EventHandler<IMqttIncomingEvent> _incomingHandler;
        private EventHandling.EventHandler<IMqttOutgoingEvent> _outgoingHandler;

        private ILifetimeScope _scope;

        public event EventEmitter<IMqttIncomingEvent> MqttEventReceived;

        public MqttBridge(ILogger logger, ILifetimeScope scope, IMqttMessageConverter messageConverter, EventHandling.EventHandler<IMqttIncomingEvent> incomingHandler, EventHandling.EventHandler<IMqttOutgoingEvent> outgoingHandler)
        {
            _scope = scope;
            _messageConverter = messageConverter;
            _incomingHandler = incomingHandler;
            _outgoingHandler = outgoingHandler;

            _brokerClients = GetBrokerClients(scope, incomingHandler, outgoingHandler);
        }

        public async Task SetupBridge()
        {
            var brokersTask = _brokerClients.Select(async client => await client.Value.Connect()).ToList();
            foreach (var type in _incomingHandler.GetSubtypes())
            {
                SetupSubscriptionForEventType(type);
            }

            await Task.WhenAll(brokersTask);
        }

        public async Task MessageReceived(Type connection, MqttApplicationMessage message)
        {
            var conv = _messageConverter.ToEvent(connection, message);
            if (_messageConverter.ToEvent(connection, message) is IMqttIncomingEvent @event)
            {
                MqttEventReceived(@event);
            }

            await Task.CompletedTask;
        }

        public void Handle(IMqttOutgoingEvent @event)
        {
            var (connection, message) = _messageConverter.ToMessage(@event) ?? (null, null);
            if (connection == null || message == null) return;

            if (_brokerClients.TryGetValue(connection, out IMqttBrokerClient client))
            {
                client.SendAsync(message);
            }
        }

        private void SetupSubscriptionForEventType(Type eventType)
        {
            var attr = eventType.GetAttribute<MqttBrokerConnectionAttribute>();
            var topic = MqttTopicMapper.ToRegularTopic(attr.Topic);
            var client = (IMqttBrokerClient)_scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(attr.BrokerConnection));
            client.Subscribe(topic);
        }

        private Dictionary<Type, IMqttBrokerClient> GetBrokerClients(ILifetimeScope scope, EventHandling.EventHandler<IMqttIncomingEvent> incomingHandler, EventHandling.EventHandler<IMqttOutgoingEvent> outgoingHandler)
        {
            var incomingEventTypes = incomingHandler.GetSubtypes();
            var outgoingEventTypes = outgoingHandler.GetSubtypes();

            var incomingEventBrokers = incomingEventTypes.Select(type => type.GetAttribute<MqttBrokerConnectionAttribute>()).Select(attr => attr.BrokerConnection);
            var outgoingEventBrokers = outgoingEventTypes.Select(type => type.GetAttribute<MqttBrokerConnectionAttribute>()).Select(attr => attr.BrokerConnection);

            var brokerTypes = incomingEventBrokers.Union(outgoingEventBrokers).ToList();

            var clients = brokerTypes.ToDictionary(type => type, type => (IMqttBrokerClient)scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(type)));

            foreach (var (_, client) in clients)
            {
                client.OnDataReceived += MessageReceived;
            }

            return clients;
        }

    }
}
