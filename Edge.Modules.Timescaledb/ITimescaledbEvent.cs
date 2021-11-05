using System;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Timescaledb
{

    /// <summary>
    /// Marker interface for outgoing Timescaledb events. 
    /// </summary>
    public interface ITimescaledbOutgoingEvent : IEvent
    {
    }

    /// <summary>
    /// Attribute for the Timescaledb connection class. All classes implementing ITimescaleDbOutgoingEvent should use this annotation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TimescaledbConnectionAttribute : Attribute
    {
        public Type Connection { get; }

        public TimescaledbConnectionAttribute(Type connection)
        {
            Connection = connection;
        }
    }

}
