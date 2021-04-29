using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Testing.Specs.Drivers;

namespace RaaLabs.Edge.Testing.Specs.Hooks
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
            _typeMapping.Add("DummyHandler", typeof(DummyHandler));
            _typeMapping.Add("IncomingDummyEvent", typeof(IncomingDummyEvent));
            _typeMapping.Add("OutgoingDummyEvent", typeof(OutgoingDummyEvent));
            _typeMapping.Add("OtherOutgoingDummyEvent", typeof(OtherOutgoingDummyEvent));
        }
    }
}
