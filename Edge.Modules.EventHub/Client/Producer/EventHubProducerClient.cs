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

namespace RaaLabs.Edge.Modules.EventHub.Client.Producer
{
    class EventHubProducerClient<T> : IEventHubProducerClient<T>, IConsumeEventAsync<T>
        where T : IEventHubOutgoingEvent
    {
        private readonly Channel<T> _eventsToProduce;
        private readonly string _eventHubName;

        public EventHubProducerClient()
        {
            _eventHubName = typeof(T).GetAttribute<EventHubNameAttribute>()?.EventHubName;
            _eventsToProduce = Channel.CreateUnbounded<T>();
        }

        public async Task HandleAsync(T @event)
        {
            await _eventsToProduce.Writer.WriteAsync(@event);
        }

        public async Task SetupClient()
        {
            var environmentVariablePrefix = _eventHubName.ToUpper().Replace("-", "");
            var eventHubConnectionString = Environment.GetEnvironmentVariable(environmentVariablePrefix + "_CONNECTION_STRING");
            AzureEventHubsClient producer = new AzureEventHubsClient(eventHubConnectionString);
            var batchedEvents = await producer.CreateBatchAsync();
            var batchSize = 10;

            while (true)
            {
                var @event = await _eventsToProduce.Reader.ReadAsync();

                var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
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
