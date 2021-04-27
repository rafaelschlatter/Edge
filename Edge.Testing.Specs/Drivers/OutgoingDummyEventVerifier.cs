using TechTalk.SpecFlow;
using FluentAssertions;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class OutgoingDummyEventVerifier : IProducedEventVerifier<OutgoingDummyEvent>
    {
        public void VerifyFromTableRow(OutgoingDummyEvent @event, TableRow row)
        {
            @event.Payload.Should().Be(row["Payload"]);
        }
    }
}
