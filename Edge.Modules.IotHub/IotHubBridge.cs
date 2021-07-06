using RaaLabs.Edge.Modules.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using RaaLabs.Edge.Modules.IotHub.Client.Consumer;
using RaaLabs.Edge.Modules.IotHub.Client.Producer;
using Autofac;

namespace RaaLabs.Edge.Modules.IotHub
{
    class IotHubBridge : IBridge
    {
        private readonly List<IIotHubConsumerClient> _consumerClients;
        private readonly List<IIotHubProducerClient> _producerClients;

        private readonly ILogger _logger;

        public IotHubBridge(ILogger logger, ILifetimeScope scope, EventHandling.EventHandler<IIotHubIncomingEvent> incomingHandler, EventHandling.EventHandler<IIotHubOutgoingEvent> outgoingHandler)
        {
            _logger = logger;

            _consumerClients = incomingHandler.GetSubtypes().Select(type => (IIotHubConsumerClient)scope.Resolve(typeof(IIotHubConsumerClient<>).MakeGenericType(type))).ToList();
            _producerClients = outgoingHandler.GetSubtypes().Select(type => (IIotHubProducerClient)scope.Resolve(typeof(IIotHubProducerClient<>).MakeGenericType(type))).ToList();
        }

        public async Task SetupBridge()
        {
            var consumersTask = _consumerClients.Select(async client => await client.SetupClient()).ToList();
            var producersTask = _producerClients.Select(async client => await client.SetupClient()).ToList();

            await Task.WhenAll(consumersTask.Concat(producersTask));
        }
    }
}
