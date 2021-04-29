using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using RaaLabs.Edge.Testing;

namespace RaaLabs.Edge.Testing.Specs.Drivers
{
    class IncomingDummyEventInstanceFactory : IEventInstanceFactory<IncomingDummyEvent>
    {
        IncomingDummyEvent IEventInstanceFactory<IncomingDummyEvent>.FromTableRow(TableRow row)
        {
            return row.CreateInstance<IncomingDummyEvent>();
        }
    }
}
