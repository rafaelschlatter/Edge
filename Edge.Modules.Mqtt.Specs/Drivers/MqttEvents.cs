using RaaLabs.Edge.Modules.Mqtt.Client.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt.Specs.Drivers
{
    public class MqttBrokerConnection : IMqttBrokerConnection
    {
        public string Ip { get; set; } = "localhost";
        public int Port { get; set; } = 1337;
        public string ClientId { get; set; } = "ThisClient";
        public IAuthentication Authentication { get; set; } = null;
    }


    [MqttBrokerConnection(typeof(MqttBrokerConnection), "site1/area1/input/sensor1")]
    public class SomeMqttIncomingEvent : IMqttIncomingEvent
    {
        public int Value { get; set; }
    }

    [MqttBrokerConnection(typeof(MqttBrokerConnection), "site1/area2/input/{Sensor}")]
    public class AnotherMqttIncomingEvent : IMqttIncomingEvent
    {
        [JsonIgnore]
        public string Sensor { get; set; }
        public int Value { get; set; }
    }

    [MqttBrokerConnection(typeof(MqttBrokerConnection), "site1/area1/output/sensor1")]
    public class SomeMqttOutgoingEvent : IMqttOutgoingEvent
    {
        public int Value { get; set; }
    }

    [MqttBrokerConnection(typeof(MqttBrokerConnection), "site1/area2/output/{Sensor}")]
    public class AnotherMqttOutgoingEvent : IMqttOutgoingEvent
    {
        [JsonIgnore]
        public string Sensor { get; set; }
        public int Value { get; set; }
    }
}
