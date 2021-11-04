using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Serialization;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using Azure.Messaging.EventHubs;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.EventHub
{
    /// <summary>
    /// Converts an event to an EventHub message, and vice versa.
    /// </summary>
    public class EventHubEventDataConverter : IEventHubEventDataConverter
    {
        private readonly ILifetimeScope _scope;
        private readonly Dictionary<Type, Type> _incomingEventTypeForConnection;
        private readonly Dictionary<Type, Func<IEvent, (Type, EventData)>> _eventToEventDataConverters = new();
        private readonly Dictionary<Type, Func<EventData, IEvent>> _eventDataToEventConverters = new();
        

        public EventHubEventDataConverter(ILifetimeScope scope, IEventHandler<IEventHubIncomingEvent> incomingHandler, IEventHandler<IEventHubOutgoingEvent> outgoingHandler)
        {
            _scope = scope;

            var allEventTypes = incomingHandler.GetSubtypes().Union(outgoingHandler.GetSubtypes()).ToHashSet();
            _incomingEventTypeForConnection = allEventTypes
                .Where(type => type.IsAssignableTo<IEventHubIncomingEvent>())
                .Select(type => (type, attr: type.GetAttribute<EventHubConnectionAttribute>()))
                .ToDictionary(type => type.attr.Connection, type => type.type);

            foreach (var eventType in allEventTypes)
            {
                GetType().GetMethod("SetupConvertersForType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .MakeGenericMethod(eventType)
                    .Invoke(this, Array.Empty<object>());
            }
        }

        /// <summary>
        /// Convert an EventHub message to an event.
        /// </summary>
        /// <param name="connection">The connection that received the message</param>
        /// <param name="data">The event data to convert</param>
        /// <returns>the EventHub EventData converted to an event</returns>
        public IEvent ToEvent(Type connection, EventData data)
        {
            if (!_incomingEventTypeForConnection.TryGetValue(connection, out Type eventType)) return null;
            if (!_eventDataToEventConverters.TryGetValue(eventType, out Func<EventData, IEvent> converter)) return null;

            return converter(data);
        }

        /// <summary>
        /// Convert an event to an EventHub message.
        /// </summary>
        /// <param name="event">The event to convert</param>
        /// <returns>a tuple consisting of the connection to send to, and the event converted to event data</returns>
        public (Type connection, EventData data)? ToEventData(IEvent @event)
        {
            if (!_eventToEventDataConverters.TryGetValue(@event.GetType(), out Func<IEvent, (Type, EventData)> converter)) return null;
            
            return converter(@event);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupConvertersForType<T>() where T : class, IEvent
        {
            var attribute = typeof(T).GetAttribute<EventHubConnectionAttribute>();
            var connection = attribute.Connection;
            var serializer = _scope.ResolveSerializer<T>(connection);
            var deserializer = _scope.ResolveDeserializer<T>(connection);

            _eventToEventDataConverters.Add(typeof(T), (@event) =>
            {
                var serializedEvent = serializer.Serialize(@event as T);
                var data = new EventData(Encoding.UTF8.GetBytes(serializedEvent));

                return (connection, data);
            });

            _eventDataToEventConverters.Add(typeof(T), (data) =>
            {
                var dataBytes = data.EventBody.ToArray();
                var dataString = Encoding.UTF8.GetString(dataBytes);

                var @event = deserializer.Deserialize(dataString);

                return @event;
            });
        }
    }
}
