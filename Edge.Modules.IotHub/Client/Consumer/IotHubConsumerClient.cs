using RaaLabs.Edge.Serialization;
using RaaLabs.Edge.Modules.EventHandling;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Autofac;
using System.Threading.Channels;

namespace RaaLabs.Edge.Modules.IotHub.Client.Consumer
{
    class IotHubConsumerClient<T> : IIotHubConsumerClient<T>, IProduceEvent<T>
        where T : IIotHubIncomingEvent
    {
        public event AsyncEventEmitter<T> EventReceived;

        private readonly ILogger _logger;
        private readonly IIotHubClient _client;
        private readonly IDeserializer<T> _deserializer;

        public IotHubConsumerClient(ILogger logger, ILifetimeScope scope)
        {
            var connectionType = typeof(T).GetAttribute<IotHubConnectionAttribute>().Connection;
            _client = (IIotHubClient)scope.Resolve(typeof(IIotHubClient<>).MakeGenericType(connectionType));

            _deserializer = scope.ResolveDeserializer<T>(connectionType);

            _logger = logger;
        }

        public async Task SetupClient()
        {
            _logger.Information("Setting up consumer client for event type '{EventType}'", typeof(T).Name);
            var incomingEvents = Channel.CreateUnbounded<T>();

            await _client.Subscribe(async message =>
            {
                var messageBytes = message.GetBytes();
                var messageString = Encoding.UTF8.GetString(messageBytes);

                var @event = _deserializer.Deserialize(messageString);
                await incomingEvents.Writer.WriteAsync(@event);
            });

            while (true)
            {
                var @event = await incomingEvents.Reader.ReadAsync();
                await EventReceived(@event);
            }
        }
    }
}
