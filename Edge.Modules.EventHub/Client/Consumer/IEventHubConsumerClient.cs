using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EventHub.Client.Consumer
{
    public interface IEventHubConsumerClient<ConnectionType> : IEventHubConsumerClient, IReceiverClient<ConnectionType, EventData>
        where ConnectionType : IEventHubConnection
    {
    }

    public interface IEventHubConsumerClient : IReceiverClient<EventData>
    {
    }

    public delegate Task EventDataReceivedDelegate(Type connection, EventData data);
}
