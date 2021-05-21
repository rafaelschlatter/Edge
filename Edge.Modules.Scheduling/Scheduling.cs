using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// 
    /// </summary>
    public class Scheduling : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SchedulingTask>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
        }

        /// <summary>
        /// This function will detect event classes implementing IEdgeHubIncomingEvent when they are being activated, and will
        /// set up the EdgeHub client to publish messages to these events.
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            if (IsHandlerForScheduledEvent(registration, out Type eventType))
            {
                registration.PipelineBuilding += (sender, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);
                        var eventSetupFunction = GetType().GetMethod("SetupSchedulingEvents").MakeGenericMethod(eventType);
                        eventSetupFunction.Invoke(this, new object[] { c });
                    });
                };

            }

        }

        /// <summary>
        /// This function will set up the application to publish scheduled events to the specified event type T.
        /// 
        /// IMPORTANT: This function will appear to not be in use, but will be called at runtime using reflection.
        /// 
        /// </summary>
        /// <typeparam name="T">The event handling class receiving the incoming input</typeparam>
        /// <param name="context">The autofac scope for resolving dependencies</param>
        public static void SetupSchedulingEvents<T>(ResolveRequestContext context)
            where T : IScheduledEvent
        {
            var schedulerTask = context.Resolve<SchedulingTask>();

            schedulerTask.SetupSchedulingForType<T>();
        }

        /// <summary>
        /// Test whether the type being activated implements IEdgeHubIncomingEvent, and if it does, returns the concrete
        /// type being activated.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static bool IsHandlerForScheduledEvent(IComponentRegistration registration, out Type eventType)
        {
            eventType = null;
            var eventHandlers = registration.Services
                .Where(s => s is IServiceWithType && typeof(IEventHandler).IsAssignableFrom(((IServiceWithType)s).ServiceType))
                .Select(s => ((IServiceWithType)s).ServiceType)
                .Where(eh => eh.GetGenericArguments().First().GetInterfaces().Any(i => typeof(IScheduledEvent).IsAssignableFrom(i)))
                .ToList();

            if (eventHandlers.Count == 0)
            {
                return false;
            }

            eventType = eventHandlers.Select(eh => eh.GetGenericArguments().First()).First();

            return true;
        }

    }
}
