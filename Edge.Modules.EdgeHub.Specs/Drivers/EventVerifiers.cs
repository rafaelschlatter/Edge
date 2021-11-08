using TechTalk.SpecFlow;
using FluentAssertions;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Linq;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers
{

    class SomeEdgeHubIncomingEventVerifier : IProducedEventVerifier<SomeEdgeHubIncomingEvent>
    {
        public void VerifyFromTableRow(SomeEdgeHubIncomingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(int.Parse(row["Value"]));
        }
    }

    class AnotherEdgeHubIncomingEventVerifier : IProducedEventVerifier<AnotherEdgeHubIncomingEvent>
    {
        public void VerifyFromTableRow(AnotherEdgeHubIncomingEvent @event, TableRow row)
        {
            @event.Value.Should().Be(row["Value"]);
        }
    }

    class MessageVerifier : IProducedEventVerifier<(string outputName, Message message)>
    {
        public void VerifyFromTableRow((string outputName, Message message) data, TableRow row)
        {
            var actualPayload = JObject.Parse(System.Text.Encoding.UTF8.GetString(data.message.GetBytes()));
            var expectedPayload = JObject.Parse(row["Payload"]);
            var expectedOutputName = row["OutputName"];

            actualPayload.Should().BeEquivalentTo(expectedPayload);
            data.outputName.Should().Be(expectedOutputName);
        }
    }
}
