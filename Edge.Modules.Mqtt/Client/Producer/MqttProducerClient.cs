using RaaLabs.Edge.Modules.EventHandling;
using System.Threading.Tasks;
using System.Threading.Channels;
using Autofac;

namespace RaaLabs.Edge.Modules.Mqtt.Client.Producer
{
    class MqttProducerClient<T> : IMqttProducerClient<T>, IConsumeEventAsync<T>
        where T : IMqttOutgoingEvent
    {
        private readonly Channel<T> _eventsToProduce;
        private readonly IMqttBrokerClient _client;
        private readonly MqttMessageConverter<T> _messageConverter;

        public MqttProducerClient(ILifetimeScope scope, MqttMessageConverter<T> messageConverter)
        {
            var attribute = typeof(T).GetAttribute<MqttBrokerConnectionAttribute>();
            var brokerType = attribute.BrokerConnection;
            _client = (IMqttBrokerClient)scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(brokerType));
            _eventsToProduce = Channel.CreateUnbounded<T>();
            _messageConverter = messageConverter;
        }

        public async Task HandleAsync(T @event)
        {
            await _eventsToProduce.Writer.WriteAsync(@event);
        }

        public async Task SetupClient()
        {
            while(true)
            {
                var @event = await _eventsToProduce.Reader.ReadAsync();

                var message = _messageConverter.ToMessage(@event);

                await _client.SendMessageAsync(message);
            }
        }
    }
}
