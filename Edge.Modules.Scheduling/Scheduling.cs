using Autofac;

namespace RaaLabs.Edge.Modules.Scheduling
{
    /// <summary>
    /// 
    /// </summary>
    public class Scheduling : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SchedulingTask>().AsSelf().AsImplementedInterfaces().InstancePerMatchingLifetimeScope("runtime");
        }
    }
}
