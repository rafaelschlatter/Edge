using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using FluentAssertions;
using System.Text.RegularExpressions;
using MQTTnet;
using System.Text;

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Drivers
{
    class SomeMqttIncomingEventInstanceFactory : IEventInstanceFactory<SomeMqttIncomingEvent>
    {
        public SomeMqttIncomingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeMqttIncomingEvent
            {
                Value = value
            };
        }
    }

    class AnotherMqttIncomingEventInstanceFactory : IEventInstanceFactory<AnotherMqttIncomingEvent>
    {
        private static Regex _topicPattern = new (@"site1/area2/input/([\w\d_]+)");
        public AnotherMqttIncomingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            string sensor = row.TryGetValue("Topic", out string topic) ? _topicPattern.Match(topic).Groups[1].Value : row["Sensor"];
            return new AnotherMqttIncomingEvent
            {
                Value = value,
                Sensor = sensor
            };
        }
    }

    class SomeMqttOutgoingEventInstanceFactory : IEventInstanceFactory<SomeMqttOutgoingEvent>
    {
        public SomeMqttOutgoingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            return new SomeMqttOutgoingEvent
            {
                Value = value
            };
        }
    }

    class AnotherMqttOutgoingEventInstanceFactory : IEventInstanceFactory<AnotherMqttOutgoingEvent>
    {
        private static Regex _topicPattern = new(@"site1/area2/input/([\w\d_]+)");
        public AnotherMqttOutgoingEvent FromTableRow(TableRow row)
        {
            int value = int.Parse(row["Value"]);
            string sensor = row.TryGetValue("Topic", out string topic) ? _topicPattern.Match(topic).Groups[1].Value : row["Sensor"];
            return new AnotherMqttOutgoingEvent
            {
                Value = value,
                Sensor = sensor
            };
        }
    }

    class TopicInstanceFactory : IEventInstanceFactory<string>
    {
        public string FromTableRow(TableRow row)
        {
            return row["Topic"];
        }
    }

    class MqttApplicationMessageInstanceFactory : IEventInstanceFactory<MqttApplicationMessage>
    {
        public MqttApplicationMessage FromTableRow(TableRow row)
        {
            var message = new MqttApplicationMessage
            {
                Payload = Encoding.UTF8.GetBytes(row["Payload"]),
                Topic = row["Topic"]
            };

            return message;
        }
    }

}
