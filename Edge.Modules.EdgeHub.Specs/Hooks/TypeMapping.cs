using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Hooks
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
            _typeMapping.Add("EdgeHub", typeof(EdgeHub));
            _typeMapping.Add("IIotModuleClient", typeof(IIotModuleClient));
            _typeMapping.Add("SomeEdgeHubIncomingEvent", typeof(SomeEdgeHubIncomingEvent));
            _typeMapping.Add("AnotherEdgeHubIncomingEvent", typeof(AnotherEdgeHubIncomingEvent));
            _typeMapping.Add("SomeEdgeHubOutgoingEvent", typeof(SomeEdgeHubOutgoingEvent));
            _typeMapping.Add("AnotherEdgeHubOutgoingEvent", typeof(AnotherEdgeHubOutgoingEvent));
        }
    }
}
