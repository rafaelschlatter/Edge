using Autofac;

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
                builder.RegisterType<IotModuleClient>().As<IIotModuleClient>().AsImplementedInterfaces().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<NullIotModuleClient>().As<IIotModuleClient>().InstancePerLifetimeScope();
            }
            builder.RegisterType<IncomingEventsSubscriberTask>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterModule<IncomingEvents>();
            builder.RegisterModule<OutgoingEvents>();
        }
    }
}
