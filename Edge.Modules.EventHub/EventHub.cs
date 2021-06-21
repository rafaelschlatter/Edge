using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.EventHub.Client;
using RaaLabs.Edge.Modules.EventHub.Client.Consumer;
using RaaLabs.Edge.Modules.EventHub.Client.Producer;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// The module for registering the EventHub bridge for the application.
    /// </summary>
    public class EventHub : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EventHubConsumerClient<>))
                .AsSelf()
                .As(typeof(IEventHubConsumerClient<>))
                .InstancePerRuntime();

            builder.RegisterGeneric(typeof(EventHubProducerClient<>))
                .AsSelf()
                .As(typeof(IEventHubProducerClient<>))
                .InstancePerRuntime();

            builder.RegisterBridge<EventHubBridge>();
        }
    }
}
