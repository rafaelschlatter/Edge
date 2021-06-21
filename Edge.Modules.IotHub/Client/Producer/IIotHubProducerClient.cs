using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.IotHub.Client.Producer
{
    interface IIotHubProducerClient<T> : IIotHubProducerClient
        where T : IIotHubOutgoingEvent
    {
    }

    interface IIotHubProducerClient
    {
        public Task SetupClient();
    }
}
