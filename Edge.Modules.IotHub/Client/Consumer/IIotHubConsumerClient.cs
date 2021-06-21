using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.IotHub.Client.Consumer
{
    interface IIotHubConsumerClient<T> : IIotHubConsumerClient
        where T : IIotHubIncomingEvent
    {
    }

    interface IIotHubConsumerClient
    {
        public Task SetupClient();
    }
}
