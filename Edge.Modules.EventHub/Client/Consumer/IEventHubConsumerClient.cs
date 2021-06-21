using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub.Client.Consumer
{
    interface IEventHubConsumerClient<T> : IEventHubConsumerClient
        where T : IEventHubIncomingEvent
    {
    }

    interface IEventHubConsumerClient
    {
        public Task SetupClient();
    }
}
