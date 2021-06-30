using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using AzureEventHubsClient = Azure.Messaging.EventHubs.Producer.EventHubProducerClient;
using Azure.Messaging.EventHubs;
using Newtonsoft.Json;
using Serilog;
using RaaLabs.Edge.Serialization;
using Autofac;

namespace RaaLabs.Edge.Modules.EventHub.Client.Producer
{
    class EventHubProducerClient<T> : IEventHubProducerClient<T>, IConsumeEventAsync<T>
        where T : IEventHubOutgoingEvent
    {
        private readonly Channel<T> _eventsToProduce;
        private readonly IEventHubConnection _connection;
        private readonly ILogger _logger;
        private readonly ISerializer<T> _serializer;

        public EventHubProducerClient(ILogger logger, ILifetimeScope scope)
        {
            _logger = logger;
            var connectionType = typeof(T).GetAttribute<EventHubConnectionAttribute>().Connection;
            _connection = (IEventHubConnection)scope.Resolve(connectionType);
            _eventsToProduce = Channel.CreateUnbounded<T>();

            _serializer =
                   (ISerializer<T>)scope.ResolveOptional(typeof(ISerializer<,>).MakeGenericType(typeof(T), connectionType))
                ?? (ISerializer<T>)scope.ResolveOptional(typeof(ISerializer<>).MakeGenericType(typeof(T)))
                ?? new JsonSerializer<T>();
        }

        public async Task HandleAsync(T @event)
        {
            await _eventsToProduce.Writer.WriteAsync(@event);
        }

        public async Task SetupClient()
        {
            _logger.Information("Setting up EventHubProducerClient for event type '{EventType}'", typeof(T).Name);
            AzureEventHubsClient producer = new AzureEventHubsClient(_connection.ConnectionString);
            var batchedEvents = await producer.CreateBatchAsync();
            var batchSize = 10;

            while (true)
            {
                var @event = await _eventsToProduce.Reader.ReadAsync();

                var serialized = Encoding.UTF8.GetBytes(_serializer.Serialize(@event));
                var data = new EventData(serialized);
                batchedEvents.TryAdd(data);

                if (batchedEvents.Count >= batchSize)
                {
                    await producer.SendAsync(batchedEvents);
                    batchedEvents = await producer.CreateBatchAsync();
                }
            }
        }
    }
}
