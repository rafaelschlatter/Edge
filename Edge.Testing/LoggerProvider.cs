using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TechTalk.SpecFlow;
using Serilog;
using BoDi;

namespace RaaLabs.Edge.Testing
{
    [Binding]
    public sealed class LoggerProvider
    {
        private readonly IObjectContainer _container;
        private readonly ComponentAssemblies _assemblies;

        public LoggerProvider(IObjectContainer container, ComponentAssemblies assemblies)
        {
            _container = container;
            _assemblies = assemblies;
        }

        [BeforeScenario]
        public void SetupLogger()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            _container.RegisterInstanceAs<ILogger>(logger);
        }
    }
}
