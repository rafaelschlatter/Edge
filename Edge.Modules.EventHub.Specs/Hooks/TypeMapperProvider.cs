using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Modules.EventHub.Specs.Drivers;
using RaaLabs.Edge.Modules.EventHub.Client.Consumer;
using RaaLabs.Edge.Modules.EventHub.Client.Producer;

namespace RaaLabs.Edge.Modules.EventHub.Specs.Hooks
{
    [Binding]
    public sealed class TypeMapperProvider
    {
        private readonly TypeMapping _typeMapping;

        public TypeMapperProvider(TypeMapping typeMapping)
        {
            _typeMapping = typeMapping;
        }

        [BeforeScenario]
        public void SetupTypes()
        {
            _typeMapping.Add("EventHandling", typeof(EventHandling.EventHandling));
            _typeMapping.Add("EventHub", typeof(EventHub));
            _typeMapping.Add("IEventHubConsumerClient", typeof(IEventHubConsumerClient<>));
            _typeMapping.Add("IEventHubProducerClient", typeof(IEventHubProducerClient<>));
            _typeMapping.Add("IncomingEventHubConnection", typeof(IncomingEventHubConnection));
            _typeMapping.Add("OutgoingEventHubConnection", typeof(OutgoingEventHubConnection));
            _typeMapping.Add("SomeEventHubIncomingEvent", typeof(SomeEventHubIncomingEvent));
            _typeMapping.Add("SomeEventHubOutgoingEvent", typeof(SomeEventHubOutgoingEvent));
        }
    }
}
