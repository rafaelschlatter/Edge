using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Drivers
{

    [Schedule(Pattern = "*/1 * * * * ?")]
    public class CronEvent : IScheduledEvent
    {
    }

    [Schedule(Interval = 0.25)]
    public class IntervalEvent : IScheduledEvent
    {
    }

    public class TypeScheduledEvent : IScheduledEvent
    {
        public List<string> Tags { get; set; }
    }

    public class ScheduleConfig : IScheduleForType<TypeScheduledEvent>
    {
        public Dictionary<string, Interval<TypeScheduledEvent>> GetIntervals()
        {
            return new Dictionary<string, Interval<TypeScheduledEvent>>();
        }

        public Dictionary<string, Pattern<TypeScheduledEvent>> GetPatterns()
        {
            return new Dictionary<string, Pattern<TypeScheduledEvent>>
            {
                { "onceASecond", new Pattern<TypeScheduledEvent>{ Value = "*/1 * * * * ?", Payload = new TypeScheduledEvent { Tags = new List<string>{ "tag-1", "tag-2" } } } }
            };
        }
    }

    public class ScheduleHandler : IConsumeEvent<CronEvent>, IConsumeEvent<IntervalEvent>, IConsumeEvent<TypeScheduledEvent>
    {
        private readonly EventCounter _eventCounter;

        public ScheduleHandler(EventCounter eventCounter)
        {
            _eventCounter = eventCounter;
        }

        public void Handle(CronEvent @event)
        {
            _eventCounter.IncrementEventCountForType<CronEvent>();
        }

        public void Handle(IntervalEvent @event)
        {
            _eventCounter.IncrementEventCountForType<IntervalEvent>();
        }

        public void Handle(TypeScheduledEvent @event)
        {
            _eventCounter.IncrementEventCountForType<TypeScheduledEvent>();
        }
    }

    public class EventCounter
    {
        private Dictionary<Type, int> _eventsProducedForType;

        public EventCounter()
        {
            _eventsProducedForType = new Dictionary<Type, int>();
        }

        public void IncrementEventCountForType<T>()
        {
            var type = typeof(T);
            if (!_eventsProducedForType.ContainsKey(type)) _eventsProducedForType[type] = 0;

            _eventsProducedForType[type]++;
        }

        public int GetEventsProducedForType(Type type)
        {
            return _eventsProducedForType.GetValueOrDefault(type);
        }
    }
}
