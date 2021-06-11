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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="handlers"></param>
        public Application(IContainer container, List<Type> handlers)
        {
            Container = container;
            _handlers = handlers;
        }

        /// <summary>
        /// Start the application. This will instantiate all handlers and tasks.
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            using var buildScope = Container.BeginLifetimeScope();
            var logger = buildScope.Resolve<Serilog.ILogger>();

            var bootloaders = buildScope.Resolve<IEnumerable<IBootloader>>();

            var runtimeScope = await BuildRuntimeScope(buildScope, bootloaders.ToHashSet());

            logger.Information("Starting up handlers...");

            // Instantiate all handlers. Assigned to a variable to ensure they are not removed by the Garbage Collector.
            var handlers = _handlers.Select(handlerType => runtimeScope.Resolve(handlerType)).ToList();
            logger.Information("Handlers started.");
            logger.Information("Starting up tasks...");
            var tasks = runtimeScope.Resolve<IEnumerable<IRunAsync>>();
            var runningTasks = tasks
                .Select(async task => await task.Run())
                .ToList();

            logger.Information("Tasks started.");

            await Task.WhenAll(runningTasks);

            while (true)
            {
                await Task.Delay(1000);
            }
        }

        private async Task<ILifetimeScope> BuildRuntimeScope(ILifetimeScope scope, ISet<IBootloader> bootloaders)
        {
            var logger = scope.Resolve<Serilog.ILogger>();

            var scopeHasPerformedBootloaders = false;
            var newScope = scope.BeginLifetimeScope(builder =>
            {
                var readyBootloaders = bootloaders
                    .Where(bootloader => bootloader.Status == Status.Ready)
                    .ToArray();

                foreach (var bootloader in readyBootloaders)
                {
                    bootloader.RunBootloader(builder);
                    bootloaders.Remove(bootloader);
                    scopeHasPerformedBootloaders = true;
                }
            });

            if (!scopeHasPerformedBootloaders)
            {
                throw new Exception("Some bootloaders never ran.");
            }

            if (bootloaders.Count > 0)
            {
                return await BuildRuntimeScope(newScope, bootloaders);
            }
            else
            {
                return await Task.FromResult(newScope);
            }
        }
    }
}
