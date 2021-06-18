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
            builder.RegisterType<TimescaledbBridge>().AsSelf().As<IBridge>().InstancePerMatchingLifetimeScope("runtime");
            builder.RegisterType<TimescaledbClient>().AsSelf().As<ITimescaledbClient>().InstancePerMatchingLifetimeScope("runtime");
        }
    }
}
