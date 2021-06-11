using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using Serilog;

namespace RaaLabs.Edge.Modules.EventHandling
{
    class SetupEventHandlers : IBootloader
    {
        private readonly IEnumerable<Assembly> _assemblies;
        private readonly ILifetimeScope _scope;
        private readonly ILogger _logger;

        private bool _complete = false;
        public Status Status {
            get
            {
                return _complete ? Status.Complete : Status.Ready;
            }
        }

        public SetupEventHandlers(ILifetimeScope scope, IEnumerable<Assembly> assemblies, ILogger logger)
        {
            _scope = scope;
            _assemblies = assemblies;
            _logger = logger;
        }

        public void RunBootloader(ContainerBuilder builder)
        {
            var allEventTypes = _assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Distinct()
                .Where(type => type.IsAssignableTo<IEvent>());

            foreach (var type in allEventTypes)
            {
                builder.RegisterType(typeof(EventHandler<>).MakeGenericType(type)).AsSelf().SingleInstance();
            }

            _complete = true;
        }
    }
}
