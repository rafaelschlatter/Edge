using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Serilog;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json.Serialization;
using Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    class EdgeHubBridge : IBridge, IConsumeEventAsync<IEdgeHubOutgoingEvent>, IProduceEvent<IEdgeHubIncomingEvent>
    {
        public event AsyncEventEmitter<IEdgeHubIncomingEvent> IncomingEvent;
        
        private ChannelReader<IEdgeHubOutgoingEvent> _outgoingEventsReader;
        private ChannelWriter<IEdgeHubOutgoingEvent> _outgoingEventsWriter;

        private readonly EventHandling.EventHandler<IEdgeHubIncomingEvent> _incomingHandler;
        private readonly EventHandling.EventHandler<IEdgeHubOutgoingEvent> _outgoingHandler;
        private readonly IIotModuleClient _client;
        private readonly ILogger _logger;

        public EdgeHubBridge(ILogger logger, IIotModuleClient client, EventHandling.EventHandler<IEdgeHubIncomingEvent> incomingHandler, EventHandling.EventHandler<IEdgeHubOutgoingEvent> outgoingHandler)
        {
            _logger = logger;
            _incomingHandler = incomingHandler;
            _outgoingHandler = outgoingHandler;
            var outgoingChannel = Channel.CreateUnbounded<IEdgeHubOutgoingEvent>();
            _outgoingEventsReader = outgoingChannel.Reader;
            _outgoingEventsWriter = outgoingChannel.Writer;
            _client = client;
        }

        public async Task HandleAsync(IEdgeHubOutgoingEvent @event)
        {
            await _outgoingEventsWriter.WriteAsync(@event);
        }

        public async Task SetupBridge()
        {
            var incomingEventsTask = SetupIncomingEvents();
            var outgoingEventsTask = SetupOutgoingEvents();

            await Task.WhenAll(incomingEventsTask, outgoingEventsTask);
        }

        private async Task SetupIncomingEvents()
        {
            var allIncomingEventTasks = _incomingHandler.GetSubtypes()
                .Select(async type => await SetupIncomingEvent(type));

            await Task.WhenAll(allIncomingEventTasks);
        }

        private async Task SetupIncomingEvent(Type type)
        {
            var inputName = type.GetAttribute<InputNameAttribute>()?.InputName;
            if (inputName == null) throw new Exception($"Type '{type.Name}' is missing 'InputName' attribute.");

            await _client.SetInputMessageHandlerAsync(inputName, async (message, context) =>
            {
                try
                {
                    _logger.Information("Handling incoming event for input {InputName}", inputName);
                    var messageBytes = message.GetBytes();
                    var messageString = Encoding.UTF8.GetString(messageBytes);

                    var deserialized = (IEdgeHubIncomingEvent)JsonConvert.DeserializeObject(messageString, type);

                    _logger.Information("New incoming message: {IncomingMessage}", messageString);

                    await IncomingEvent(deserialized);

                    return MessageResponse.Completed;
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex.Message);
                    return MessageResponse.Abandoned;
                }
            }, null);
        }

        private async Task SetupOutgoingEvents()
        {
            var allOutgoingTypes = _outgoingHandler.GetSubtypes();

            var outputNameForType = allOutgoingTypes
                .ToDictionary(type => type, type => type.GetAttribute<OutputNameAttribute>()?.OutputName);

            if (outputNameForType.Where(type => type.Value == null).Any())
            {
                throw new Exception($"The following types are missing 'OutputName' attribute: {string.Join(",", outputNameForType.Where(type => type.Value == null).Select(type => type.Key.Name))}");
            }

            while (true)
            {
                var outgoingEvent = await _outgoingEventsReader.ReadAsync();
                if (outputNameForType.TryGetValue(outgoingEvent.GetType(), out var outputName))
                {
                    var outputString = JsonConvert.SerializeObject(outgoingEvent, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    var outputBytes = Encoding.UTF8.GetBytes(outputString);
                    var outputMessage = new Message(outputBytes);

                    await _client.SendEventAsync(outputName, outputMessage);
                }
            }
        }
    }
}
