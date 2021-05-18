using Quartz;
using RaaLabs.Edge.Modules.EventHandling;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Scheduler
{
    /// <summary>
    /// A scheduled jub
    /// </summary>
    public class ScheduledJob<T> : IJob
        where T : IScheduledEvent
    {
        private readonly EventHandler<T> _eventHandler;
        private readonly T _instance;
        /// <summary>
        /// Creates an instance of <see cref="ScheduledJob{T}"/>
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="instance"></param>
        public ScheduledJob(EventHandler<T> eventHandler, T instance)
        {
            _eventHandler = eventHandler;
            _instance = instance;
        }

        /// <summary>
        /// Run the job
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            _eventHandler.Produce(_instance);
            await Task.CompletedTask;
        }
    }
}
