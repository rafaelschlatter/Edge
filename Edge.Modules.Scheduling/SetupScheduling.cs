using Autofac;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// This class will add support for Scheduling to the application.
    /// </summary>
    class SetupScheduling : IBootloader, IRegistrationStage
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

        public SetupScheduling(SetupEventHandlers setupEventHandlers)
        {
            _setupEventHandlers = setupEventHandlers;
        }

        public void RegistrationStage(ContainerBuilder builder)
        {
            builder.RegisterType<SchedulingTask>().AsSelf().As<IRunAsync>().InstancePerMatchingLifetimeScope("runtime");
            _complete = true;
        }
    }
}
