using System;
using System.Threading.Tasks;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    public interface ITimescaledbClient<ConnectionType> : ITimescaledbClient, ISenderClient<ConnectionType, object>
        where ConnectionType : ITimescaledbConnection
    {
    }

    public interface ITimescaledbClient : ISenderClient<object>
    {
    }
}