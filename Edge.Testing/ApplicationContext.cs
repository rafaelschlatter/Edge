using Autofac;
using Autofac.Core;
using BoDi;
using Moq;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationContext
    {
        /// <summary>
        /// 
        /// </summary>
        public TestApplicationBuilder ApplicationBuilder { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Application Application { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ILifetimeScope Scope { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> Instances { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationContext()
        {
            ApplicationBuilder = new TestApplicationBuilder();
            Instances = new Dictionary<string, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="typeMapping"></param>
        public static ApplicationContext FromTable(Table table, TypeMapping typeMapping)
        {
            var appContext = new ApplicationContext();

            foreach (var row in table.Rows)
            {
                var kindName = row["Kind"];
                var registrationTypeName = row["Type"];
                var registrationType = typeMapping[registrationTypeName];
                var withRegistrationMethod = typeof(ApplicationContext).GetMethod($"With{kindName}").MakeGenericMethod(registrationType);
                withRegistrationMethod.Invoke(appContext, Array.Empty<object>());
            }

            appContext.Build();

            return appContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithModule<T>() where T : IModule, new()
        {
            ApplicationBuilder.WithModule<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithTestModule<T>() where T : IModule, new()
        {
            ApplicationBuilder.WithTestModule<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithEventType<T>() where T : IEvent
        {
            ApplicationBuilder.WithEventType<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithType<T>() where T : new()
        {
            ApplicationBuilder.WithType<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithSingletonType<T>() where T : new()
        {
            ApplicationBuilder.WithSingletonType<T, T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithHandler<T>()
        {
            ApplicationBuilder.WithHandler<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public void WithInstance<T>(T instance)
            where T : class
        {
            ApplicationBuilder.WithManualRegistration(builder => builder.RegisterInstance(instance).AsSelf().AsImplementedInterfaces());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void WithMock<T>(Mock<T> mocked) where T : class
        {
            ApplicationBuilder.WithMock<T>(mocked);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Build()
        {
            Application = ApplicationBuilder.Build();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope StartScope()
        {
            var container = Application.Container;
            Scope = container.BeginLifetimeScope();

            return Scope;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            Application.Startup();
            Scope = Application.RuntimeScope;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ResolveInstance(string name, Type type)
        {
            var instance = Scope.Resolve(type);
            Instances.Add(name, instance);

            return instance;
        }
    }
}
