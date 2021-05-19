using System;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// Attribute for loading schedule from file
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScheduleFileAttribute : Attribute
    {
        /// <summary>
        /// The name of the file to load schedule from
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// A selector for which schedule objects in config to use. Set to null if all schedule objects will be used.
        /// </summary>
        public string Qualifier { get; }

        /// <summary>
        /// Creates an instance of <see cref="ScheduleFileAttribute"/>
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="qualifier"></param>
        public ScheduleFileAttribute(string filename, string qualifier = null)
        {
            Filename = filename;
            Qualifier = qualifier;
        }
    }

    /// <summary>
    /// Attribute for creating a schedule from a cron pattern
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SchedulePatternAttribute : Attribute
    {
        /// <summary>
        /// The cron pattern to use
        /// </summary>
        public string Pattern { get; }
        
        /// <summary>
        /// Creates an instance of <see cref="SchedulePatternAttribute"/>
        /// </summary>
        /// <param name="pattern"></param>
        public SchedulePatternAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }

    /// <summary>
    /// Attribute for creating a schedule from an interval
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScheduleIntervalAttribute : Attribute
    {
        /// <summary>
        /// The interval to use
        /// </summary>
        public double Interval { get; }

        /// <summary>
        /// Creates an instance of <see cref="ScheduleIntervalAttribute"/>
        /// </summary>
        /// <param name="interval"></param>
        public ScheduleIntervalAttribute(double interval)
        {
            Interval = interval;
        }
    }

    /// <summary>
    /// Attribute for creating a schedule from an interval
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

        /// <summary>
        /// The name of the file to load schedule from
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// A selector for which schedule objects in config to use. Set to null if all schedule objects will be used.
        /// </summary>
        public string Qualifier { get; set; }
    }

}
