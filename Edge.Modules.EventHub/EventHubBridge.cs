using RaaLabs.Edge.Modules.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using RaaLabs.Edge.Modules.EventHub.Client.Consumer;
using RaaLabs.Edge.Modules.EventHub.Client.Producer;
using Autofac;
using System;
using Azure.Messaging.EventHubs;

namespace RaaLabs.Edge.Modules.EventHub
{
    class EventHubBridge : IBridgeIncomingEvent<IEventHubIncomingEvent>, IBridgeOutgoingEvent<IEventHubOutgoingEvent>
    {
        private readonly Dictionary<Type, IEventHubConsumerClient> _consumerClients;
        private readonly Dictionary<Type, IEventHubProducerClient> _producerClients;

        private readonly IEventHubEventDataConverter _dataConverter;

        private readonly ILogger _logger;

        public event EventEmitter<IEventHubIncomingEvent> EventHubEventReceived;

        public EventHubBridge(ILogger logger, ILifetimeScope scope, IEventHubEventDataConverter dataConverter, EventHandling.EventHandler<IEventHubIncomingEvent> incomingHandler, EventHandling.EventHandler<IEventHubOutgoingEvent> outgoingHandler)
        {
            _logger = logger;
            _dataConverter = dataConverter;

            _consumerClients = GetEventHubConsumerClients(scope, incomingHandler);
            _producerClients = GetEventHubProducerClients(scope, outgoingHandler);
        }

        public void Handle(IEventHubOutgoingEvent @event)
        {
            var (connection, data) = _dataConverter.ToEventData(@event) ?? (null, null);
            if (connection == null || data == null) return;

            if (!_producerClients.TryGetValue(connection, out IEventHubProducerClient client)) return;

            client.SendAsync(data);
        }

        public async Task SetupBridge()
        {
            _logger.Information("Setting up EventHubBridge");
            var consumersTask = _consumerClients.Select(async client => await client.Value.Connect()).ToList();
            var producersTask = _producerClients.Select(async client => await client.Value.Connect()).ToList();

            await Task.WhenAll(consumersTask);
            await Task.WhenAll(producersTask);
        }

        public async Task EventReceived(Type connection, EventData data)
        {
            var @event = _dataConverter.ToEvent(connection, data);
            if (!@event.GetType().IsAssignableTo<IEventHubIncomingEvent>()) return;
            EventHubEventReceived((IEventHubIncomingEvent)@event);

            await Task.CompletedTask;
        }


        private Dictionary<Type, IEventHubConsumerClient> GetEventHubConsumerClients(ILifetimeScope scope, EventHandling.EventHandler<IEventHubIncomingEvent> incomingHandler)
        {
            var incomingEventTypes = incomingHandler.GetSubtypes();

            var clientTypes = incomingEventTypes.Select(type => type.GetAttribute<EventHubConnectionAttribute>()).Select(attr => attr.Connection).ToHashSet();

            var consumerClients = clientTypes.ToDictionary(type => type, type => (IEventHubConsumerClient)scope.Resolve(typeof(IEventHubConsumerClient<>).MakeGenericType(type)));
        
            // Connect event data received for all consumer clients to this bridge
            foreach (var client in consumerClients)
            {
                client.Value.OnDataReceived += EventReceived;
            }

            return consumerClients;
        }

        private static Dictionary<Type, IEventHubProducerClient> GetEventHubProducerClients(ILifetimeScope scope, EventHandling.EventHandler<IEventHubOutgoingEvent> outgoingHandler)
        {
            var outgoingEventTypes = outgoingHandler.GetSubtypes();

            var clientTypes = outgoingEventTypes.Select(type => type.GetAttribute<EventHubConnectionAttribute>()).Select(attr => attr.Connection).ToHashSet();

            return clientTypes.ToDictionary(type => type, type => (IEventHubProducerClient)scope.Resolve(typeof(IEventHubProducerClient<>).MakeGenericType(type)));
        }

    }
}
