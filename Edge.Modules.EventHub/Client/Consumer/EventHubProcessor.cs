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

namespace RaaLabs.Edge.Modules.EventHub.Client.Consumer
{
    class EventHubProcessor : AzureBlobStorageEventProcessor<EventProcessorPartition>
    {
        private readonly EventDataReceived ReceivedEventData;
        public EventHubProcessor(EventDataReceived receivedEventData, int eventBatchMaximumCount, string consumerGroup, string fullyQualifiedNamespace, string eventHubName, BlobContainerClient storageContainer, EventProcessorOptions options = null) : base(eventBatchMaximumCount, consumerGroup, fullyQualifiedNamespace, eventHubName, storageContainer, options)
        {
            ReceivedEventData = receivedEventData;
        }

        public static async Task<EventHubProcessor> FromEventHubConnection(IEventHubConnection connection, EventDataReceived eventDataReceived)
        {
            var options = connection.ReaderOptions;
            var storageClient = new BlobContainerClient(connection.BlobStorageConnectionString, connection.BlobStorageContainerName);

            var exists = storageClient.Exists();
            if (exists && connection.DeleteCheckpointStoreAtStartup)
            {
                await storageClient.DeleteIfExistsAsync();
                await Task.Delay(60000);
            }
            await storageClient.CreateIfNotExistsAsync();

            var processor = new EventHubProcessor(
                eventDataReceived,
                connection.MaxIncomingBatchCount,
                connection.ConsumerGroup,
                connection.ConnectionString,
                connection.EventHubName,
                storageClient,
                options);

            return processor;

        }

        public static async Task<EventHubProcessor> FromEventHubName(string eventHubName, EventDataReceived eventDataReceived)
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
                await storageClient.DeleteIfExistsAsync();
                await Task.Delay(60000);
            }
            await storageClient.CreateIfNotExistsAsync();

            var processor = new EventHubProcessor(
                eventDataReceived,
                eventBatchMaximumCount, 
                consumerGroup,
                eventHubConnectionString,
                eventHubName,
                storageClient,
                options);

            return processor;
        }

        protected override async Task OnProcessingEventBatchAsync(IEnumerable<EventData> events, EventProcessorPartition partition, CancellationToken cancellationToken)
        {
            try 
            {
                foreach (var @event in events)
                {
                    await ReceivedEventData(@event);                    
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
                    _ = Encoding.UTF8.GetString(eventArgs.EventBody.ToArray());
                }
                await OnProcessingErrorAsync(exception, partition, "testEvent", cancellationToken);
            }
        }

        protected override async Task OnProcessingErrorAsync(Exception exception, EventProcessorPartition partition, string operationDescription, CancellationToken cancellationToken)
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

        public delegate Task EventDataReceived(EventData data);
    }
}
