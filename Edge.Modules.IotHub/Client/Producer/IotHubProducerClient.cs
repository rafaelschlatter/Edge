using RaaLabs.Edge.Modules.EventHandling;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Azure.Devices.Client;
using Autofac;
using RaaLabs.Edge.Serialization;

namespace RaaLabs.Edge.Modules.IotHub.Client.Producer
{
    class IotHubProducerClient<T> : IIotHubProducerClient<T>, IConsumeEventAsync<T>
        where T : IIotHubOutgoingEvent
    {
        private readonly IIotHubClient _client;
        private readonly ISerializer<T> _serializer;
        private readonly IIotHubConnection _connection;
        private readonly Channel<T> _pendingOutgoingEvents;

        public IotHubProducerClient(ILifetimeScope scope)
        {
            _pendingOutgoingEvents = Channel.CreateUnbounded<T>();
            var connectionType = typeof(T).GetAttribute<IotHubConnectionAttribute>().Connection;
            _connection = (IIotHubConnection)scope.Resolve(connectionType);

            _serializer = scope.ResolveSerializer<T>(connectionType);

            _client = (IIotHubClient)scope.Resolve(typeof(IIotHubClient<>).MakeGenericType(connectionType));
        }

        public async Task HandleAsync(T @event)
        {
            await _pendingOutgoingEvents.Writer.WriteAsync(@event);
            var serializedEvent = _serializer.Serialize(@event);
            await _client.SendMessageAsync(new Message(Encoding.UTF8.GetBytes(serializedEvent)));
        }

        public async Task SetupClient()
        {
            while (true)
            {
                var @event = await _pendingOutgoingEvents.Reader.ReadAsync();
                var serializedEvent = _serializer.Serialize(@event);
                await _client.SendMessageAsync(new Message(Encoding.UTF8.GetBytes(serializedEvent)));
            }
        }
    }
}
