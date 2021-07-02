using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt.Client.Consumer
{
    interface IMqttConsumerClient<T> : IMqttConsumerClient
        where T : IMqttIncomingEvent
    {
    }

    interface IMqttConsumerClient
    {
        public Task SetupClient();
    }
}
