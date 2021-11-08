using TechTalk.SpecFlow;
using FluentAssertions;
using FluentAssertions.Json;
using Azure.Messaging.EventHubs;
using Newtonsoft.Json.Linq;

namespace RaaLabs.Edge.Modules.EventHub.Specs.Drivers
{

    class SomeEventHubIncomingEventVerifier : IProducedEventVerifier<SomeEventHubIncomingEvent>
    {
        public void VerifyFromTableRow(SomeEventHubIncomingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
        }
    }

    class SomeEventHubOutgoingEventVerifier : IProducedEventVerifier<SomeEventHubOutgoingEvent>
    {
        public void VerifyFromTableRow(SomeEventHubOutgoingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Payload"]));
        }
    }

    class EventDataVerifier : IProducedEventVerifier<EventData>
    {
        public void VerifyFromTableRow(EventData data, TableRow row)
        {
            var actualPayload = JObject.Parse(System.Text.Encoding.UTF8.GetString(data.EventBody.ToArray()));
            var expectedPayload = JObject.Parse(row["Payload"]);
            actualPayload.Should().BeEquivalentTo(expectedPayload);
        }
    }

}
