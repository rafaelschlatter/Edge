using System;
using Autofac;
using System.Collections.Generic;
using Serilog;

namespace RaaLabs.Edge
{
    public class ApplicationBuilder
    {
        private readonly ContainerBuilder _builder;
        private readonly List<Type> _handlers;
        private readonly List<Type> _tasks;

        public ApplicationBuilder()
        {
            _builder = new ContainerBuilder();
            _handlers = new List<Type>();
            _tasks = new List<Type>();

            _builder.Register(_ => CreateLogger()).As<ILogger>();
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

        public ApplicationBuilder WithTask<T>() where T : IRunAsync
        {
            _tasks.Add(typeof(T));
            _builder.RegisterType<T>();
            return this;
        }

        public Application Build()
        {
            IContainer container = _builder.Build();
            return new Application(container, _handlers, _tasks);
        }

        private Serilog.Core.Logger CreateLogger()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            return log;
        }

    }
}
