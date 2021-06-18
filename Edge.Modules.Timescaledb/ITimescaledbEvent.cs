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

}
