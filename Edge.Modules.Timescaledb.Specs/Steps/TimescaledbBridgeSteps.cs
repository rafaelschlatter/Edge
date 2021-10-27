using TechTalk.SpecFlow;
using BoDi;
using RaaLabs.Edge.Modules.Timescaledb.Specs.Drivers;

namespace RaaLabs.Edge.Modules.Timescaledb.Specs.Steps
{
    [Binding]
    class TimescaledbBridgeSteps
    {
        private readonly IObjectContainer _container;

        public TimescaledbBridgeSteps(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeTimescaledbEventInstanceFactory, IEventInstanceFactory<SomeTimescaledbEvent>>();
            _container.RegisterTypeAs<AnotherTimescaledbEventInstanceFactory, IEventInstanceFactory<AnotherTimescaledbEvent>>();
            _container.RegisterTypeAs<ThirdTimescaledbEventInstanceFactory, IEventInstanceFactory<ThirdTimescaledbEvent>>();

            _container.RegisterTypeAs<ObjectEventVerifier, IProducedEventVerifier<object>>();
        }
    }
}
