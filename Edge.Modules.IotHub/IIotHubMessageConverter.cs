using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;
using System;

namespace RaaLabs.Edge.Modules.IotHub
{
    public interface IIotHubMessageConverter
    {
        public IEvent ToEvent(Type connection, Message message);
        public (Type connection, Message message)? ToMessage(IEvent @event);
    }

}
