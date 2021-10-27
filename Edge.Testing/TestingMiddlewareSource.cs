using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Moq;
using System;
using System.Collections.Generic;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A middleware source returning a mocked interface if the mock has been registered.
    /// </summary>
    public class TestingMiddlewareSource : IServiceMiddlewareSource
    {
        private readonly Dictionary<Type, Mock> _mocks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mocks"></param>
        public TestingMiddlewareSource(Dictionary<Type, Mock> mocks)
        {
            _mocks = mocks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="availableServices"></param>
        /// <param name="pipelineBuilder"></param>
        public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
        {
            pipelineBuilder.Use(PipelinePhase.ResolveRequestStart, (context, next) =>
            {
                if (context.Service is TypedService type)
                {
                    InterceptTypedService(type, context, next);
                    
                    return;
                }

                next(context);
            });
        }

        private void InterceptTypedService(TypedService type, ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            if (_mocks.TryGetValue(type.ServiceType, out Mock mock))
            {
                context.Instance = mock.Object;

                return;
            }

            next(context);
        }
    }
}
