using Autofac;
using MQTTnet;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Serialization;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    /// <summary>
    /// Converts an event to a MQTT message, and vice versa.
    /// </summary>
    public class MqttMessageConverter : IMqttMessageConverter
    {
        private readonly ILifetimeScope _scope;
        private readonly IMqttTopicMapper _topicMapper;
        private readonly Dictionary<Type, Func<IEvent, (Type, MqttApplicationMessage)>> _eventToMessageConverters = new();
        private readonly Dictionary<Type, Func<MqttApplicationMessage, IEvent>> _messageToEventConverters = new();

        public MqttMessageConverter(ILifetimeScope scope, IMqttTopicMapper topicMapper, IEventHandler<IMqttIncomingEvent> incomingHandler, IEventHandler<IMqttOutgoingEvent> outgoingHandler)
        {
            _topicMapper = topicMapper;
            _scope = scope;

            var allEventTypes = incomingHandler.GetSubtypes().Union(outgoingHandler.GetSubtypes()).ToHashSet();

            foreach (var eventType in allEventTypes)
            {
                GetType().GetMethod("SetupConvertersForType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .MakeGenericMethod(eventType)
                    .Invoke(this, Array.Empty<object>());
            }
        }

        /// <summary>
        /// Convert an MQTT message to an event.
        /// </summary>
        /// <param name="connection">The connection that received the message</param>
        /// <param name="message">The message to convert</param>
        /// <returns>the MQTT message converted to an event</returns>
        public IEvent ToEvent(Type connection, MqttApplicationMessage message)
        {
            var type = _topicMapper.Resolve(connection, message.Topic);
            return _messageToEventConverters.TryGetValue(type, out Func<MqttApplicationMessage, IEvent> converter) ? converter(message) : null;
        }

        /// <summary>
        /// Convert an event to an MQTT message.
        /// </summary>
        /// <param name="event">The event to convert</param>
        /// <returns>a tuple consisting of the connection to send to, and the event converted to a message</returns>
        public (Type connection, MqttApplicationMessage message)? ToMessage(IEvent @event)
        {
            return _eventToMessageConverters.TryGetValue(@event.GetType(), out Func<IEvent, (Type, MqttApplicationMessage)> converter) ? converter(@event) : null;
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupConvertersForType<T>() where T : class, IEvent
        {
            var attribute = typeof(T).GetAttribute<MqttBrokerConnectionAttribute>();
            var connection = attribute!.BrokerConnection;
            var serializer = _scope.ResolveSerializer<T>(connection);
            var deserializer = _scope.ResolveDeserializer<T>(connection);
            var topicTemplate = new TemplatedString<T>(attribute.Topic.Replace("+", "{_}"));

            _eventToMessageConverters.Add(typeof(T), (@event) =>
            {
                var serialized = serializer.Serialize(@event as T);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topicTemplate.BuildFrom(@event as T).Replace("{_}", "+"))
                    .WithPayload(Encoding.UTF8.GetBytes(serialized))
                    .WithAtLeastOnceQoS()
                    .Build();

                return (connection, message);
            });

            _messageToEventConverters.Add(typeof(T), (message) =>
            {
                var @event = deserializer.Deserialize(Encoding.UTF8.GetString(message.Payload));
                topicTemplate.ExtractTo(message.Topic, @event);

                return @event;
            });
        }
    }
}
