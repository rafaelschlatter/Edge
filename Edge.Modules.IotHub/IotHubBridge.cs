using RaaLabs.Edge.Modules.EventHandling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Autofac;
using RaaLabs.Edge.Modules.IotHub.Client;
using System;
using Microsoft.Azure.Devices.Client;

namespace RaaLabs.Edge.Modules.IotHub
{
    /// <summary>
    /// Class responsible for bridging events to and from IotHub clients
    /// </summary>
    public class IotHubBridge : IBridgeIncomingEvent<IIotHubIncomingEvent>, IBridgeOutgoingEvent<IIotHubOutgoingEvent>
    {
        private readonly Dictionary<Type, IIotHubClient> _clients;
        private readonly IIotHubMessageConverter _messageConverter;

        public event EventEmitter<IIotHubIncomingEvent> IotHubEventReceived;

        public IotHubBridge(ILifetimeScope scope, IIotHubMessageConverter messageConverter, EventHandling.EventHandler<IIotHubIncomingEvent> incomingHandler, EventHandling.EventHandler<IIotHubOutgoingEvent> outgoingHandler)
        {
            _messageConverter = messageConverter;

            _clients = GetIotHubClients(scope, incomingHandler, outgoingHandler);
        }

        public async Task SetupBridge()
        {
            var clientsTask = _clients.Select(async client => await client.Value.Connect()).ToList();

            await Task.WhenAll(clientsTask);
        }

        private async Task MessageReceived(Type connection, Message message)
        {
            var @event = _messageConverter.ToEvent(connection, message);
            if (!@event.GetType().IsAssignableTo<IIotHubIncomingEvent>()) return;
            _ = Task.Run(() => IotHubEventReceived!((IIotHubIncomingEvent)@event));

            await Task.CompletedTask;
        }


        public void Handle(IIotHubOutgoingEvent @event)
        {
            var (connection, message) = _messageConverter.ToMessage(@event) ?? (null, null);
            if (connection == null || message == null) return;

            if (!_clients.TryGetValue(connection, out IIotHubClient client)) return;

            client.SendAsync(message);
        }

        private Dictionary<Type, IIotHubClient> GetIotHubClients(ILifetimeScope scope, EventHandling.EventHandler<IIotHubIncomingEvent> incomingHandler, EventHandling.EventHandler<IIotHubOutgoingEvent> outgoingHandler)
        {
            var incomingEventTypes = incomingHandler.GetSubtypes();
            var outgoingEventTypes = outgoingHandler.GetSubtypes();

            var incomingConnections = incomingEventTypes.Select(type => type.GetAttribute<IotHubConnectionAttribute>()).Select(attr => attr.Connection);
            var outgoingConnections = outgoingEventTypes.Select(type => type.GetAttribute<IotHubConnectionAttribute>()).Select(attr => attr.Connection);

            var clientTypes = incomingConnections.Union(outgoingConnections).ToList();

            var clients = clientTypes.ToDictionary(type => type, type => (IIotHubClient)scope.Resolve(typeof(IIotHubClient<>).MakeGenericType(type)));
            foreach (var (clientType, client) in clients)
            {
                client.OnDataReceived += MessageReceived;
            }

            return clients;
        }

    }
}
