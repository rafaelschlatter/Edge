using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using System;
using System.Linq;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// Module for bridging EdgeHub input messages to Event Handling classes.
    /// </summary>
    class IncomingEvents : Module
    {
        /// <summary>
        /// This function will detect event classes implementing IEdgeHubIncomingEvent when they are being activated, and will
        /// set up the EdgeHub client to publish messages to these events.
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            Type eventType;

            if (IsEventHandlerForEventHubIncomingEvent(registration, out eventType))
            {
                registration.PipelineBuilding += (sender, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);
                        var eventSetupFunction = typeof(IncomingEvents).GetMethod("SetupEventHubIncomingEvents").MakeGenericMethod(eventType);
                        eventSetupFunction.Invoke(this, new object[] { c });
                    });
                };
            }
        }

        /// <summary>
        /// This function will set up EventHub to publish incoming events to the specified event type T.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// 
        /// </summary>
        /// <typeparam name="T">The event handling class receiving the incoming input</typeparam>
        /// <param name="context">The autofac scope for resolving dependencies</param>
        public static void SetupEventHubIncomingEvents<T>(ResolveRequestContext context)
            where T : IEvent
        {
            var subscriberTask = context.Resolve<IncomingEventsSubscriberTask>();

            subscriberTask.SetupSubscriptionForEventHandler<T>();
        }

        /// <summary>
        /// Test whether the type being activated implements IEventHubIncomingEvent, and if it does, returns the concrete
        /// type being activated.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static bool IsEventHandlerForEventHubIncomingEvent(IComponentRegistration registration, out Type eventType)
        {
            eventType = null;
            var eventHandlers = registration.Services
                .Where(s => s is IServiceWithType && typeof(IEventHandler).IsAssignableFrom(((IServiceWithType)s).ServiceType))
                .Select(s => ((IServiceWithType)s).ServiceType)
                .Where(eh => eh.GetGenericArguments().First().GetInterfaces().Any(i => typeof(IEventHubIncomingEvent).IsAssignableFrom(i)))
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
