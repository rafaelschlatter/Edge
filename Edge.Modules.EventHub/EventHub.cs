using Autofac;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// The module for registering the EventHub bridge for the application.
    /// </summary>
    public class EventHub : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            // If the application is running in field, use an actual IotModuleClient. If running locally,
            // use a mock client.
            if (IotEdgeHelpers.IsRunningInIotEdge())
            {
                builder.RegisterType<EventHubModuleClient>().As<IEventHubModuleClient>().AsImplementedInterfaces().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<NullEventHubModuleClient>().As<IEventHubModuleClient>().InstancePerLifetimeScope();
            }
            builder.RegisterType<IncomingEventsSubscriberTask>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterModule<IncomingEvents>();
            builder.RegisterModule<OutgoingEvents>();
        }
    }
}
