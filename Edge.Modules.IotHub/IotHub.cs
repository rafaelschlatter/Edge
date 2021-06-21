using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.IotHub.Client;
using RaaLabs.Edge.Modules.IotHub.Client.Consumer;
using RaaLabs.Edge.Modules.IotHub.Client.Producer;

namespace RaaLabs.Edge.Modules.IotHub
{
    /// <summary>
    /// The module for registering the EventHub bridge for the application.
    /// </summary>
    public class IotHub : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(IotHubConsumerClient<>))
                .AsSelf()
                .As(typeof(IIotHubConsumerClient<>))
                .InstancePerRuntime();

            builder.RegisterGeneric(typeof(IotHubProducerClient<>))
                .AsSelf()
                .As(typeof(IIotHubProducerClient<>))
                .InstancePerRuntime();

            builder.RegisterBridge<IotHubBridge>();
        }
    }
}
