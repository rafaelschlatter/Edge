using TechTalk.SpecFlow;
using Serilog;
using BoDi;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A helper class for adding a logger to the container context
    /// </summary>
    [Binding]
    public sealed class LoggerProvider
    {
        private readonly IObjectContainer _container;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        public LoggerProvider(IObjectContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// 
        /// </summary>
        [BeforeScenario]
        public void SetupLogger()
        {
            var logger = new LoggerConfiguration().CreateLogger();
            _container.RegisterInstanceAs<ILogger>(logger);
        }
    }
}
