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
        public event AsyncEventEmitter<IEventHubIncomingEvent> IncomingEvent;
        
        private readonly List<IEventHubConsumerClient> _consumerClients;
        private readonly List<IEventHubProducerClient> _producerClients;

        private readonly ILogger _logger;

        public EventHubBridge(ILogger logger, ILifetimeScope scope, EventHandling.EventHandler<IEventHubIncomingEvent> incomingHandler, EventHandling.EventHandler<IEventHubOutgoingEvent> outgoingHandler)
        {
            _logger = logger;

            _consumerClients = incomingHandler.GetSubtypes().Select(type => (IEventHubConsumerClient)scope.Resolve(typeof(IEventHubConsumerClient<>).MakeGenericType(type))).ToList();
            _producerClients = outgoingHandler.GetSubtypes().Select(type => (IEventHubProducerClient)scope.Resolve(typeof(IEventHubProducerClient<>).MakeGenericType(type))).ToList();
        }

        public async Task SetupBridge()
        {
            var consumersTask = _consumerClients.Select(async client => await client.SetupClient()).ToList();
            var producersTask = _producerClients.Select(async client => await client.SetupClient()).ToList();

            await Task.WhenAll(consumersTask.Concat(producersTask));
        }
    }
}
