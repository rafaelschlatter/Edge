using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Modules.IotHub.Specs.Drivers;
using RaaLabs.Edge.Modules.IotHub.Client;

namespace RaaLabs.Edge.Modules.IotHub.Specs.Hooks
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
            _typeMapping.Add("IotHub", typeof(IotHub));
            _typeMapping.Add("IotHubConnection", typeof(IotHubConnection));
            _typeMapping.Add("IIotHubClient", typeof(IIotHubClient<>));
            _typeMapping.Add("SomeIotHubIncomingEvent", typeof(SomeIotHubIncomingEvent));
            _typeMapping.Add("SomeIotHubOutgoingEvent", typeof(SomeIotHubOutgoingEvent));
        }
    }
}
