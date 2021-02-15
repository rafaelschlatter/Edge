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
    class IncomingEvents : Module
    {
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
