using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Reflection;
using Serilog;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Serialization;
using Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    class TimescaledbBridge : IBridge, IConsumeEventAsync<ITimescaledbOutgoingEvent>
    {        
        private ChannelReader<ITimescaledbOutgoingEvent> _outgoingEventsReader;
        private ChannelWriter<ITimescaledbOutgoingEvent> _outgoingEventsWriter;
        private readonly EventHandling.EventHandler<ITimescaledbOutgoingEvent> _outgoingHandler;
        private readonly ILogger _logger;
        private readonly ITimescaledbClient _client;

        public TimescaledbBridge(ILogger logger, EventHandling.EventHandler<ITimescaledbOutgoingEvent> outgoingHandler, ITimescaledbClient client)
        {
            _logger = logger;
            _outgoingHandler = outgoingHandler;
            var outgoingChannel = Channel.CreateUnbounded<ITimescaledbOutgoingEvent>();
            _outgoingEventsReader = outgoingChannel.Reader;
            _outgoingEventsWriter = outgoingChannel.Writer;
            _client = client;
        }

        public async Task HandleAsync(ITimescaledbOutgoingEvent @event)
        {
            await _outgoingEventsWriter.WriteAsync(@event);
        }

        public async Task SetupBridge()
        {
            await SetupOutgoingEvents();
        }
        private async Task SetupOutgoingEvents()
        {
            await _client.SetupClient();
            while (true)
            {
                var outgoingEvent = await _outgoingEventsReader.ReadAsync();
                var insertEventMethod = GetType().GetMethod("InsertEvent", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(outgoingEvent.GetType());
                await (Task) insertEventMethod.Invoke(this, new object[]{outgoingEvent});
            }
        }
        
        private async Task InsertEvent<T>(T @event)
            where T: class
        {
            await _client.IngestEventAsync(@event);
        }
    }
}
