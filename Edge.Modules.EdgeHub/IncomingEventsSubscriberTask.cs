using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Channels;
using Autofac;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using RaaLabs.Edge.Modules.EventHandling;
using Serilog;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    public class IncomingEventsSubscriberTask : IRunAsync
    {
        private readonly Channel<Type> _newSubscriptions;
        private readonly ChannelReader<Type> _newSubscriptionsReader;
        private readonly Channel<Task> _newSubscriber;
        private readonly ChannelReader<Task> _newSubscriberReader;
        private readonly ILifetimeScope _scope;
        private readonly IIotModuleClient _client;
        private readonly ILogger _logger;

        public IncomingEventsSubscriberTask(ILifetimeScope scope, IIotModuleClient client, ILogger logger)
        {
            _newSubscriptions = Channel.CreateUnbounded<Type>();
            _newSubscriptionsReader = _newSubscriptions.Reader;
            _newSubscriber = Channel.CreateUnbounded<Task>();
            _newSubscriberReader = _newSubscriber.Reader;

            _scope = scope;
            _client = client;
            _logger = logger;
        }

        public async Task Run()
        {
            var startSubscribersTask = StartNewSubscribersAsync();
            var allSubscriberTasks = new List<Task> { startSubscribersTask };

            while (true)
            {
                var waitingForSubscribersToFinishTask = Task.WhenAny(allSubscriberTasks);
                var newSubscribersAddedTask = _newSubscriberReader.ReadAllAsync().ToListAsync().AsTask();

                var finishedTask = await Task.WhenAny(waitingForSubscribersToFinishTask, newSubscribersAddedTask);

                if (finishedTask == newSubscribersAddedTask)
                {
                    allSubscriberTasks.AddRange(newSubscribersAddedTask.Result);
                }
            }
        }

        /// <summary>
        /// Register subscription to EdgeHub event
        /// </summary>
        /// <typeparam name="T">the event type to subscribe to</typeparam>
        /// <param name="eventHandler"></param>
        public void SetupSubscriptionForEventHandler<T>()
            where T : IEvent
        {
            _newSubscriptions.Writer.WriteAsync(typeof(T));
        }

        /// <summary>
        /// Deserialize incoming EdgeHub message and publish the event to the application.
        /// </summary>
        /// <typeparam name="T">The event type to deserialize the incoming data to</typeparam>
        /// <param name="eventHandler">The event handler for the given event type</param>
        /// <param name="message">The incoming EdgeHub message</param>
        /// <param name="logger">a logger</param>
        /// <returns></returns>
        static async Task<MessageResponse> HandleSubscriber<T>(EventHandling.EventHandler<T> eventHandler, Message message, ILogger logger)
            where T : IEvent
        {
            try
            {
                var messageBytes = message.GetBytes();
                var messageString = Encoding.UTF8.GetString(messageBytes);

                var deserialized = JsonConvert.DeserializeObject<T>(messageString);
                await eventHandler.ProduceAsync(deserialized);

                logger.Information("New incoming message: {IncomingMessage}", messageString);

                return await Task.FromResult(MessageResponse.Completed);
            }
            catch (Exception ex)
            {
                logger.Warning(ex.Message);
                return MessageResponse.Abandoned;
            }
        }

        private async Task StartNewSubscribersAsync()
        {
            while (true)
            {
                var subscriberType = await _newSubscriptionsReader.ReadAsync();
                var setupMethod = GetType().GetMethod("SetupSubscriber", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MakeGenericMethod(subscriberType);
                var subscriberTask = (Task)setupMethod.Invoke(this, new object[] { });
                _logger.Information($"subscription to '{subscriberType.Name}' set up!");
                await _newSubscriber.Writer.WriteAsync(subscriberTask);
            }
        }

        private Task SetupSubscriber<T>()
            where T : IEvent
        {
            var eventHandler = _scope.Resolve<EventHandling.EventHandler<T>>();
            var inputName = ((InputNameAttribute)typeof(T).GetCustomAttributes(typeof(InputNameAttribute), true).First()).InputName;

            return _client.SetInputMessageHandlerAsync(inputName, async (message, context) =>
            {
                _logger.Information("Handling incoming event for input {InputName}", inputName);
                return await HandleSubscriber(eventHandler, message, _logger);
            }, null);
        }
    }
}
