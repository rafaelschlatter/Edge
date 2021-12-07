using System.Text;
using Microsoft.Azure.Devices.Client;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.Modules.IotHub.Specs.Drivers
{
    class SomeIotHubIncomingEventInstanceFactory : IEventInstanceFactory<SomeIotHubIncomingEvent>
    {
        public SomeIotHubIncomingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeIotHubIncomingEvent
            {
                Value = value
            };
        }
    }

    class SomeIotHubOutgoingEventInstanceFactory : IEventInstanceFactory<SomeIotHubOutgoingEvent>
    {
        public SomeIotHubOutgoingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeIotHubOutgoingEvent
            {
                Value = value
            };
        }
    }

    class MessageInstanceFactory : IEventInstanceFactory<Message>
    {
        public Message FromTableRow(TableRow row)
        {
            return new Message(Encoding.UTF8.GetBytes(row["Payload"]));
        }
    }

}
