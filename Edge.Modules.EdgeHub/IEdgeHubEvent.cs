using System;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// Marker interface for incoming EdgeHub events. By implementing this interface for a class, The EdgeHub module will
    /// set up a subscription to the EdgeHub input with the name given in the InputName attribute for the class. The class
    /// will be used for deserializing the incoming event.
    /// </summary>
    public interface IEdgeHubIncomingEvent : IEvent
    {
    }

    /// <summary>
    /// Marker interface for outgoing EdgeHub events. By implementing this interface for a class, The EdgeHub module will
    /// subscribe to this event, and send out the event to EdgeHub output with the name given in the OutputName attribute
    /// for the class, whenever a new event is produced. The class will be used for serializing the outgoing event.
    /// </summary>
    public interface IEdgeHubOutgoingEvent : IEvent
    {
    }

    /// <summary>
    /// Attribute for the EdgeHub input name. All classes implementing IEdgeHubIncomingEvent should use this annotation.
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
    /// Attribute for the EdgeHub output name. All classes implementing IEdgeHubOutgoingEvent should use this annotation.
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
