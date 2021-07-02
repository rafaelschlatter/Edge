using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Newtonsoft.Json;
using Autofac;
using Newtonsoft.Json.Serialization;
using MQTTnet;
using RaaLabs.Edge.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RaaLabs.Edge.Modules.Mqtt.Client.Producer
{
    class MqttProducerClient<T> : IMqttProducerClient<T>, IConsumeEventAsync<T>
        where T : IMqttOutgoingEvent
    {
        private readonly Channel<T> _eventsToProduce;
        private readonly IMqttBrokerClient _client;
        private readonly string _topic;
        private static Regex _substitutionPattern = new Regex(@"{(\w+)}");
        private bool _topicHasSubstitutions;
        private List<(string pattern, PropertyInfo property)> _substitutions;
        private readonly ISerializer<T> _serializer;

        public MqttProducerClient(ILifetimeScope scope)
        {
            var attribute = typeof(T).GetAttribute<MqttBrokerConnectionAttribute>();
            var brokerType = attribute.BrokerConnection;
            _topic = attribute.Topic;
            _serializer = scope.ResolveSerializer<T>(brokerType);
            _topicHasSubstitutions = _substitutionPattern.IsMatch(_topic);
            ValidateTopic(_topic);
            _substitutions = _substitutionPattern.Matches(_topic).Select(match => (match.Groups[0].Value, typeof(T).GetProperty(match.Groups[1].Value))).ToList();
            _client = (IMqttBrokerClient)scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(brokerType));
            _eventsToProduce = Channel.CreateUnbounded<T>();
        }

        public async Task HandleAsync(T @event)
        {
            await _eventsToProduce.Writer.WriteAsync(@event);
        }

        public async Task SetupClient()
        {
            while(true)
            {
                var @event = await _eventsToProduce.Reader.ReadAsync();
                var serialized = _serializer.Serialize(@event);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(BuildTopic(@event))
                    .WithPayload(Encoding.UTF8.GetBytes(serialized))
                    .WithAtLeastOnceQoS()
                    .Build();

                await _client.SendMessageAsync(message);
            }
        }

        private void ValidateTopic(string topic)
        {
            if (!_topicHasSubstitutions) return;

            var matches = _substitutionPattern.Matches(topic);
            var allEventProperties = typeof(T).GetProperties().Select(prop => prop.Name).ToHashSet();

            var propsNotFound = new List<string>();

            foreach (Match match in matches)
            {
                var expectedProperty = match.Groups[1].Value;
                if (!allEventProperties.Contains(expectedProperty))
                {
                    propsNotFound.Add(expectedProperty);
                }
            }

            if (propsNotFound.Count > 0)
            {
                throw new Exception($"MQTT Event type '{typeof(T).Name}' does not contain the following properties: {string.Join(", ", propsNotFound)}");
            }
        }

        private string BuildTopic(T @event)
        {
            if (!_topicHasSubstitutions)
            {
                return _topic;
            }

            var topic = _topic;

            foreach (var property in _substitutions)
            {
                topic = topic.Replace(property.pattern, (string)property.property.GetValue(@event));
            }

            return topic;
        }
    }
}
