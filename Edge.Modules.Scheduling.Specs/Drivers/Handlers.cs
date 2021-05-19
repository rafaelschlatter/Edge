using Autofac;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Drivers
{

    [Schedule(Pattern = "*/1 * * * * ?")]
    class CronEvent : IScheduledEvent
    {
    }

    [Schedule(Interval = 0.25)]
    class IntervalEvent : IScheduledEvent
    {
    }

    class CronEventHandler : IConsumeEvent<CronEvent>
    {
        public int NumEvents { get; set; }

        public virtual void Handle(CronEvent @event)
        {
            NumEvents++;
        }
    }
}
