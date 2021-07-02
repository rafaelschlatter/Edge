using Newtonsoft.Json;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Autofac;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using RaaLabs.Edge.Serialization;

namespace RaaLabs.Edge.Modules.Mqtt.Client.Consumer
{
    class MqttConsumerClient<T> : IMqttConsumerClient<T>, IProduceEvent<T>
        where T : IMqttIncomingEvent
    {
        public event AsyncEventEmitter<T> EventReceived;

        private readonly IMqttBrokerClient _brokerClient;
        private readonly string _topic;
        private readonly IDeserializer<T> _deserializer;

        private readonly ILogger _logger;

        public MqttConsumerClient(ILogger logger, ILifetimeScope scope)
        {
            _logger = logger;
            var attr = typeof(T).GetAttribute<MqttBrokerConnectionAttribute>();
            var brokerType = attr.BrokerConnection;
            var connection = (IMqttBrokerConnection)scope.Resolve(brokerType);
            _brokerClient = (IMqttBrokerClient)scope.Resolve(typeof(IMqttBrokerClient<>).MakeGenericType(brokerType));

            _topic = attr.Topic;

            _deserializer = scope.ResolveDeserializer<T>(brokerType);
        }

        public async Task SetupClient()
        {

            await _brokerClient.SubscribeToTopic(_topic, async (client, message) =>
            {
                var payload = message.Payload;
                var @event = _deserializer.Deserialize(Encoding.UTF8.GetString(payload));
                await EventReceived(@event);
            });
        }
    }
}
