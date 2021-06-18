using System;
using System.Threading.Tasks;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    public interface ITimescaledbClient 
    {
        public Task SetupClient();
        public Task IngestEventAsync<T>(T @event) where T: class;
    }
}