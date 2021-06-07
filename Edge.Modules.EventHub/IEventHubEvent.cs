using System;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// Marker interface for incoming EventHub events. By implementing this interface for a class, The EventHub module will
    /// set up a subscription to the EventHub input with the name given in the InputName attribute for the class. The class
    /// will be used for deserializing the incoming event.
    /// </summary>
    public interface IEventHubIncomingEvent : IEvent
    {
    }

    /// <summary>
    /// Marker interface for outgoing EventHub events. By implementing this interface for a class, The EventHub module will
    /// subscribe to this event, and send out the event to EventHub output with the name given in the OutputName attribute
    /// for the class, whenever a new event is produced. The class will be used for serializing the outgoing event.
    /// </summary>
    public interface IEventHubOutgoingEvent : IEvent
    {
    }

    /// <summary>
    /// Attribute for the EventHub input name. All classes implementing IEventHubIncomingEvent should use this annotation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InputNameAttribute : Attribute
    {
        public string InputName { get; }
        public InputNameAttribute(string inputName)
        {
            InputName = inputName;
        }
    }

    /// <summary>
    /// Attribute for the EventHub output name. All classes implementing IEventHubOutgoingEvent should use this annotation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class OutputNameAttribute : Attribute
    {
        public string OutputName { get; }
        public OutputNameAttribute(string outputName)
        {
            OutputName = outputName;
        }
    }
}
