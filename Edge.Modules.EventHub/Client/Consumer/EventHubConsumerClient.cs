using Newtonsoft.Json;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;
using RaaLabs.Edge.Serialization;
using Autofac;

namespace RaaLabs.Edge.Modules.EventHub.Client.Consumer
{
    class EventHubConsumerClient<T> : IEventHubConsumerClient<T>, IProduceEvent<T>
        where T : IEventHubIncomingEvent
    {
        public event AsyncEventEmitter<T> EventReceived;

        private EventHubProcessor _eventHubProcessor;
        private ILogger _logger;
        private readonly IDeserializer<T> _deserializer;
        private readonly IEventHubConnection _connection;

        public EventHubConsumerClient(ILogger logger, ILifetimeScope scope)
        {
            _logger = logger;
            var connection = typeof(T).GetAttribute<EventHubConnectionAttribute>();
            _connection = (IEventHubConnection)scope.Resolve(connection.Connection);
            _deserializer =
                   (IDeserializer<T>)scope.ResolveOptional(typeof(IDeserializer<,>).MakeGenericType(typeof(T), connection.Connection))
                ?? (IDeserializer<T>)scope.ResolveOptional(typeof(IDeserializer<>).MakeGenericType(typeof(T)))
                ?? new JsonDeserializer<T>();
        }

        public async Task SetupClient()
        {
            _logger.Information("Setting up consumer client for event type '{EventType}'", typeof(T).Name);
            var incomingEvents = Channel.CreateUnbounded<T>();
            _eventHubProcessor = await EventHubProcessor.FromEventHubConnection(_connection, async message =>
            {
                _logger.Information("Incoming message for event type '{EventType}'", typeof(T).Name);
                var @event = _deserializer.Deserialize(message);
                await incomingEvents.Writer.WriteAsync(@event);
            });

            var startedProcessorTask = _eventHubProcessor.StartProcessingAsync();

            while (true)
            {
                var @event = await incomingEvents.Reader.ReadAsync();
                _logger.Information("Received message for event type '{EventType}'", typeof(T).Name);
                await EventReceived(@event);
            }
        }
    }
}
