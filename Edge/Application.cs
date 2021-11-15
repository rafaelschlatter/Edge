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
        private List<Task> _runningTasks;

        /// <summary>
        /// The runtime scope for the application
        /// </summary>
        public ILifetimeScope RuntimeScope { get; private set; }

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
            Startup();
            var logger = RuntimeScope.Resolve<Serilog.ILogger>();

            logger.Information("Starting up handlers...");

            // Instantiate all handlers. Assigned to a variable to ensure they are not removed by the Garbage Collector.
            var handlers = _handlers.Select(handlerType => RuntimeScope.Resolve(handlerType)).ToList();
            logger.Information("Handlers started.");

            if (_runningTasks.Count > 0)
            {
                await Task.WhenAll(_runningTasks);
            }

            while (true)
            {
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Startup()
        {
            RuntimeScope = BuildRuntimeScope();

            var logger = RuntimeScope.Resolve<Serilog.ILogger>();

            logger.Information("Starting up tasks...");
            var tasks = RuntimeScope.Resolve<IEnumerable<IRunAsync>>();
            _runningTasks = tasks
                .Select(async task => await task.Run())
                .ToList();

            logger.Information("Tasks started.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope BuildRuntimeScope()
        {
            var bootloaders = Container.Resolve<IEnumerable<IBootloader>>().ToHashSet();
            
            return BuildRuntimeScope(Container.BeginLifetimeScope(), bootloaders);
        }

        /// <summary>
        /// Recursively build new runtime scopes while running all available bootloaders for each scope.
        /// Once all bootloaders have completed, tag the current scope as "runtime" and return it.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="bootloaders"></param>
        /// <returns>The runtime scope of the application</returns>
        private static ILifetimeScope BuildRuntimeScope(ILifetimeScope scope, ISet<IBootloader> bootloaders)
        {
            var readyBootloaders = bootloaders.Where(b => b.Status == Status.Ready).ToList();
            var waitingBootloaders = bootloaders.Where(b => b.Status == Status.Waiting).ToList();
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

            if (readyBootloaders.Count == 0 && waitingBootloaders.Count > 0)
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
