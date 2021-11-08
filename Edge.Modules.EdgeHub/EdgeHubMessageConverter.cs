using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Serialization;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// Converts an event to an EdgeHub message, and vice versa.
    /// </summary>
    public class EdgeHubMessageConverter : IEdgeHubMessageConverter
    {
        private readonly ILifetimeScope _scope;

        // Whenever a new message is received, the only thing we know about the message is the input name. To know what data type this input is mapped to,
        // we store this mapping in a dictionary.
        private readonly Dictionary<string, Type> _incomingEventTypeForInputName;

        private readonly Dictionary<Type, Func<IEvent, (string, Message)>> _eventToMessageConverters = new();
        private readonly Dictionary<Type, Func<Message, IEvent>> _messageToEventConverters = new();

        public EdgeHubMessageConverter(ILifetimeScope scope, IEventHandler<IEdgeHubIncomingEvent> incomingHandler, IEventHandler<IEdgeHubOutgoingEvent> outgoingHandler)
        {
            _scope = scope;

            var inputTypes = incomingHandler.GetSubtypes();
            var outputTypes = outgoingHandler.GetSubtypes();

            _incomingEventTypeForInputName = inputTypes
                .Select(type => (type, attr: type.GetAttribute<InputNameAttribute>()))
                .ToDictionary(type => type.attr.InputName, type => type.type);

            foreach (var eventType in inputTypes)
            {
                GetType().GetMethod("SetupConvertersForInputType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .MakeGenericMethod(eventType)
                    .Invoke(this, Array.Empty<object>());
            }

            foreach (var eventType in outputTypes)
            {
                GetType().GetMethod("SetupConvertersForOutputType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .MakeGenericMethod(eventType)
                    .Invoke(this, Array.Empty<object>());
            }
        }

        /// <summary>
        /// Convert an EdgeHub message to an event.
        /// </summary>
        /// <param name="inputName">The input that received the message</param>
        /// <param name="message">The message to convert</param>
        /// <returns>the IotHub message converted to an event</returns>
        public IEvent ToEvent(string inputName, Message message)
        {
            if (!_incomingEventTypeForInputName.TryGetValue(inputName, out Type eventType)) return null;
            if (!_messageToEventConverters.TryGetValue(eventType, out Func<Message, IEvent> converter)) return null;

            return converter(message);
        }

        /// <summary>
        /// Convert an event to an EdgeHub message.
        /// </summary>
        /// <param name="event">The event to convert</param>
        /// <returns>a tuple consisting of the output to send to, and the event converted to a message</returns>
        public (string outputName, Message message)? ToMessage(IEvent @event)
        {
            if (!_eventToMessageConverters.TryGetValue(@event.GetType(), out Func<IEvent, (string, Message)> converter)) return null;

            return converter(@event);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupConvertersForInputType<T>() where T : class, IEvent
        {
            var attribute = typeof(T).GetAttribute<InputNameAttribute>();
            var inputName = attribute.InputName;
            var deserializer = _scope.ResolveDeserializer<T>();

            _messageToEventConverters.Add(typeof(T), (message) =>
            {
                var messageBytes = message.GetBytes();
                var messageString = Encoding.UTF8.GetString(messageBytes);

                var @event = deserializer.Deserialize(messageString);

                return @event;
            });
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private void SetupConvertersForOutputType<T>() where T : class, IEvent
        {
            var attribute = typeof(T).GetAttribute<OutputNameAttribute>();
            var outputName = attribute.OutputName;
            var serializer = _scope.ResolveSerializer<T>();

            _eventToMessageConverters.Add(typeof(T), (@event) =>
            {
                var serializedEvent = serializer.Serialize(@event as T);
                var message = new Message(Encoding.UTF8.GetBytes(serializedEvent));

                return (outputName, message);
            });
        }
    }
}
