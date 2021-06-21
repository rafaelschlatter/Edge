using Newtonsoft.Json;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub.Client.Consumer
{
    class EventHubConsumerClient<T> : IEventHubConsumerClient<T>, IProduceEvent<T>
        where T : IEventHubIncomingEvent
    {
        public event AsyncEventEmitter<T> EventReceived;

        private readonly string _eventHubName;
        private EventHubProcessor _eventHubProcessor;

        public EventHubConsumerClient()
        {
            _eventHubName = typeof(T).GetAttribute<EventHubNameAttribute>()?.EventHubName;
        }

        public async Task SetupClient()
        {
            var incomingEvents = Channel.CreateUnbounded<T>();
            _eventHubProcessor = await EventHubProcessor.FromEventHubName(_eventHubName, async message =>
            {
                var @event = JsonConvert.DeserializeObject<T>(message);
                await incomingEvents.Writer.WriteAsync(@event);
            });

            var startedProcessorTask = _eventHubProcessor.StartProcessingAsync();

            while(true)
            {
                var @event = await incomingEvents.Reader.ReadAsync();
                await EventReceived(@event);
            }
        }
    }
}
