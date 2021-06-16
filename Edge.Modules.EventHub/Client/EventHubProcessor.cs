/*---------------------------------------------------------------------------------------------
 *  Copyright (c) RaaLabs. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Primitives;
using Newtonsoft.Json;
using Serilog;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Modules.EventHub.Client
{
    class EventHubProcessor<T> : AzureBlobStorageEventProcessor<EventProcessorPartition>
        where T : IEvent
    {
        private readonly MessageReceived ReceivedMessage;
        public EventHubProcessor(MessageReceived messageReceived, int eventBatchMaximumCount, string consumerGroup, string fullyQualifiedNamespace, string eventHubName, BlobContainerClient storageContainer, EventProcessorOptions options = null) : base(eventBatchMaximumCount, consumerGroup, fullyQualifiedNamespace, eventHubName, storageContainer, options)
        {
            ReceivedMessage = messageReceived;
        }

        public static EventHubProcessor<T> FromEventHubName(string eventHubName, MessageReceived messageReceived)
        {
            var environmentVariablePrefix = eventHubName.ToUpper().Replace("-","");
            var eventHubConnectionString = Environment.GetEnvironmentVariable(environmentVariablePrefix + "_CONNECTION_STRING");
            var consumerGroup = Environment.GetEnvironmentVariable("CONSUMER_GROUP");
            var eventBatchMaximumCount = int.Parse(Environment.GetEnvironmentVariable("EVENT_BATCH_MAX_COUNT") ?? "400");
            var options = new EventProcessorOptions
            {
                DefaultStartingPosition = EventPosition.FromEnqueuedTime(DateTimeOffset.UtcNow),
                PrefetchCount = 800,
                MaximumWaitTime = TimeSpan.FromSeconds(120)
            };
            var blobStorageConnectionString = Environment.GetEnvironmentVariable(environmentVariablePrefix + "_BLOB_STORAGE_CONNECTION_STRING");
            var blobContainerName = Environment.GetEnvironmentVariable(environmentVariablePrefix + "_BLOB_CONTAINER_NAME");
            var storageClient = new BlobContainerClient(blobStorageConnectionString,
                                                    blobContainerName);
            var exists = storageClient.Exists();
            if (exists)
            {
                storageClient.DeleteIfExists();
                Thread.Sleep(60000);
            }
            storageClient.CreateIfNotExists();

            var processor = new EventHubProcessor<T>(
                messageReceived,
                eventBatchMaximumCount, 
                consumerGroup,
                eventHubConnectionString,
                eventHubName,
                storageClient,
                options);

            return processor;
        }

        protected async override Task OnProcessingEventBatchAsync(IEnumerable<EventData> events, EventProcessorPartition partition, CancellationToken cancellationToken)
        {
            try 
            {
                foreach (var eventArgs in events)
                {
                    string data = Encoding.UTF8.GetString(eventArgs.EventBody.ToArray());
                    var payload = JsonConvert.DeserializeObject<T>(data);

                    await ReceivedMessage(payload);
                    
                }
                if (events.Any())
                {
                    await CheckpointAsync(partition, events.Last(), cancellationToken);
                }

            }
            catch (Exception exception)
            {   
                // Catch and ignore, we should not allow exceptions to bubble out of this method.
                foreach (var eventArgs in events)
                {
                    var data = Encoding.UTF8.GetString(eventArgs.EventBody.ToArray());
                }
                await OnProcessingErrorAsync(exception, partition, "testEvent", cancellationToken);
            }
        }

        protected async override Task OnProcessingErrorAsync(Exception exception, EventProcessorPartition partition, string operationDescription, CancellationToken cancellationToken)
        {
            try 
            {
                if (partition != null)
                {
                    Console.Error.WriteLine($"Exception on partition {partition.PartitionId} while performing {operationDescription}: {exception.Message}");
                }
                else
                {
                    Console.Error.WriteLine($"Exception while performing {operationDescription}: {exception.Message}");
                }        
            }
            catch
            {
                // Catch and ignore, we should not allow exceptions to bubble out of this method.
            }
            await Task.CompletedTask;
        }

        public delegate Task MessageReceived(T message);
            
    }
}