using Autofac;
using System;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// Module registering a generic EventHandler<> for the application. This means that every time a component
    /// requires a type EventHandler<T>, Autofac will know how to create this type.
    /// </summary>
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
