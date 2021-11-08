using TechTalk.SpecFlow;
using BoDi;
using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Steps
{
    [Binding]
    class EdgeHubBridgeSteps
    {
        private readonly IObjectContainer _container;

        public EdgeHubBridgeSteps(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeEdgeHubOutgoingEventInstanceFactory, IEventInstanceFactory<SomeEdgeHubOutgoingEvent>>();
            _container.RegisterTypeAs<AnotherEdgeHubOutgoingEventInstanceFactory, IEventInstanceFactory<AnotherEdgeHubOutgoingEvent>>();
            _container.RegisterTypeAs<MessageInstanceFactory, IEventInstanceFactory<(string inputName, Message message)>>();
            _container.RegisterTypeAs<TopicInstanceFactory, IEventInstanceFactory<string>>();

            _container.RegisterTypeAs<MessageVerifier, IProducedEventVerifier<(string inputName, Message message)>>();
            _container.RegisterTypeAs<SomeEdgeHubIncomingEventVerifier, IProducedEventVerifier<SomeEdgeHubIncomingEvent>>();
            _container.RegisterTypeAs<AnotherEdgeHubIncomingEventVerifier, IProducedEventVerifier<AnotherEdgeHubIncomingEvent>>();
        }
    }
}
