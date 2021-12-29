using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EventHub.Client.Producer
{
    public interface IEventHubProducerClient<ConnectionType> : IEventHubProducerClient, ISenderClient<ConnectionType, EventData>, IBatchedSenderClient<ConnectionType, EventData>
        where ConnectionType : IEventHubConnection
    {
    }

    public interface IEventHubProducerClient : ISenderClient<EventData>, IBatchedSenderClient<EventData>
    {
    }

}
