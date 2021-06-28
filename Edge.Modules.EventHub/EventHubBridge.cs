using RaaLabs.Edge.Modules.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using RaaLabs.Edge.Modules.EventHub.Client.Consumer;
using RaaLabs.Edge.Modules.EventHub.Client.Producer;
using Autofac;

namespace RaaLabs.Edge.Modules.EventHub
{
    class EventHubBridge : IBridge
    {
        private readonly List<IEventHubConsumerClient> _consumerClients;
        private readonly List<IEventHubProducerClient> _producerClients;

        private readonly ILogger _logger;

        public EventHubBridge(ILogger logger, ILifetimeScope scope, EventHandler<IEventHubIncomingEvent> incomingHandler, EventHandler<IEventHubOutgoingEvent> outgoingHandler)
        {
            _logger = logger;

            var incomingEventTypes = incomingHandler.GetSubtypes();
            var outgoingEventTypes = outgoingHandler.GetSubtypes();

            foreach (var type in incomingEventTypes)
            {
                _logger.Information("Setting up consumer client for '{EventType}'", type.Name);
            }

            foreach (var type in outgoingEventTypes)
            {
                _logger.Information("Setting up producer client for '{EventType}'", type.Name);
            }

            _consumerClients = incomingEventTypes.Select(type => (IEventHubConsumerClient)scope.Resolve(typeof(IEventHubConsumerClient<>).MakeGenericType(type))).ToList();
            _producerClients = outgoingEventTypes.Select(type => (IEventHubProducerClient)scope.Resolve(typeof(IEventHubProducerClient<>).MakeGenericType(type))).ToList();
        }

        public async Task SetupBridge()
        {
            _logger.Information("Setting up EventHubBridge");
            var consumersTask = _consumerClients.Select(async client => await client.SetupClient()).ToList();
            var producersTask = _producerClients.Select(async client => await client.SetupClient()).ToList();

            await TaskHelpers.WhenAllWithLoggedExceptions(_logger, consumersTask.Concat(producersTask));
        }
    }
}
