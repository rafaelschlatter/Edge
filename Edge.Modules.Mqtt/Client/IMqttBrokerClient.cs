using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;


namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    public interface IMqttBrokerClient<ConnectionType> : IMqttBrokerClient, ISubscribingReceiverClient<ConnectionType, MqttApplicationMessage, string>, ISenderClient<ConnectionType, MqttApplicationMessage>
        where ConnectionType : IMqttBrokerConnection
    {
    }

    public interface IMqttBrokerClient : ISubscribingReceiverClient<MqttApplicationMessage, string>, ISenderClient<MqttApplicationMessage>
    {
    }
}
