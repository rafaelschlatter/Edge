using RaaLabs.Edge.Modules.EventHandling;
using System.Threading.Tasks;
using Autofac;
using System.Text.RegularExpressions;

namespace RaaLabs.Edge.Modules.Mqtt.Client.Consumer
{
    class MqttConsumerClient<T> : IMqttConsumerClient<T>, IProduceEvent<T>
        where T : IMqttIncomingEvent
    {
        public event AsyncEventEmitter<T> EventReceived;

        private readonly IMqttBrokerClient _brokerClient;
        private readonly string _topic;
        private readonly MqttMessageConverter<T> _messageConverter;

        private readonly Regex _topicTokenPattern = new(@"{(?<token>[\d\w_]+)}");

        public MqttConsumerClient(ILifetimeScope scope, MqttMessageConverter<T> messageConverter)
        {
            var attr = typeof(T).GetAttribute<MqttBrokerConnectionAttribute>();
            var brokerType = attr.BrokerConnection;
            _brokerClient = (IMqttBrokerClient)scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(brokerType));
            _messageConverter = messageConverter;

            _topic = _topicTokenPattern.Replace(attr.Topic, "+");
        }

        public async Task SetupClient()
        {
            await _brokerClient.SubscribeToTopic(_topic, async (client, message) =>
            {
                var @event = _messageConverter.ToEvent(message);
                await EventReceived(@event);
            });
        }
    }
}
