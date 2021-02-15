using Autofac;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    public class EdgeHub : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            if (IotEdgeHelpers.IsRunningInIotEdge())
            {
                builder.RegisterType<IotModuleClient>().As<IIotModuleClient>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<NullIotModuleClient>().As<IIotModuleClient>().InstancePerLifetimeScope();
            }

            builder.RegisterModule<IncomingEvents>();
            builder.RegisterModule<OutgoingEvents>();
        }
    }
}
