using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Reflection;
using Serilog;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Serialization;
using Autofac;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    class TimescaledbBridge : IBridgeOutgoingEvent<ITimescaledbOutgoingEvent>
    {        
        private readonly Dictionary<Type, ITimescaledbClient> _clients;

        public TimescaledbBridge(ILifetimeScope scope, EventHandling.EventHandler<ITimescaledbOutgoingEvent> outgoingHandler)
        {
            _clients = GetOutgoingDbClients(scope, outgoingHandler);
        }

        public void Handle(ITimescaledbOutgoingEvent @event)
        {
            var connectionType = @event.GetType().GetAttribute<TimescaledbConnectionAttribute>()?.Connection;
            if (connectionType == null) return;
            if (!_clients.TryGetValue(connectionType, out ITimescaledbClient client)) return;

            client.SendAsync(@event);
        }

        public async Task SetupBridge()
        {
            await SetupOutgoingEvents();
        }
        private async Task SetupOutgoingEvents()
        {
            // Connect to all databases
            await Task.WhenAll(_clients.Select(async client => await client.Value.Connect()).ToList());
        }

        private static Dictionary<Type, ITimescaledbClient> GetOutgoingDbClients(ILifetimeScope scope, EventHandling.EventHandler<ITimescaledbOutgoingEvent> outgoingHandler)
        {
            var outgoingEventTypes = outgoingHandler.GetSubtypes();
            var timescaledbConnectionTypes = outgoingEventTypes.Select(type => type.GetAttribute<TimescaledbConnectionAttribute>()).Select(attr => attr.Connection).Distinct();
            
            var clients = timescaledbConnectionTypes.ToDictionary(type => type, type => (ITimescaledbClient)scope.Resolve(typeof(ITimescaledbClient<>).MakeGenericType(type)));

            return clients;
        }
    }
}
