using Autofac;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// The module for registering the EdgeHub bridge for the application.
    /// </summary>
    public class EdgeHub : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            // If the application is running in field, use an actual IotModuleClient. If running locally,
            // use a mock client.
            if (IotEdgeHelpers.IsRunningInIotEdge())
            {
                builder.RegisterType<IotModuleClient>().As<IIotModuleClient>().InstancePerMatchingLifetimeScope("runtime"); //.InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<NullIotModuleClient>().As<IIotModuleClient>().InstancePerMatchingLifetimeScope("runtime"); //.InstancePerLifetimeScope();
            }
            builder.RegisterType<EdgeHubBridge>().AsSelf().As<IBridge>().InstancePerMatchingLifetimeScope("runtime");
        }
    }
}
