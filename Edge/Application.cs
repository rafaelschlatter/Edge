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

            var bootloaders = buildScope.Resolve<IEnumerable<IBootloader>>().ToHashSet();
            var runtimeScope = BuildRuntimeScope(buildScope, bootloaders);

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

        /// <summary>
        /// Recursively build new runtime scopes while running all available bootloaders for each scope.
        /// Once all bootloaders have completed, tag the current scope as "runtime" and return it.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="bootloaders"></param>
        /// <returns>The runtime scope of the application</returns>
        private ILifetimeScope BuildRuntimeScope(ILifetimeScope scope, ISet<IBootloader> bootloaders)
        {
            var logger = scope.Resolve<Serilog.ILogger>();

            var readyBootloaders = bootloaders.Where(b => b.Status == Status.Ready).ToList();
            var preRegistrationStageBootloaders = readyBootloaders.Where(b => b.GetType().IsAssignableTo<IPreRegistrationStage>()).Select(b => (IPreRegistrationStage) b).ToList();
            var registrationStageBootloaders = readyBootloaders.Where(b => b.GetType().IsAssignableTo<IRegistrationStage>()).Select(b => (IRegistrationStage)b).ToList();
            var postRegistrationStageBootloaders = readyBootloaders.Where(b => b.GetType().IsAssignableTo<IPostRegistrationStage>()).Select(b => (IPostRegistrationStage)b).ToList();

            foreach (var preRegistrationBootloader in preRegistrationStageBootloaders)
            {
                preRegistrationBootloader.PreRegistration(scope);
            }

            var newScope = scope.BeginLifetimeScope(builder =>
            {
                foreach (var bootloader in registrationStageBootloaders)
                {
                    bootloader.RegistrationStage(builder);
                }
            });

            foreach (var bootloader in postRegistrationStageBootloaders)
            {
                bootloader.PostRegistration(newScope);
            }

            foreach (var bootloader in readyBootloaders)
            {
                bootloaders.Remove(bootloader);
            }

            if (readyBootloaders.Count == 0)
            {
                throw new Exception("Some bootloaders never ran.");
            }

            if (bootloaders.Count > 0)
            {
                return BuildRuntimeScope(newScope, bootloaders);
            }
            else
            {
                return newScope.BeginLifetimeScope("runtime");
            }
        }
    }
}
