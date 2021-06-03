using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// Plumbing module for connecting all event producers to the corresponding EventHandler.
    /// </summary>
    class EventProducers : Autofac.Module
    {
        /// <summary>
        /// This function will detect classes implementing IProduceEvent when they are being activated, and will
        /// set up publishing to all the specified event types that they produce.
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            if (HasInterface<IProduceEvent>(registration))
            {
                registration.PipelineBuilding += (sender, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);
                        SetupEventProductionForProducer(c);
                    });
                };
            }
        }

        /// <summary>
        /// This function will set up all event publishing for a class instance upon activation.
        /// 
        /// </summary>
        /// <param name="context">The autofac resolve context</param>
        private void SetupEventProductionForProducer(ResolveRequestContext context)
        {
            var producer = (IProduceEvent)context.Instance;
            List<Type> allMessageTypesToProduce = producer.GetType().GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IProduceEvent<>))
                .Select(i => i.GetGenericArguments().First())
                .ToList();

            var emitters = producer.GetType().GetEvents()
                .Where(i => i.EventHandlerType.IsGenericType)
                .Where(i => i.EventHandlerType.GetGenericTypeDefinition() == typeof(EventEmitter<>) || i.EventHandlerType.GetGenericTypeDefinition() == typeof(AsyncEventEmitter<>))
                .ToList();

            var messageTypeToEmitter = emitters.ToDictionary(em => em.EventHandlerType.GetGenericArguments().First(), em => em);
            if (!allMessageTypesToProduce.All(m => messageTypeToEmitter.ContainsKey(m)))
            {
                throw new Exception($"{producer.GetType().Name} is missing events for some messages it claims to produce.");
            }

            SetupEventEmitting(context, producer, messageTypeToEmitter);
        }

        private void SetupEventEmitting(IComponentContext context, IProduceEvent producer, Dictionary<Type, EventInfo> messageTypeToEmitter)
        {
            foreach (var (messageType, emitter) in messageTypeToEmitter)
            {
                var eventSetupFunction = GetType().GetMethod("SetupEventEmitter").MakeGenericMethod(messageType);
                eventSetupFunction.Invoke(this, new object[] { context, producer, emitter });
            }
        }

        /// <summary>
        /// Set up publishing to a given event type T for a producer.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// </summary>
        /// <typeparam name="T">The event type to publish to</typeparam>
        /// <param name="context">The Autofac context</param>
        /// <param name="emitter">The class which will emit the event</param>
        /// <returns></returns>
        public static void SetupEventEmitter<T>(IComponentContext context, IProduceEvent producer, EventInfo emitter)
            where T : IEvent
        {
            bool isAsync = emitter.EventHandlerType.GetGenericTypeDefinition() == typeof(AsyncEventEmitter<>);

            var eventHandler = context.Resolve<EventHandler<T>>();

            Delegate eventDelegate;
            if (isAsync)
            {
                eventDelegate = Delegate.CreateDelegate(typeof(AsyncEventEmitter<T>), eventHandler, "ProduceAsync", false);
            }
            else
            {
                eventDelegate = Delegate.CreateDelegate(typeof(EventEmitter<T>), eventHandler, "Produce", false);
            }
            emitter.AddEventHandler(producer, eventDelegate);
        }

        private bool HasInterface<T>(IComponentRegistration registration)
        {
            return registration.Services.Any(s => s is IServiceWithType && typeof(T).IsAssignableFrom(((IServiceWithType)s).ServiceType));
        }
    }
}
