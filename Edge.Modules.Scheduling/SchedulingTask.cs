using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using RaaLabs.Edge.Modules.EventHandling;
using Serilog;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// Task for setting up scheduling
    /// </summary>
    public class SchedulingTask : IRunAsync
    {
        private readonly ILogger _logger;
        private readonly ILifetimeScope _scope;
        private readonly StdSchedulerFactory _schedulerFactory;
        private readonly EdgeJobFactory _jobFactory;
        private IScheduler _scheduler;
        private readonly IList<(ITrigger trigger, IJobDetail detail)> _scheduledJobs;

        /// <summary>
        /// Creates an instance of <see cref="SchedulerTask"/>
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="fs"></param>
        /// <param name="logger"></param>
        public SchedulingTask(ILifetimeScope scope, ILogger logger)
        {
            _logger = logger;
            _scope = scope;
            _schedulerFactory = new StdSchedulerFactory();
            _scheduledJobs = new List<(ITrigger, IJobDetail)>();
            _jobFactory = new EdgeJobFactory(scope);
        }

        /// <summary>
        /// Start scheduler
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            _scheduler = await _schedulerFactory.GetScheduler();
            _scheduler.JobFactory = _jobFactory;
            await _scheduler.Start();

            var scheduled = _scheduledJobs.Select(async job => await _scheduler.ScheduleJob(job.detail, job.trigger));
            var started = await Task.WhenAll(scheduled.ToArray());
            _logger.Information($"Scheduled {started.Length} jobs");
        }

        /// <summary>
        /// Setup scheduling for a class implementing <see cref="IScheduledEvent"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetupSchedulingForType<T>()
            where T : IScheduledEvent
        {
            var schedule = _scope.ResolveOptional<IScheduleForType<T>>();
            var scheduleAttribute = typeof(T).GetCustomAttribute<ScheduleAttribute>(true);

            var intervalValue = scheduleAttribute?.Interval ?? 0.0;
            var patternValue = scheduleAttribute?.Pattern ?? "";

            if (schedule == null && intervalValue == 0.0 && patternValue == "")
            {
                throw new NoScheduleConfiguredException(typeof(T));
            }

            if (intervalValue > 0.0) SetupSchedulingInterval<T>(intervalValue);
            if (patternValue != "") SetupSchedulingPattern<T>(patternValue);

            if (schedule != null)
            {
                var allSchedules = schedule.Schedules;
                var intervals = allSchedules
                    .Where(s => IsInterval(s.Value))
                    .ToDictionary(s => s.Key, s => s.Value);

                var patterns = allSchedules
                    .Where(s => IsPattern(s.Value))
                    .ToDictionary(s => s.Key, s => s.Value as Pattern<T>);

                foreach (var (key, interval) in intervals)
                {
                    var intervall = interval as Interval<T>;
                    _jobFactory.AddInstance(key, intervall.Payload);
                    SetupSchedulingInterval<T>(intervall.Value, key);
                }

                foreach (var (key, pattern) in patterns)
                {
                    _jobFactory.AddInstance(key, pattern.Payload);
                    SetupSchedulingPattern<T>(pattern.Value, key);
                }
            }
        }

        private static bool IsInterval(ISchedule schedule)
        {
            if (!schedule.GetType().IsGenericType) return false;
            return schedule.GetType().GetGenericTypeDefinition() == typeof(Interval<>);
        }

        private static bool IsPattern(ISchedule schedule)
        {
            if (!schedule.GetType().IsGenericType) return false;
            return schedule.GetType().GetGenericTypeDefinition() == typeof(Pattern<>);
        }


        /// <summary>
        /// Set up scheduling for an interval
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="interval"></param>
        /// <param name="key">key identifier for the job</param>
        private void SetupSchedulingInterval<T>(double interval, string key = null)
            where T : IScheduledEvent
        {
            var jobBuilder = JobBuilder.Create<ScheduledJob<T>>();

            if (key != null)
            {
                jobBuilder = jobBuilder.WithIdentity(key);
            }

            var job = jobBuilder.Build();
            var trigger = TriggerBuilder.Create()
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(System.TimeSpan.FromSeconds(interval))
                    .RepeatForever())
                .Build();

            _scheduledJobs.Add((trigger, job));
        }

        /// <summary>
        /// Set up scheduling for a pattern
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pattern"></param>
        /// <param name="key">key identifier for the job</param>
        private void SetupSchedulingPattern<T>(string pattern, string key = null)
            where T : IScheduledEvent
        {
            var jobBuilder = JobBuilder.Create<ScheduledJob<T>>();

            if (key != null)
            {
                jobBuilder = jobBuilder.WithIdentity(key);
            }

            var job = jobBuilder.Build();

            var trigger = TriggerBuilder.Create()
                .StartNow()
                .WithCronSchedule(pattern)
                .Build();

            _scheduledJobs.Add((trigger, job));
        }

        /// <summary>
        /// Factory for creating job instances for the different schedule event types.
        /// </summary>
        private class EdgeJobFactory : IJobFactory
        {
            private readonly ILifetimeScope _scope;
            private readonly Dictionary<System.Type, IJobFactory> _jobFactories;
            private readonly Dictionary<System.Type, Dictionary<string, object>> _instancesForFactories;
            
            public EdgeJobFactory(ILifetimeScope scope)
            {
                _jobFactories = new Dictionary<System.Type, IJobFactory>();
                _instancesForFactories = new Dictionary<System.Type, Dictionary<string, object>>();
                _scope = scope;
            }

            public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            {
                var jobType = bundle.JobDetail.JobType;
                var eventType = jobType.GetGenericArguments().First();

                var factory = GetFactory(eventType);

                return factory.NewJob(bundle, scheduler);
            }

            public void ReturnJob(IJob job)
            {
                (job as System.IDisposable)?.Dispose();
            }

            /// <summary>
            /// Add an instance of type T with a given key. This instance will be used in the subfactory for the given
            /// type every time this factory will create an instance with the specified key.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="key"></param>
            /// <param name="instance"></param>
            public void AddInstance<T>(string key, T instance)
                where T : IScheduledEvent
            {
                if (!_instancesForFactories.ContainsKey(typeof(T)))
                {
                    _instancesForFactories.Add(typeof(T), new Dictionary<string, object>());
                }
                _instancesForFactories[typeof(T)].Add(key, instance);
            }

            private IJobFactory GetFactory(System.Type eventType)
            {
                IJobFactory jobFactory;
                if (!_jobFactories.TryGetValue(eventType, out jobFactory))
                {
                    try
                    {
                        jobFactory = (IJobFactory)_scope.Resolve(typeof(JobFactoryForType<>).MakeGenericType(eventType));
                        var instances = _instancesForFactories.GetValueOrDefault(eventType);

                        if (instances != null)
                        {
                            var method = typeof(JobFactoryForType<>).MakeGenericType(eventType).GetMethod("SetInstances");
                            method.Invoke(jobFactory, new object[] { instances });
                        }
                        _jobFactories[eventType] = jobFactory;

                    }
                    catch(System.Exception e)
                    {
                        System.Console.WriteLine($"Exception: {e.Message}");
                        throw;
                    }
                }

                return jobFactory;
            }

            private class JobFactoryForType<T> : IJobFactory
                where T : IScheduledEvent
            {
                private readonly EventHandler<T> _eventHandler;
                private readonly T _defaultInstance;
                private Dictionary<string, T> _instances;

                public JobFactoryForType(ILifetimeScope scope)
                {
                    _instances = new Dictionary<string, T>();
                    _defaultInstance = System.Activator.CreateInstance<T>();
                    _eventHandler = (EventHandler<T>)scope.Resolve(typeof(EventHandler<>).MakeGenericType(typeof(T)));
                }

                public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
                {
                    if (_instances.TryGetValue(bundle.JobDetail.Key.Name, out T instance))
                    {
                        return new ScheduledJob<T>(_eventHandler, instance);
                    }
                    else
                    {
                        return new ScheduledJob<T>(_eventHandler, _defaultInstance);
                    }
                }

                public void ReturnJob(IJob job)
                {
                    (job as System.IDisposable)?.Dispose();
                }

                public void SetInstances(Dictionary<string, object> instances)
                {
                    _instances = instances.ToDictionary(_ => _.Key, _ => (T)_.Value);
                }
            }
        }

    }
}
