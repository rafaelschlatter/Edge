using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.IotHub.Client
{
    public interface IIotHubClient<ConnectionType> : IIotHubClient, IReceiverClient<ConnectionType, Message>, ISenderClient<ConnectionType, Message>
        where ConnectionType : IIotHubConnection
    {
    }

    public interface IIotHubClient : IReceiverClient<Message>, ISenderClient<Message>
    {
    }

    public delegate Task MessageReceivedDelegate(Type connection, Message message);
}
