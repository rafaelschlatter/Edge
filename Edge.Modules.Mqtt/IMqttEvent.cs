using System;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Mqtt
{
    /// <summary>
    /// Marker interface for incoming EventHub events. By implementing this interface for a class, The EventHub module will
    /// set up a subscription to the EventHub input with the name given in the InputName attribute for the class. The class
    /// will be used for deserializing the incoming event.
    /// </summary>
    public interface IMqttIncomingEvent : IEvent
    {
    }

    /// <summary>
    /// Marker interface for outgoing EventHub events. By implementing this interface for a class, The EventHub module will
    /// subscribe to this event, and send out the event to EventHub output with the name given in the OutputName attribute
    /// for the class, whenever a new event is produced. The class will be used for serializing the outgoing event.
    /// </summary>
    public interface IMqttOutgoingEvent : IEvent
    {
    }

    /// <summary>
    /// Attribute for the Mqtt broker connection class. All classes implementing IMqttIncomingEvent or IMqttOutgoingEvent should use this annotation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MqttBrokerConnectionAttribute : Attribute
    {
        public Type BrokerConnection { get; }
        public string Topic { get; }

        public MqttBrokerConnectionAttribute(Type brokerConnection, string topic)
        {
            BrokerConnection = brokerConnection;
            Topic = topic;
        }
    }
}
