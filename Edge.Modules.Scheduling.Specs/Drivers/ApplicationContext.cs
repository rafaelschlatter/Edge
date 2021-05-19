using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Scheduling.Specs.Drivers
{
    public class ApplicationContext
    {
        public ApplicationBuilder ApplicationBuilder { get; private set; }
        public Application Application { get; private set; }

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

        public void WithType<T>() where T : IModule, new()
        {
            ApplicationBuilder.WithModule<T>();
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
            var container = Application.Container;
            Scope = container.BeginLifetimeScope();

            return Scope;
        }

        public object ResolveInstance(string name, Type type)
        {
            var instance = Scope.Resolve(type);
            Instances.Add(name, instance);

            return instance;
        }
    }
}
