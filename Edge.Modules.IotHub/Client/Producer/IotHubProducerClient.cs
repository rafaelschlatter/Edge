using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace RaaLabs.Edge.Modules.IotHub.Client.Producer
{
    class IotHubProducerClient<T> : IIotHubProducerClient<T>, IConsumeEventAsync<T>
        where T : IIotHubOutgoingEvent
    {
        private readonly Channel<T> _eventsToProduce;
        private readonly string _iotHubName;

        public IotHubProducerClient()
        {
            _iotHubName = typeof(T).GetAttribute<IotHubNameAttribute>()?.IotHubName;
            _eventsToProduce = Channel.CreateUnbounded<T>();
        }

        public async Task HandleAsync(T @event)
        {
            await _eventsToProduce.Writer.WriteAsync(@event);
        }

        public async Task SetupClient()
        {
            var environmentVariablePrefix = _iotHubName.ToUpper().Replace("-", "");
            var iotHubConnectionString = Environment.GetEnvironmentVariable(environmentVariablePrefix + "_CONNECTION_STRING");

            var producer = DeviceClient.CreateFromConnectionString(iotHubConnectionString, TransportType.Amqp);

            var batchedEvents = new List<Message>();
            var batchSize = 64;

            while (true)
            {
                var @event = await _eventsToProduce.Reader.ReadAsync();

                var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
                var data = new Message(serialized);
                batchedEvents.Add(data);

                if (batchedEvents.Count >= batchSize)
                {
                    await producer.SendEventBatchAsync(batchedEvents);
                    batchedEvents = new List<Message>();
                }
            }
        }
    }
}
