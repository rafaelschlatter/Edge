using TechTalk.SpecFlow;
using BoDi;
using RaaLabs.Edge.Testing.Specs.Drivers;

namespace RaaLabs.Edge.Testing.Specs.Steps
{
    [Binding]
    class EdgeTestingSteps
    {
        private readonly IObjectContainer _container;

        public EdgeTestingSteps(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<IncomingDummyEventInstanceFactory, IEventInstanceFactory<IncomingDummyEvent>>();

            _container.RegisterTypeAs<OutgoingDummyEventVerifier, IProducedEventVerifier<OutgoingDummyEvent>>();
            _container.RegisterTypeAs<OtherOutgoingDummyEventVerifier, IAllProducedEventsVerifier<OtherOutgoingDummyEvent>>();
        }
    }
}
