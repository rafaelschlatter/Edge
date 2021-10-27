using Azure.Messaging.EventHubs;
using RaaLabs.Edge.Modules.EventHandling;
using System;

namespace RaaLabs.Edge.Modules.EventHub
{
    public interface IEventHubEventDataConverter
    {
        public IEvent ToEvent(Type connection, EventData data);
        public (Type connection, EventData data)? ToEventData(IEvent @event);
    }

}
