using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling.Specs.Drivers
{
    public class ApplicationContext
    {
        public ApplicationBuilder ApplicationBuilder { get; private set; }
        public Application Application { get; private set; }

        public Task RunningApplication { get; private set; }

        public ILifetimeScope Scope { get; private set; }

        public IDictionary<string, object> Instances { get; private set; }

        public ApplicationContext()
        {
            ApplicationBuilder = new ApplicationBuilder();
            Instances = new Dictionary<string, object>();
        }

        public void WithModule<T>() where T : IModule, new()
        {
            ApplicationBuilder.WithModule<T>();
        }

        public void WithType<T>() where T : new()
        {
            ApplicationBuilder.WithType<T>();
        }

        public void WithHandler<T>()
        {
            ApplicationBuilder.WithHandler<T>();
        }

        public void Build()
        {
            Application = ApplicationBuilder.Build();
        }

        public ILifetimeScope StartScope()
        {
            Scope = Application.BuildRuntimeScope();

            return Scope;
        }

        public void Start()
        {
            Application.Startup();
            Scope = Application.RuntimeScope;
        }

        public object ResolveInstance(string name, Type type)
        {
            var instance = Scope.Resolve(type);
            Instances.Add(name, instance);

            return instance;
        }
    }
}
