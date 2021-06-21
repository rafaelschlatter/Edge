using Newtonsoft.Json;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Serilog;

namespace RaaLabs.Edge.Modules.IotHub.Client.Consumer
{
    class IotHubConsumerClient<T> : IIotHubConsumerClient<T>, IProduceEvent<T>
        where T : IIotHubIncomingEvent
    {
        public event AsyncEventEmitter<T> EventReceived;

        private readonly string _iotHubName;
        private readonly ILogger _logger;

        public IotHubConsumerClient(ILogger logger)
        {
            _iotHubName = typeof(T).GetAttribute<IotHubNameAttribute>()?.IotHubName;
            _logger = logger;
        }

        public async Task SetupClient()
        {
            var environmentVariablePrefix = _iotHubName.ToUpper().Replace("-", "");
            var iotHubConnectionString = Environment.GetEnvironmentVariable(environmentVariablePrefix + "_CONNECTION_STRING");

            var consumer = DeviceClient.CreateFromConnectionString(iotHubConnectionString, TransportType.Amqp);
            await consumer.OpenAsync();

            while(true)
            {
                try
                {
                    var message = await consumer.ReceiveAsync();
                    var messageBytes = message.GetBytes();
                    var messageString = Encoding.UTF8.GetString(messageBytes);

                    var @event = JsonConvert.DeserializeObject<T>(messageString);
                    await EventReceived(@event);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Exception while reading from IotHub {IotHubName}: '{Message}'", _iotHubName, ex.Message);
                }
            }
        }
    }
}
