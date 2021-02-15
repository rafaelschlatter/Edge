using System;
using Autofac;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules
{
    public class Application
    {
        private readonly IContainer _container;
        private readonly List<Type> _handlers;

        public Application(IContainer container, List<Type> handlers)
        {
            _container = container;
            _handlers = handlers;
        }

        public async Task Run()
        {
            using var scope = _container.BeginLifetimeScope();
            var logger = scope.Resolve<Serilog.ILogger>();

            logger.Information("Starting up handlers...");
            // Instantiate all handlers
            var handlers = _handlers.Select(handlerType => scope.Resolve(handlerType)).ToList();
            logger.Information("Handlers started.");

            while (true)
            {
                await Task.Delay(1000);
            }
        }

    }
}
