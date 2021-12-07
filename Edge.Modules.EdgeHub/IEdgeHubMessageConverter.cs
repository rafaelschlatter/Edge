using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    public interface IEdgeHubMessageConverter
    {
        public IEvent ToEvent(string inputName, Message message);
        public (string outputName, Message message)? ToMessage(IEvent @event);
    }
}
