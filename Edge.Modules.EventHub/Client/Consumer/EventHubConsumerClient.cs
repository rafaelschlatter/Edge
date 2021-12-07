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
using Azure.Messaging.EventHubs;

namespace RaaLabs.Edge.Modules.EventHub.Client.Consumer
{
    class EventHubConsumerClient<ConnectionType> : IEventHubConsumerClient<ConnectionType>
        where ConnectionType : IEventHubConnection
    {
        private readonly ConnectionType _connection;

        public event DataReceivedDelegate<EventData> OnDataReceived;

        public EventHubConsumerClient(ConnectionType connection)
        {
            _connection = connection;
        }

        public async Task Connect()
        {
            var eventHubProcessor = await EventHubProcessor.FromEventHubConnection(_connection, async data => await OnDataReceived(typeof(ConnectionType), data));
            await eventHubProcessor.StartProcessingAsync();
        }
    }
}
