using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// Plumbing module for connecting all event consumers to the corresponding EventHandler.
    /// </summary>
    class EventConsumers : Autofac.Module
    {
        /// <summary>
        /// This function will detect classes implementing IConsumeEvent when they are being activated, and will
        /// set up subscription to all the specified event types that they consume.
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            if (HasInterface<IConsumeEvent>(registration))
            {
                registration.PipelineBuilding += (sender, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);
                        SetupEventConsumptionForConsumer(c);
                    });
                };
            }
        }


        /// <summary>
        /// This function will set up all event subscriptions for a class instance upon activation.
        /// 
        /// </summary>
        /// <param name="context">The autofac resolve context</param>
        private void SetupEventConsumptionForConsumer(ResolveRequestContext context)
        {
            var consumer = (IConsumeEvent) context.Instance;
            List<Type> allMessageTypesToConsume = consumer.GetType().GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IConsumeEvent<>))
                .Select(i => i.GetGenericArguments().First())
                .ToList();

            SetupSubscriptions(context, consumer, allMessageTypesToConsume);
        }

        private void SetupSubscriptions(IComponentContext context, IConsumeEvent consumer, List<Type> messageTypesToConsume)
        {
            foreach (var messageType in messageTypesToConsume)
            {
                var subscribeFunction = typeof(EventConsumers).GetMethod("SubscribeToEvent").MakeGenericMethod(messageType);
                var unsubscriber = subscribeFunction.Invoke(this, new object[] { context, consumer });
            }
        }

        /// <summary>
        /// Set up subscription to a given event type T for a consumer.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// </summary>
        /// <typeparam name="T">The event type to subscribe to</typeparam>
        /// <param name="context">The Autofac context</param>
        /// <param name="consumer">The class which will consume the event</param>
        /// <returns></returns>
        public static Unsubscriber<T> SubscribeToEvent<T>(IComponentContext context, IConsumeEvent<T> consumer)
            where T : IEvent
        {
            var eventHandler = context.Resolve<EventHandler<T>>();
            return (Unsubscriber<T>)eventHandler.Subscribe(consumer);
        }

        private bool HasInterface<T>(IComponentRegistration registration)
        {
            return registration.Services.Any(s => s is IServiceWithType && typeof(T).IsAssignableFrom(((IServiceWithType)s).ServiceType));
        }
    }
}
