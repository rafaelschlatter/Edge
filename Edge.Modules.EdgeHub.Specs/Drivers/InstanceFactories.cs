using System.Text;
using Microsoft.Azure.Devices.Client;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.Modules.EdgeHub.Specs.Drivers
{
    class SomeEdgeHubOutgoingEventInstanceFactory : IEventInstanceFactory<SomeEdgeHubOutgoingEvent>
    {
        public SomeEdgeHubOutgoingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeEdgeHubOutgoingEvent
            {
                Value = value
            };
        }
    }

    class AnotherEdgeHubOutgoingEventInstanceFactory : IEventInstanceFactory<AnotherEdgeHubOutgoingEvent>
    {
        public AnotherEdgeHubOutgoingEvent FromTableRow(TableRow row)
        {
            return new AnotherEdgeHubOutgoingEvent
            {
                Value = row["Value"]
            };
        }
    }


    class MessageInstanceFactory : IEventInstanceFactory<(string inputName, Message message)>
    {
        public (string inputName, Message message) FromTableRow(TableRow row)
        {
            var message = new Message(Encoding.UTF8.GetBytes(row["Payload"]));

            return (row["InputName"], message);
        }
    }

    class TopicInstanceFactory : IEventInstanceFactory<string>
    {
        public string FromTableRow(TableRow row)
        {
            return row["InputName"];
        }
    }
}
