using TechTalk.SpecFlow;
using BoDi;
using RaaLabs.Edge.Modules.EventHub.Specs.Drivers;
using Azure.Messaging.EventHubs;

namespace RaaLabs.Edge.Modules.EventHub.Specs.Steps
{
    [Binding]
    class EventHubBridgeSteps
    {
        private readonly IObjectContainer _container;

        public EventHubBridgeSteps(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeEventHubIncomingEventInstanceFactory, IEventInstanceFactory<SomeEventHubIncomingEvent>>();
            _container.RegisterTypeAs<SomeEventHubOutgoingEventInstanceFactory, IEventInstanceFactory<SomeEventHubOutgoingEvent>>();
            _container.RegisterTypeAs<EventDataInstanceFactory, IEventInstanceFactory<EventData>>();

            _container.RegisterTypeAs<SomeEventHubIncomingEventVerifier, IProducedEventVerifier<SomeEventHubIncomingEvent>>();
            _container.RegisterTypeAs<EventDataVerifier, IProducedEventVerifier<EventData>>();
        }
    }
}
