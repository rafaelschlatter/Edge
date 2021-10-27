using TechTalk.SpecFlow;
using BoDi;
using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.IotHub.Specs.Drivers;

namespace RaaLabs.Edge.Modules.IotHub.Specs.Steps
{
    [Binding]
    class IotHubBridgeSteps
    {
        private readonly IObjectContainer _container;

        public IotHubBridgeSteps(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeScenario]
        public void SetupEventFactories()
        {
            _container.RegisterTypeAs<SomeIotHubOutgoingEventInstanceFactory, IEventInstanceFactory<SomeIotHubOutgoingEvent>>();
            _container.RegisterTypeAs<MessageInstanceFactory, IEventInstanceFactory<Message>>();

            _container.RegisterTypeAs<MessageVerifier, IProducedEventVerifier<Message>>();
            _container.RegisterTypeAs<SomeIotHubIncomingEventVerifier, IProducedEventVerifier<SomeIotHubIncomingEvent>>();
        }
    }
}
