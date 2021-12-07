using Autofac;
using Moq;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// An application builder for testing purposes.
    /// </summary>
    public class TestApplicationBuilder : ApplicationBuilder
    {
        private readonly Dictionary<Type, Mock> _mocks = new();

        /// <summary>
        /// Register an Autofac module for the application.
        /// </summary>
        /// <typeparam name="TModule">The module to register</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithTestModule<TModule>() where TModule : Autofac.Core.IModule, new()
        {
            _builder.RegisterModule<TModule>();
            return this;
        }

        /// <summary>
        /// Register an event type for the application.
        /// </summary>
        /// <typeparam name="TEvent">The event type to register</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithEventType<TEvent>() where TEvent : IEvent
        {
            _builder.Register(context => typeof(TEvent)).Named<Type>("EventType").SingleInstance();

            return this;
        }

        /// <summary>
        /// Register a mocked type for the application.
        /// </summary>
        /// <typeparam name="MockedType">The type to mock</typeparam>
        /// <returns></returns>
        public ApplicationBuilder WithMock<MockedType>(Mock<MockedType> mocked) where MockedType : class
        {
            _mocks.Add(typeof(MockedType), mocked);
            return this;
        }

        /// <summary>
        /// Build the application.
        /// </summary>
        /// <returns>The application containing all tasks and handlers to start up</returns>
        public override Application Build()
        {
            _builder.RegisterServiceMiddlewareSource(new TestingMiddlewareSource(_mocks));
            return base.Build();
        }

    }
}
