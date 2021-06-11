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
    class SetupBridges : IBootloader
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _logger;
        private readonly SetupEventHandlers _setupEventHandlers;
        private readonly IEnumerable<Assembly> _assemblies;

        private bool _complete = false;

        public Status Status
        {
            get
            {
                if (_complete) return Status.Complete;
                return (_setupEventHandlers.Status == Status.Complete) ? Status.Ready : Status.Waiting;
            }
        }

        public SetupBridges(ILifetimeScope scope, ILogger logger, IEnumerable<Assembly> assemblies, SetupEventHandlers setupEventHandlers)
        {
            _scope = scope;
            _logger = logger;
            _setupEventHandlers = setupEventHandlers;
            _assemblies = assemblies;
        }

        public void RunBootloader(ContainerBuilder builder)
        {
            var allBridges = _assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Distinct()
                .Where(type => !type.IsInterface)
                .Where(type => type.IsAssignableTo<IBridge>())
                .ToList();

            allBridges.ForEach(bridge => builder.RegisterType(bridge).AsSelf().As<IBridge>().SingleInstance());

            builder.RegisterType<BridgesTask>().AsSelf().As<IRunAsync>().SingleInstance();
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
                _logger.Information("Started {Count} bridges.", allBridgeTasks.Count);
                await Task.WhenAll(allBridgeTasks);
            }
        }
    }
}
