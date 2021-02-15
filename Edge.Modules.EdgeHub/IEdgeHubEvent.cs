using System;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    public interface IEdgeHubIncomingEvent : IEvent
    {
    }

    public interface IEdgeHubOutgoingEvent : IEvent
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InputNameAttribute : Attribute
    {
        public string InputName { get; }
        public InputNameAttribute(string inputName)
        {
            InputName = inputName;
        }
    }

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
