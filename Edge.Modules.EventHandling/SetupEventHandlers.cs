using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// This class will add support for <see cref="EventHandlers"/> to the application.
    /// </summary>
    public class SetupEventHandlers : IBootloader, IPreRegistrationStage, IRegistrationStage, IPostRegistrationStage
    {
        private IList<Type> _allEventTypes;
        private IList<IEventHandler> _eventHandlers;

        public Status Status { get; private set; } = Status.Ready;


        public void PreRegistration(ILifetimeScope oldScope)
        {
            var assemblies = oldScope.Resolve<IEnumerable<Assembly>>();
            var allTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Distinct()
                .ToList();

            _allEventTypes = allTypes
                .Where(type => type.IsAssignableTo<IEvent>())
                .ToList();
        }

        public void RegistrationStage(ContainerBuilder builder)
        {
            foreach (var type in _allEventTypes)
            {
                builder.RegisterType(typeof(EventHandler<>).MakeGenericType(type)).AsSelf().As<IEventHandler>().SingleInstance();
            }

            Status = Status.Complete;
        }

        public void PostRegistration(ILifetimeScope newScope)
        {
            _eventHandlers = newScope.Resolve<IEnumerable<IEventHandler>>().ToList();
        }
    }
}
