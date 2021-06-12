using System;
using Autofac;
using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Autofac.Features.ResolveAnything;
using System.Reflection;

namespace RaaLabs.Edge
{
    /// <summary>
    /// Builder class for the application.
    /// </summary>
    public class ApplicationBuilder
    {
        private readonly ContainerBuilder _builder;
        private readonly List<Type> _handlers;
        private readonly ISet<Assembly> _assemblies;

        /// <summary>
        /// 
        /// </summary>
        public ApplicationBuilder()
        {
            _builder = new ContainerBuilder();
            _handlers = new List<Type>();
            _assemblies = new HashSet<Assembly>();

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
            _assemblies.Add(typeof(TModule).Assembly);
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
            _assemblies.Add(typeof(THandler).Assembly);
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
            _assemblies.Add(typeof(T).Assembly);
            _builder.RegisterType<T>().AsImplementedInterfaces().AsSelf();
            return this;
        }

        /// <summary>
        /// Register a singleton class for the runtime.
        /// </summary>
        /// <typeparam name="T">The class to register</typeparam>
        /// <typeparam name="I">The interface to register as</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithSingletonType<T, I>()
            where T : I
        {
            _assemblies.Add(typeof(T).Assembly);
            _builder.RegisterType<T>().AsSelf().As<I>().InstancePerMatchingLifetimeScope("runtime");
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
            _assemblies.Add(typeof(Task).Assembly);
            _builder.RegisterType<Task>().AsImplementedInterfaces().AsSelf().InstancePerMatchingLifetimeScope("runtime");
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
        /// 
        /// </summary>
        /// <typeparam name="I">the interface to search for implementations of. 
        /// Note that the interaface and the implementation should exist within the same assembly. </typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithAllImplementationsOf<I>() where I : class
        {
            _assemblies.Add(typeof(I).Assembly);
            var dataAccess = typeof(I).Assembly;
            _builder.RegisterAssemblyTypes(dataAccess)
                .Where(type => type.IsAssignableTo<I>())
                .AsSelf()
                .As<I>();

            return this;
        }

        /// <summary>
        /// Manually register an assembly used by the application.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public ApplicationBuilder WithAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        /// <summary>
        /// Build the application.
        /// </summary>
        /// <returns>The application containing all tasks and handlers to start up</returns>
        public Application Build()
        {
            foreach (var assembly in _assemblies)
            {
                _builder.RegisterInstance(assembly).As<Assembly>();
            }

            IContainer container = _builder.Build();
            return new Application(container, _handlers);
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
