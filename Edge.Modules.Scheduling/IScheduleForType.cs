using System.Collections.Generic;

namespace RaaLabs.Edge.Modules.Scheduler
{
    /// <summary>
    /// Interface for class providing scheduling mapping for a type
    /// </summary>
    /// <typeparam name="T">the type to provide scheduling for</typeparam>
    public interface IScheduleForType<T>
        where T : IScheduledEvent
    {
        /// <summary>
        /// Get all intervals scheduled for event type
        /// </summary>
        /// <returns>all intervals scheduled for event type</returns>
        public Dictionary<string, Interval<T>> GetIntervals();

        /// <summary>
        /// Get all patterns scheduled for event type
        /// </summary>
        /// <returns>all patterns scheduled for event type</returns>
        public Dictionary<string, Pattern<T>> GetPatterns();
    }

    /// <summary>
    /// A scheduled interval
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class Interval<T>
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
    public class Pattern<T>
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
