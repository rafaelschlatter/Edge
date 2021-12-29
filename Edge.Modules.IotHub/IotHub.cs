using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.IotHub.Client;

namespace RaaLabs.Edge.Modules.IotHub
{
    /// <summary>
    /// The module for registering the IotHub bridge for the application.
    /// </summary>
    public class IotHub : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(IotHubClient<>))
                .AsSelf()
                .As(typeof(IIotHubClient<>))
                .InstancePerRuntime();

            builder.RegisterType<IotHubMessageConverter>()
                .AsSelf()
                .As<IIotHubMessageConverter>()
                .InstancePerRuntime();

            builder.RegisterBridge<IotHubBridge>();
        }
    }
}
