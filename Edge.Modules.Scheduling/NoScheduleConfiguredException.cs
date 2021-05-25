using System;

namespace RaaLabs.Edge.Modules.Scheduling
{
    public class NoScheduleConfiguredException : Exception
    {
        public Type EventType { get; set; }

        public NoScheduleConfiguredException(Type type)
            : base($"No schedule configuration found for event type '{type.Name}'")
        {
            EventType = type;
        }
    }
}
