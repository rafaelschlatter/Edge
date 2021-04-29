using TechTalk.SpecFlow;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class OtherOutgoingDummyEventVerifier : IAllProducedEventsVerifier<OtherOutgoingDummyEvent>
    {
        public void VerifyFromTable(IList<OtherOutgoingDummyEvent> events, Table table)
        {
            foreach(var (actual, expected) in events.Zip(table.Rows))
            {
                actual.Payload.Should().Be(expected["Payload"]);
            }
        }
    }
}
