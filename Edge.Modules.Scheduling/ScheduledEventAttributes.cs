using System;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// Attribute for creating a schedule from either an interval or a pattern
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScheduleAttribute : Attribute
    {
        /// <summary>
        /// The interval to use
        /// </summary>
        public double Interval { get; set; }

        /// <summary>
        /// The cron pattern to use
        /// </summary>
        public string Pattern { get; set; }
    }

}
