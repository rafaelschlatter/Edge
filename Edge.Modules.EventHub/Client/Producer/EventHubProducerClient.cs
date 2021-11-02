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
using Azure.Messaging.EventHubs.Producer;

namespace RaaLabs.Edge.Modules.EventHub.Client.Producer
{
    class EventHubProducerClient<ConnectionType> : IEventHubProducerClient<ConnectionType>
        where ConnectionType : IEventHubConnection
    {
        private readonly ConnectionType _connection;
        private AzureEventHubsClient _client;

        // Automatically batch event data
        private readonly DataBatcher<EventData> _dataBatcher = new(TimeSpan.FromMilliseconds(100), 100);

        public EventHubProducerClient(ConnectionType connection)
        {
            _connection = connection;
            _dataBatcher.OnDataBatched += SendBatchAsync;
        }

        public async Task SendAsync(EventData data)
        {
            await _dataBatcher.Enqueue(data);
        }

        public async Task SendBatchAsync(IEnumerable<EventData> data)
        {
            if (_client == null) await Connect();

            await foreach (var batch in BatchEventData(data))
            {
                await _client.SendAsync(batch);
            }
        }

        public async Task Connect()
        {
            _client = new AzureEventHubsClient(_connection.ConnectionString);
            await Task.CompletedTask;
        }

        private async IAsyncEnumerable<EventDataBatch> BatchEventData(IEnumerable<EventData> data)
        {
            var batch = await _client.CreateBatchAsync();
            foreach (var d in data)
            {
                if (!batch.TryAdd(d))
                {
                    yield return batch;
                    batch = await _client.CreateBatchAsync();
                    batch.TryAdd(d);
                }
            }

            if (batch.Count > 0) yield return batch;
        }

    }
}
