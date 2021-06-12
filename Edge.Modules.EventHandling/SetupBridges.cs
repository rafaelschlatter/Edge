using Autofac;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// This class will add support for <see cref="IBridge"/> components to the application.
    /// </summary>
    class SetupBridges : IBootloader, IRegistrationStage
    {
        private readonly SetupEventHandlers _setupEventHandlers;

        private bool _complete = false;

        public Status Status
        {
            get
            {
                if (_complete) return Status.Complete;
                return (_setupEventHandlers.Status == Status.Complete) ? Status.Ready : Status.Waiting;
            }
        }

        public SetupBridges(SetupEventHandlers setupEventHandlers)
        {
            _setupEventHandlers = setupEventHandlers;
        }

        public void RegistrationStage(ContainerBuilder builder)
        {
            builder.RegisterType<BridgesTask>().AsSelf().As<IRunAsync>().InstancePerMatchingLifetimeScope("runtime");
            _complete = true;
        }
    }

    class BridgesTask : IRunAsync
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IBridge> _bridges;

        public BridgesTask(ILogger logger, IEnumerable<IBridge> bridges)
        {
            _logger = logger;
            _bridges = bridges;
        }

        public async Task Run()
        {
            var allBridgeTasks = _bridges.Select(async bridge => await bridge.SetupBridge()).ToList();
            if (allBridgeTasks.Count > 0)
            {
                await Task.WhenAll(allBridgeTasks);
            }
        }
    }
}
