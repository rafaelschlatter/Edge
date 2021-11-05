using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Modules.Timescaledb.Specs.Drivers;

namespace RaaLabs.Edge.Modules.Timescaledb.Specs.Hooks
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
            _typeMapping.Add("Timescaledb", typeof(Timescaledb));
            _typeMapping.Add("SomeTimescaledbConnection", typeof(SomeTimescaledbConnection));
            _typeMapping.Add("AnotherTimescaledbConnection", typeof(AnotherTimescaledbConnection));
            _typeMapping.Add("ITimescaledbClient", typeof(ITimescaledbClient<>));
            _typeMapping.Add("SomeTimescaledbEvent", typeof(SomeTimescaledbEvent));
            _typeMapping.Add("AnotherTimescaledbEvent", typeof(AnotherTimescaledbEvent));
            _typeMapping.Add("ThirdTimescaledbEvent", typeof(ThirdTimescaledbEvent));
        }
    }
}
