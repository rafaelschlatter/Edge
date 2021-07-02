using RaaLabs.Edge.Modules.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using RaaLabs.Edge.Modules.Mqtt.Client.Consumer;
using RaaLabs.Edge.Modules.Mqtt.Client.Producer;
using Autofac;
using RaaLabs.Edge.Modules.Mqtt.Client;

namespace RaaLabs.Edge.Modules.Mqtt
{
    class MqttBridge : IBridge
    {
        private readonly List<IMqttConsumerClient> _consumerClients;
        private readonly List<IMqttProducerClient> _producerClients;
        private readonly List<IMqttBrokerClient> _brokerClients;

        private readonly ILogger _logger;

        public MqttBridge(ILogger logger, ILifetimeScope scope, EventHandling.EventHandler<IMqttIncomingEvent> incomingHandler, EventHandling.EventHandler<IMqttOutgoingEvent> outgoingHandler)
        {
            _logger = logger;

            var incomingEventTypes = incomingHandler.GetSubtypes();
            var outgoingEventTypes = outgoingHandler.GetSubtypes();

            var incomingEventBrokers = incomingEventTypes.Select(type => type.GetAttribute<MqttBrokerConnectionAttribute>()).Select(attr => attr.BrokerConnection);
            var outgoingEventBrokers = outgoingEventTypes.Select(type => type.GetAttribute<MqttBrokerConnectionAttribute>()).Select(attr => attr.BrokerConnection);

            var allBrokerTypes = incomingEventBrokers.Union(outgoingEventBrokers).ToHashSet();
            _brokerClients = allBrokerTypes.Select(type => (IMqttBrokerClient)scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(type))).ToList();

            _consumerClients = incomingEventTypes.Select(type => (IMqttConsumerClient)scope.Resolve(typeof(IMqttConsumerClient<>).MakeGenericType(type))).ToList();
            _producerClients = outgoingEventTypes.Select(type => (IMqttProducerClient)scope.Resolve(typeof(IMqttProducerClient<>).MakeGenericType(type))).ToList();
        }

        public async Task SetupBridge()
        {
            var brokersTask = _brokerClients.Select(async client => await client.SetupClient()).ToList();
            var consumersTask = _consumerClients.Select(async client => await client.SetupClient()).ToList();
            var producersTask = _producerClients.Select(async client => await client.SetupClient()).ToList();

            await Task.WhenAll(brokersTask.Concat(consumersTask).Concat(producersTask));
        }
    }
}
