using System.Collections.Generic;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// Interface for class providing scheduling mapping for a type
    /// </summary>
    /// <typeparam name="T">the type to provide scheduling for</typeparam>
    public interface IScheduleForType<T>
        where T : IScheduledEvent
    {
        /// <summary>
        /// Property for all configured schedules for a given type
        /// </summary>
        public Dictionary<string, ISchedule> Schedules { get; }
    }

    public interface ISchedule { }

    /// <summary>
    /// A scheduled interval
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class Interval<T> : ISchedule
    {
        /// <summary>
        /// The interval, in seconds
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// The contents for this event type instance
        /// </summary>
        public T Payload { get; set; }
    }

    /// <summary>
    /// A scheduled pattern
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class Pattern<T> : ISchedule
    {
        /// <summary>
        /// The cron pattern
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The contents for this event type instance
        /// </summary>
        public T Payload { get; set; }
    }
}
