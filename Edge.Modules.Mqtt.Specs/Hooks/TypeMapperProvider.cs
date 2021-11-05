using TechTalk.SpecFlow;
using RaaLabs.Edge.Testing;
using RaaLabs.Edge.Modules.Mqtt.Specs.Drivers;
using RaaLabs.Edge.Modules.Mqtt.Client;

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Hooks
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
            _typeMapping.Add("Mqtt", typeof(Mqtt));
            _typeMapping.Add("IMqttIncomingEvent", typeof(IMqttIncomingEvent));
            _typeMapping.Add("IMqttOutgoingEvent", typeof(IMqttOutgoingEvent));
            _typeMapping.Add("IMqttBrokerClient", typeof(IMqttBrokerClient<>));
            _typeMapping.Add("SomeMqttIncomingEvent", typeof(SomeMqttIncomingEvent));
            _typeMapping.Add("AnotherMqttIncomingEvent", typeof(AnotherMqttIncomingEvent));
            _typeMapping.Add("SomeMqttOutgoingEvent", typeof(SomeMqttOutgoingEvent));
            _typeMapping.Add("AnotherMqttOutgoingEvent", typeof(AnotherMqttOutgoingEvent));
            _typeMapping.Add("MqttBrokerConnection", typeof(MqttBrokerConnection));
        }
    }
}
