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
        private Dictionary<string, ISchedule> _schedules;

        public ScheduleConfig()
        {
            _schedules = new Dictionary<string, ISchedule>
            {
                { "onceASecond", new Pattern<TypeScheduledEvent>{ Value = "*/1 * * * * ?", Payload = new TypeScheduledEvent { Tags = new List<string>{ "tag-1", "tag-2" } } } },
                { "fourTimesASecond", new Interval<TypeScheduledEvent>{ Value = 0.25, Payload = new TypeScheduledEvent { Tags = new List<string>{ "tag-3", "tag-4" } } } }
            };
        }

        public Dictionary<string, ISchedule> Schedules => _schedules;
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
            _eventCounter.RegisterEventForType(@event);
        }

        public void Handle(IntervalEvent @event)
        {
            _eventCounter.RegisterEventForType(@event);
        }

        public void Handle(TypeScheduledEvent @event)
        {
            _eventCounter.RegisterEventForType(@event);
        }
    }

    public class EventCounter
    {
        private Dictionary<Type, List<object>> _eventsProducedForType;

        public EventCounter()
        {
            _eventsProducedForType = new Dictionary<Type, List<object>>();
        }

        public void RegisterEventForType<T>(T @event)
        {
            var type = typeof(T);
            if (!_eventsProducedForType.ContainsKey(type)) _eventsProducedForType[type] = new List<object>();

            _eventsProducedForType[type].Add(@event);
        }

        public List<object> GetEventsProducedForType(Type type)
        {
            return _eventsProducedForType.GetValueOrDefault(type);
        }
    }
}
