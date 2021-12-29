using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Serialization;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.IotHub
{
    /// <summary>
    /// Converts an event to an IotHub message, and vice versa.
    /// </summary>
    public class IotHubMessageConverter : IIotHubMessageConverter
    {
        private readonly ILifetimeScope _scope;
        private readonly Dictionary<Type, Type> _incomingEventTypeForConnection;
        private readonly Dictionary<Type, Func<IEvent, (Type, Message)>> _eventToMessageConverters = new();
        private readonly Dictionary<Type, Func<Message, IEvent>> _messageToEventConverters = new();

        public IotHubMessageConverter(ILifetimeScope scope, IEventHandler<IIotHubIncomingEvent> incomingHandler, IEventHandler<IIotHubOutgoingEvent> outgoingHandler)
        {
            _scope = scope;

            var allEventTypes = incomingHandler.GetSubtypes().Union(outgoingHandler.GetSubtypes()).ToHashSet();
            _incomingEventTypeForConnection = allEventTypes
                .Where(type => type.IsAssignableTo<IIotHubIncomingEvent>())
                .Select(type => (type, attr: type.GetAttribute<IotHubConnectionAttribute>()))
                .ToDictionary(type => type.attr.Connection, type => type.type);

            foreach (var eventType in allEventTypes)
            {
                GetType().GetMethod("SetupConvertersForType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .MakeGenericMethod(eventType)
                    .Invoke(this, Array.Empty<object>());
            }
        }

        /// <summary>
        /// Convert an IotHub message to an event.
        /// </summary>
        /// <param name="connection">The connection that received the message</param>
        /// <param name="message">The message to convert</param>
        /// <returns>the IotHub message converted to an event</returns>
        public IEvent ToEvent(Type connection, Message message)
        {
            if (!_incomingEventTypeForConnection.TryGetValue(connection, out Type eventType)) return null;
            if (!_messageToEventConverters.TryGetValue(eventType, out Func<Message, IEvent> converter)) return null;

            return converter(message);
        }

        /// <summary>
        /// Convert an event to an IotHub message.
        /// </summary>
        /// <param name="event">The event to convert</param>
        /// <returns>a tuple consisting of the connection to send to, and the event converted to a message</returns>
        public (Type connection, Message message)? ToMessage(IEvent @event)
        {
            if (!_eventToMessageConverters.TryGetValue(@event.GetType(), out Func<IEvent, (Type, Message)> converter)) return null;
            
            return converter(@event);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupConvertersForType<T>() where T : class, IEvent
        {
            var attribute = typeof(T).GetAttribute<IotHubConnectionAttribute>();
            var connection = attribute.Connection;
            var serializer = _scope.ResolveSerializer<T>(connection);
            var deserializer = _scope.ResolveDeserializer<T>(connection);

            _eventToMessageConverters.Add(typeof(T), (@event) =>
            {
                var serializedEvent = serializer.Serialize(@event as T);
                var message = new Message(Encoding.UTF8.GetBytes(serializedEvent));

                return (connection, message);
            });

            _messageToEventConverters.Add(typeof(T), (message) =>
            {
                var messageBytes = message.GetBytes();
                var messageString = Encoding.UTF8.GetString(messageBytes);

                var @event = deserializer.Deserialize(messageString);

                return @event;
            });
        }
    }
}
