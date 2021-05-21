using System;
using System.Collections.Generic;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Drivers
{
    public class TypeMapping : Dictionary<string, Type>
    {
        public TypeMapping()
        {
            Add("EventHandling", typeof(EventHandling.EventHandling));
            Add("Scheduling", typeof(Scheduling));
            Add("ScheduleHandler", typeof(ScheduleHandler));
            Add("EventCounter", typeof(EventCounter));
            Add("IntervalEvent", typeof(IntervalEvent));
            Add("CronEvent", typeof(CronEvent));
            Add("TypeScheduledEvent", typeof(TypeScheduledEvent));
            Add("ScheduleConfig", typeof(ScheduleConfig));
        }
    }
}
