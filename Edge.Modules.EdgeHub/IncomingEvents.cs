using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaaLabs.Edge.Modules.EventHandling;
using Serilog;

namespace RaaLabs.Edge.Modules.EdgeHub
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

            if (IsEventHandlerForEdgeHubIncomingEvent(registration, out eventType))
            {
                registration.PipelineBuilding += (sender, pipeline) =>
                {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) =>
                    {
                        next(c);
                        var eventSetupFunction = typeof(IncomingEvents).GetMethod("SetupEdgeHubIncomingEvents").MakeGenericMethod(eventType);
                        eventSetupFunction.Invoke(this, new object[] { c });
                    });
                };
            }
        }

        /// <summary>
        /// This function will set up EdgeHub to publish incoming events to the specified event type T.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// 
        /// </summary>
        /// <typeparam name="T">The event handling class receiving the incoming input</typeparam>
        /// <param name="context">The autofac scope for resolving dependencies</param>
        public static void SetupEdgeHubIncomingEvents<T>(ResolveRequestContext context)
            where T : IEvent
        {
            var logger = context.Resolve<ILogger>();
            var client = context.Resolve<IIotModuleClient>();
            EventHandling.EventHandler<T> eventHandler = (EventHandling.EventHandler<T>)context.Instance;
            var inputName = ((InputNameAttribute)typeof(T).GetCustomAttributes(typeof(InputNameAttribute), true).First()).InputName;

            client.SetInputMessageHandlerAsync(inputName, async (message, context) =>
            {
                logger.Information("Handling incoming event for input {InputName}", inputName);
                return await HandleSubscriber(eventHandler, message, logger);
            }, null);
        }

        /// <summary>
        /// Deserialize incoming EdgeHub message and publish the event to the application.
        /// </summary>
        /// <typeparam name="T">The event type to deserialize the incoming data to</typeparam>
        /// <param name="eventHandler">The event handler for the given event type</param>
        /// <param name="message">The incoming EdgeHub message</param>
        /// <param name="logger">a logger</param>
        /// <returns></returns>
        async static Task<MessageResponse> HandleSubscriber<T>(EventHandling.EventHandler<T> eventHandler, Message message, ILogger logger)
            where T : IEvent
        {
            try
            {
                var messageBytes = message.GetBytes();
                var messageString = Encoding.UTF8.GetString(messageBytes);

                var deserialized = JsonConvert.DeserializeObject<T>(messageString);
                eventHandler.Produce(deserialized);

                logger.Information("New incoming message: {IncomingMessage}", messageString);

                await Task.CompletedTask;
                return MessageResponse.Completed;
            }
            catch (Exception ex)
            {
                return MessageResponse.Abandoned;
            }
        }

        /// <summary>
        /// Test whether the type being activated implements IEdgeHubIncomingEvent, and if it does, returns the concrete
        /// type being activated.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private static bool IsEventHandlerForEdgeHubIncomingEvent(IComponentRegistration registration, out Type eventType)
        {
            eventType = null;
            var eventHandlers = registration.Services
                .Where(s => s is IServiceWithType && typeof(IEventHandler).IsAssignableFrom(((IServiceWithType)s).ServiceType))
                .Select(s => ((IServiceWithType)s).ServiceType)
                .Where(eh => eh.GetGenericArguments().First().GetInterfaces().Any(i => typeof(IEdgeHubIncomingEvent).IsAssignableFrom(i)))
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
