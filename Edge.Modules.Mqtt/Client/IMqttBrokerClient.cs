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
    public interface IMqttBrokerClient<T> : IMqttBrokerClient where T : IMqttBrokerConnection
    {
    }

    public interface IMqttBrokerClient
    {
        public Task SetupClient();
        public Task SubscribeToTopic(string topicPattern, MessageReceivedDelegate eventHandler);
        public Task SendMessageAsync(MqttApplicationMessage message);
    }

    public delegate Task MessageReceivedDelegate(string clientId, MqttApplicationMessage message);
}
