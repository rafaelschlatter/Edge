using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub.Client.Producer
{
    interface IEventHubProducerClient<T> : IEventHubProducerClient
        where T : IEventHubOutgoingEvent
    {
    }

    interface IEventHubProducerClient
    {
        public Task SetupClient();
    }
}
