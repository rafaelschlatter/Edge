using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Azure.Devices.Client;


namespace RaaLabs.Edge.Modules.IotHub.Client
{
    public interface IIotHubClient<T> : IIotHubClient where T : IIotHubConnection
    {
    }

    public interface IIotHubClient
    {
        public Task SetupClient();
        public Task Subscribe(MessageReceivedDelegate eventHandler);
        public Task SendMessageAsync(Message message);
    }

    public delegate Task MessageReceivedDelegate(Message message);
}
