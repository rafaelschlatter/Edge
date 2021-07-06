using System;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.IotHub
{
    /// <summary>
    /// Marker interface for incoming IotHub events. By implementing this interface for a class, The IotHub module will
    /// set up a subscription to the IotHub connection class specified in the IotHubConnection attribute for the class. The class
    /// will be used for deserializing the incoming event.
    /// </summary>
    public interface IIotHubIncomingEvent : IEvent
    {
    }

    /// <summary>
    /// Marker interface for outgoing IotHub events. By implementing this interface for a class, The IotHub module will
    /// subscribe to this event, and send out the event to the IotHub connection class specified in the IotHubConnection attribute
    /// for the class, whenever a new event is produced. The class will be used for serializing the outgoing event.
    /// </summary>
    public interface IIotHubOutgoingEvent : IEvent
    {
    }

    /// <summary>
    /// Attribute for the IotHub input name. All classes implementing IIotHubIncomingEvent should use this annotation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IotHubConnectionAttribute : Attribute
    {
        public Type Connection { get; }
        public IotHubConnectionAttribute(Type connection)
        {
            Connection = connection;
        }
    }

}
