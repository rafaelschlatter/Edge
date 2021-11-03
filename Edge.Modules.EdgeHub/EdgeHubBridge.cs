using RaaLabs.Edge.Modules.EventHandling;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using System;
using Microsoft.Azure.Devices.Client;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// Class responsible for bridging events to and from EdgeHub clients
    /// </summary>
    public class EdgeHubBridge : IBridgeIncomingEvent<IEdgeHubIncomingEvent>, IBridgeOutgoingEvent<IEdgeHubOutgoingEvent>
    {
        private readonly IIotModuleClient _client;
        private readonly IEdgeHubMessageConverter _messageConverter;
        private readonly IEventHandler<IEdgeHubIncomingEvent> _incomingHandler;

        public event EventEmitter<IEdgeHubIncomingEvent> EdgeHubEventReceived;

        public EdgeHubBridge(IEdgeHubMessageConverter messageConverter, IIotModuleClient client, IEventHandler<IEdgeHubIncomingEvent> incomingHandler)
        {
            _messageConverter = messageConverter;
            _incomingHandler = incomingHandler;

            _client = client;

            client.OnDataReceived += MessageReceived;
        }

        public async Task SetupBridge()
        {
            await _client.Connect();
            var subscriptionsTask = _incomingHandler.GetSubtypes()
                .Select(type => type.GetAttribute<InputNameAttribute>().InputName)
                .Select(inputName => _client.Subscribe(inputName))
                .ToList();

            await Task.WhenAll(subscriptionsTask);
        }

        public async Task MessageReceived(Type connection, (string inputName, Message message) data)
        {
            var @event = _messageConverter.ToEvent(data.inputName, data.message);
            if (!@event.GetType().IsAssignableTo<IEdgeHubIncomingEvent>()) return;
            _ = Task.Run(() => EdgeHubEventReceived!((IEdgeHubIncomingEvent)@event));

            await Task.CompletedTask;
        }

        public void Handle(IEdgeHubOutgoingEvent @event)
        {
            var (outputName, message) = _messageConverter.ToMessage(@event) ?? (null, null);
            if (outputName == null || message == null) return;

            _client.SendAsync((outputName, message));
        }
    }
}
