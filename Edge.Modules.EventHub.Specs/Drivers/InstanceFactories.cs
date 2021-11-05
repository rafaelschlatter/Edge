using System.Text;
using Azure.Messaging.EventHubs;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.Modules.EventHub.Specs.Drivers
{
    class SomeEventHubIncomingEventInstanceFactory : IEventInstanceFactory<SomeEventHubIncomingEvent>
    {
        public SomeEventHubIncomingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeEventHubIncomingEvent
            {
                Value = value
            };
        }
    }

    class SomeEventHubOutgoingEventInstanceFactory : IEventInstanceFactory<SomeEventHubOutgoingEvent>
    {
        public SomeEventHubOutgoingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeEventHubOutgoingEvent
            {
                Value = value
            };
        }
    }

    class EventDataInstanceFactory : IEventInstanceFactory<EventData>
    {
        public EventData FromTableRow(TableRow row)
        {
            return new EventData(Encoding.UTF8.GetBytes(row["Payload"]));
        }
    }
}
