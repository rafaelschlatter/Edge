using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Text;
using RaaLabs.Edge.Modules.EventHandling;
using Newtonsoft.Json.Serialization;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// Module for bridging EdgeHub output messages to Event Handling classes.
    /// </summary>
    class OutgoingEvents : Module
    {
        /// <summary>
        /// This function will detect event classes implementing IEventHubOutgoingEvent when they are being activated, and will
        /// set up the EventHub client to consume messages from these events.
        /// </summary>
        /// <param name="componentRegistry"></param>
        /// <param name="registration"></param>
        protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        {
            Type eventType;

            if (IsEventHandlerForEventHubOutgoingEvent(registration, out eventType))
            {
                registration.PipelineBuilding += (sender, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);
                        var eventSetupFunction = typeof(OutgoingEvents).GetMethod("SetupEventHubOutgoingEvents").MakeGenericMethod(eventType);
                        eventSetupFunction.Invoke(this, new object[] { c });
                    });
                };
            }
        }

        /// <summary>
        /// This function will set up EventHub to consume outgoing events from the specified event type T. For each
        /// message produced for this event type, the message will be serialized and sent to EventHub.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// 
        /// </summary>
        /// <typeparam name="T">The event handling class producing the outgoing output</typeparam>
        /// <param name="context">The autofac scope for resolving dependencies</param>
        public static void SetupEventHubOutgoingEvents<T>(ResolveRequestContext context)
            where T : IEvent
        {
            var client = context.Resolve<IEventHubModuleClient>();
            var logger = context.Resolve<ILogger>();
            EventHandling.EventHandler<T> eventHandler = (EventHandling.EventHandler<T>)context.Instance;
            var outputName = ((EventHubNameAttribute)typeof(T).GetCustomAttributes(typeof(EventHubNameAttribute), true).First()).EventHubName;
            eventHandler.Subscribe(async message =>
            {
                var outputString = JsonConvert.SerializeObject(message, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                var outputBytes = Encoding.UTF8.GetBytes(outputString);
                var outputMessage = new Message(outputBytes);

                await client.SendEventAsync(outputName, outputMessage);
            });
        }

        /// <summary>
        /// Test whether the type being activated implements IEventHubOutgoingEvent, and if it does, returns the concrete
        /// type being activated.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static bool IsEventHandlerForEventHubOutgoingEvent(IComponentRegistration registration, out Type eventType)
        {
            eventType = null;
            var eventHandlers = registration.Services
                .Where(s => s is IServiceWithType && typeof(IEventHandler).IsAssignableFrom(((IServiceWithType)s).ServiceType))
                .Select(s => ((IServiceWithType)s).ServiceType)
                .Where(eh => eh.GetGenericArguments().First().GetInterfaces().Any(i => typeof(IEventHubOutgoingEvent).IsAssignableFrom(i)))
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
