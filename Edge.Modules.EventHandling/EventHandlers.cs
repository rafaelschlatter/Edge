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
            builder.RegisterType<SetupEventHandlers>().AsSelf().As<IBootloader>().SingleInstance();
        }
    }
}
