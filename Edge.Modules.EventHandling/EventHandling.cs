using Autofac;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EventHandling
{
    public class EventHandling : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<EventHandlers>();
            builder.RegisterModule<EventProducers>();
            builder.RegisterModule<EventConsumers>();
        }
    }
}