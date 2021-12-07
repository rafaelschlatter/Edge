using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RaaLabs.Edge.Modules.Timescaledb
{
    public class TimescaledbClient<ConnectionType> : ITimescaledbClient<ConnectionType>
        where ConnectionType : ITimescaledbConnection
    {
        private NpgsqlConnection _client;
        private readonly ConnectionType _connection;
        private readonly Dictionary<Type, Func<object, Task>> _ingestMethods = new();

        public TimescaledbClient(ConnectionType connection)
        {
            _connection = connection;
        }

        public async Task Connect()
        {
            _client = new NpgsqlConnection(_connection.ConnectionString);
            await _client.OpenAsync();
        }
        
        public async Task SendAsync(object data)
        {
            var dataType = data.GetType();
            if (!_ingestMethods.TryGetValue(dataType, out var ingestMethod))
            {
                ingestMethod = (Func<object, Task>)GetType().GetMethod("BuildIngestMethodForType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.MakeGenericMethod(dataType).Invoke(this, Array.Empty<object>());
                _ingestMethods.Add(dataType, ingestMethod);
            }

            await ingestMethod!(data);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private Func<object, Task> BuildIngestMethodForType<T>()
            where T : class
        {
            return async data =>
            {
                await _client.InsertAsync((T) data);
            };
        }
    }
}