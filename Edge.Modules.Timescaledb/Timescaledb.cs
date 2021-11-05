using Autofac;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    /// <summary>
    /// The module for registering the Timescaledb bridge for the application.
    /// </summary>
    public class Timescaledb : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(TimescaledbClient<>))
                .AsSelf()
                .As(typeof(ITimescaledbClient<>))
                .InstancePerRuntime();

            builder.RegisterType<TimescaledbBridge>().AsSelf().As<IBridge>().InstancePerMatchingLifetimeScope("runtime");
        }
    }
}
