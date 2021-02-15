using Autofac;
using System;

namespace RaaLabs.Edge.Modules.EventHandling
{
    class EventHandlers : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register one EventHandler<> per event type
            builder.RegisterGeneric((ctxt, types, parameters) =>
            {
                return Activator.CreateInstance(typeof(EventHandler<>).MakeGenericType(types));
            }).As(typeof(EventHandler<>)).InstancePerLifetimeScope();
        }
    }
}
