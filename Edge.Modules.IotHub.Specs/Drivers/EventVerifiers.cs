using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Linq;

namespace RaaLabs.Edge.Modules.IotHub.Specs.Drivers
{

    class SomeIotHubIncomingEventVerifier : IProducedEventVerifier<SomeIotHubIncomingEvent>
    {
        public void VerifyFromTableRow(SomeIotHubIncomingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
        }
    }

    class SomeIotHubOutgoingEventVerifier : IProducedEventVerifier<SomeIotHubOutgoingEvent>
    {
        public void VerifyFromTableRow(SomeIotHubOutgoingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
        }
    }

    class MessageVerifier : IProducedEventVerifier<Message>
    {
        public void VerifyFromTableRow(Message data, TableRow row)
        {
            var actualPayload = JObject.Parse(System.Text.Encoding.UTF8.GetString(data.GetBytes()));
            var expectedPayload = JObject.Parse(row["Payload"]);
            actualPayload.Should().BeEquivalentTo(expectedPayload);
        }
    }
}
