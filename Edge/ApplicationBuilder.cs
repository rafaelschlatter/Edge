using System;
using Autofac;
using System.Collections.Generic;

namespace RaaLabs.Edge.Modules
{
    public class ApplicationBuilder
    {
        private readonly ContainerBuilder _builder;
        private readonly List<Type> _handlers;

        public ApplicationBuilder()
        {
            _builder = new ContainerBuilder();
            _handlers = new List<Type>();
        }

        public ApplicationBuilder WithModule<TModule>() where TModule : Autofac.Core.IModule, new()
        {
            _builder.RegisterModule<TModule>();
            return this;
        }

        public ApplicationBuilder WithHandler<THandler>()
        {
            _builder.RegisterType<THandler>();
            _handlers.Add(typeof(THandler));
            return this;
        }

        public ApplicationBuilder WithType<T>()
        {
            _builder.RegisterType<T>();
            return this;
        }

        public Application Build()
        {
            IContainer container = _builder.Build();
            return new Application(container, _handlers);
        }
    }
}
