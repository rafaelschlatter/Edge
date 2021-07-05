using Autofac;
using MQTTnet;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    public class MqttMessageConverter<T> : IMqttMessageConverter<T>
    {
        private readonly ISerializer<T> _serializer;
        private readonly IDeserializer<T> _deserializer;
        private readonly string _topic;

        private readonly TemplatedString<T> _topicTemplate;

        public MqttMessageConverter(ILifetimeScope scope)
        {
            var attribute = typeof(T).GetAttribute<MqttBrokerConnectionAttribute>();
            var brokerType = attribute.BrokerConnection;
            _topic = attribute.Topic;
            _topicTemplate = new TemplatedString<T>(_topic.Replace("+", "{_}"));
            _serializer = scope.ResolveSerializer<T>(brokerType);
            _deserializer = scope.ResolveDeserializer<T>(brokerType);
        }

        public T ToEvent(MqttApplicationMessage message)
        {
            var @event = _deserializer.Deserialize(Encoding.UTF8.GetString(message.Payload));
            _topicTemplate.ExtractTo(message.Topic, @event);

            return @event;
        }

        public MqttApplicationMessage ToMessage(T @event)
        {
            var serialized = _serializer.Serialize(@event);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_topicTemplate.BuildFrom(@event).Replace("{_}", "+"))
                .WithPayload(Encoding.UTF8.GetBytes(serialized))
                .WithAtLeastOnceQoS()
                .Build();

            return message;
        }
    }
}
