using Microsoft.Azure.Devices.Client;
using Serilog;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;
using Dapper.Contrib.Extensions;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    public class TimescaledbClient : ITimescaledbClient
    {
        private static string timescaledbConnectionString = Environment.GetEnvironmentVariable("TIMESCALEDB_CONNECTION_STRING");
        private NpgsqlConnection _client; 

        public async Task SetupClient()
        {
            _client = new NpgsqlConnection(timescaledbConnectionString);
            await _client.OpenAsync();
        }

        public async Task IngestEventAsync<T>(T @event)
            where T: class
        {
            IDbConnection clientConnection = _client; 
            clientConnection.Insert(@event);
            await Task.CompletedTask;
        }
    }
}