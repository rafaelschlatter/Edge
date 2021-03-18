﻿using System;
using Autofac;
using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Autofac.Features.ResolveAnything;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Builder class for the application.
    /// </summary>
    public class ApplicationBuilder
    {
        private readonly ContainerBuilder _builder;
        private readonly List<Type> _handlers;
        private readonly List<Type> _tasks;

        /// <summary>
        /// 
        /// </summary>
        public ApplicationBuilder()
        {
            _builder = new ContainerBuilder();
            _handlers = new List<Type>();
            _tasks = new List<Type>();

            _builder.Register(_ => CreateLogger()).As<ILogger>();
            _builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
        }

        /// <summary>
        /// Register an Autofac module for the application.
        /// </summary>
        /// <typeparam name="TModule">The module to register</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithModule<TModule>() where TModule : Autofac.Core.IModule, new()
        {
            _builder.RegisterModule<TModule>();
            return this;
        }

        /// <summary>
        /// Register a handler for the application. The application will make sure that the handler
        /// is instantiated when the application's Run() function is called
        /// </summary>
        /// <typeparam name="THandler">The handler to register</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithHandler<THandler>()
        {
            _builder.RegisterType<THandler>();
            _handlers.Add(typeof(THandler));
            return this;
        }

        /// <summary>
        /// Register a single type for the application. This should not be required to do anymore, because
        /// the builder already register the AnyConcreteTypeNotAlreadyRegisteredSource source.
        /// </summary>
        /// <typeparam name="T">The type to register</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithType<T>()
        {
            _builder.RegisterType<T>();
            return this;
        }

        /// <summary>
        /// Register a task for the application. The application will make sure that the task is executed
        /// when the application's Run() function is called.
        /// </summary>
        /// <typeparam name="Task">The </typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithTask<Task>() where Task : IRunAsync
        {
            _tasks.Add(typeof(Task));
            _builder.RegisterType<Task>();
            return this;
        }

        /// <summary>
        /// An escape hatch function to access the Autofac container builder directly through a lambda function.
        /// Shouldn't be required to use too often, but provided here just in case.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        public ApplicationBuilder WithManualRegistration(Action<ContainerBuilder> registration)
        {
            registration(_builder);
            return this;
        }

        /// <summary>
        /// Build the application.
        /// </summary>
        /// <returns>The application containing all tasks and handlers to start up</returns>
        public Application Build()
        {
            IContainer container = _builder.Build();
            return new Application(container, _handlers, _tasks);
        }

        /// <summary>
        /// Create a Serilog logger
        /// </summary>
        /// <returns>A Serilog logger</returns>
        private Serilog.Core.Logger CreateLogger()
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            return log;
        }

    }
}
