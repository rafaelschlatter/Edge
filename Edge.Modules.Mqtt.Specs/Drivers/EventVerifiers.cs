using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;
using FluentAssertions.Json;
using System.Text.RegularExpressions;
using MQTTnet;
using Newtonsoft.Json.Linq;

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Drivers
{

    class SomeMqttIncomingEventVerifier : IProducedEventVerifier<SomeMqttIncomingEvent>
    {
        public void VerifyFromTableRow(SomeMqttIncomingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
        }
    }

    class AnotherMqttIncomingEventVerifier : IProducedEventVerifier<AnotherMqttIncomingEvent>
    {
        public void VerifyFromTableRow(AnotherMqttIncomingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
            @event.Sensor.Should().Be(row["Sensor"]);
        }
    }


    class SomeMqttOutgoingEventVerifier : IProducedEventVerifier<SomeMqttOutgoingEvent>
    {
        public void VerifyFromTableRow(SomeMqttOutgoingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
        }
    }

    class AnotherMqttOutgoingEventVerifier : IProducedEventVerifier<AnotherMqttOutgoingEvent>
    {
        public void VerifyFromTableRow(AnotherMqttOutgoingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
            @event.Sensor.Should().Be(row["Sensor"]);
        }
    }

    class MqttApplicationMessageVerifier : IProducedEventVerifier<MqttApplicationMessage>
    {
        public void VerifyFromTableRow(MqttApplicationMessage data, TableRow row)
        {
            var actualPayload = JObject.Parse(System.Text.Encoding.UTF8.GetString(data.Payload));
            var expectedPayload = JObject.Parse(row["Payload"]);
            actualPayload.Should().BeEquivalentTo(expectedPayload);

            data.Topic.Should().Be(row["Topic"]);            
        }
    }

}
