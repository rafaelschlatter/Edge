using System;
using Autofac;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge
{
    /// <summary>
    /// The class responsible for running all the application handlers and tasks.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// The underlying autofac container for the application
        /// </summary>
        public IContainer Container { get; }
        private readonly List<Type> _handlers;
        private readonly List<Type> _tasks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="handlers"></param>
        /// <param name="tasks"></param>
        public Application(IContainer container, List<Type> handlers, List<Type> tasks)
        {
            Container = container;
            _handlers = handlers;
            _tasks = tasks;
        }

        /// <summary>
        /// Start the application. This will instantiate all handlers and tasks.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            using var scope = Container.BeginLifetimeScope();
            var logger = scope.Resolve<Serilog.ILogger>();

            logger.Information("Starting up handlers...");
            // Instantiate all handlers
            var handlers = _handlers.Select(handlerType => scope.Resolve(handlerType)).ToList();
            logger.Information("Handlers started.");
            logger.Information("Starting up tasks...");
            var tasks = _tasks
                .Select(taskType => (IRunAsync) scope.Resolve(taskType))
                .Select(async task => await task.Run())
                .ToList();

            logger.Information("Tasks started.");

            while (true)
            {
                await Task.Delay(1000);
            }
        }

    }
}
