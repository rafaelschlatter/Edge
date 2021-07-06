using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt.Client.Producer
{
    interface IMqttProducerClient<T> : IMqttProducerClient
        where T : IMqttOutgoingEvent
    {
    }

    interface IMqttProducerClient
    {
        public Task SetupClient();
    }
}
