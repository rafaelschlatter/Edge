/*---------------------------------------------------------------------------------------------
 *  Copyright (c) RaaLabs. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *  Example of how to implement EventProcessor<TPartition> is from Matt Ellis as found in
 *  https://devblogs.microsoft.com/azure-sdk/custom-event-processor/
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Globalization;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Primitives;


namespace RaaLabs.Edge.Modules.EventHub.Client
{
    public abstract class AzureBlobStorageEventProcessor<TPartition> : EventProcessor<TPartition> where TPartition : EventProcessorPartition, new()
    {
        private BlobContainerClient StorageContainer { get; }

        protected AzureBlobStorageEventProcessor(int eventBatchMaximumCount, string consumerGroup, string connectionString, BlobContainerClient storageContainer, EventProcessorOptions options = null)
            : base(eventBatchMaximumCount, consumerGroup, connectionString, options)
        {
            StorageContainer = storageContainer;
        }

        protected AzureBlobStorageEventProcessor(int eventBatchMaximumCount, string consumerGroup, string connectionString, string eventHubName, BlobContainerClient storageContainer, EventProcessorOptions options = null)
            : base(eventBatchMaximumCount, consumerGroup, connectionString, eventHubName, options)
        {
            StorageContainer = storageContainer;
        }


        private const string OwnershipPrefixFormat = "{0}/{1}/{2}/ownership/";
        private const string OwnerIdentifierMetadataKey = "ownerid";

        protected override async Task<IEnumerable<EventProcessorPartitionOwnership>> ListOwnershipAsync(CancellationToken cancellationToken = default)
        {
            List<EventProcessorPartitionOwnership> partitonOwnerships = new List<EventProcessorPartitionOwnership>();
            string ownershipBlobsPefix = string.Format(OwnershipPrefixFormat, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant());

            await foreach (BlobItem blob in StorageContainer.GetBlobsAsync(traits: BlobTraits.Metadata, prefix: ownershipBlobsPefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                partitonOwnerships.Add(new EventProcessorPartitionOwnership()
                {
                    ConsumerGroup = ConsumerGroup,
                    EventHubName = EventHubName,
                    FullyQualifiedNamespace = FullyQualifiedNamespace,
                    LastModifiedTime = blob.Properties.LastModified.GetValueOrDefault(),
                    OwnerIdentifier = blob.Metadata[OwnerIdentifierMetadataKey],
                    PartitionId = blob.Name.Substring(ownershipBlobsPefix.Length),
                    Version = blob.Properties.ETag.ToString()
                }); ;
            }

            return partitonOwnerships;
        }
        protected override async Task<IEnumerable<EventProcessorPartitionOwnership>> ClaimOwnershipAsync(IEnumerable<EventProcessorPartitionOwnership> desiredOwnership, CancellationToken cancellationToken = default)
        {
            List<EventProcessorPartitionOwnership> claimedOwnerships = new List<EventProcessorPartitionOwnership>();

            foreach (EventProcessorPartitionOwnership ownership in desiredOwnership)
            {
                Dictionary<string, string> ownershipMetadata = new Dictionary<string, string>()
                {
                    { OwnerIdentifierMetadataKey, ownership.OwnerIdentifier },
                };

                // Construct the path to the blob and get a blob client for it so we can interact with it.
                string ownershipBlob = string.Format(OwnershipPrefixFormat + ownership.PartitionId, ownership.FullyQualifiedNamespace.ToLowerInvariant(), ownership.EventHubName.ToLowerInvariant(), ownership.ConsumerGroup.ToLowerInvariant());
                BlobClient ownershipBlobClient = StorageContainer.GetBlobClient(ownershipBlob);

                try
                {
                    if (ownership.Version == null)
                    {
                        // In this case, we are trying to claim ownership of a partition which was previously unowned, and hence did not have an ownership file. To ensure only a single host grabs the partition, 
                        // we use a conditional request so that we only create our blob in the case where it does not yet exist.
                        BlobRequestConditions requestConditions = new BlobRequestConditions() { IfNoneMatch = ETag.All };

                        using MemoryStream emptyStream = new MemoryStream(Array.Empty<byte>());
                        BlobContentInfo info = await ownershipBlobClient.UploadAsync(emptyStream, metadata: ownershipMetadata, conditions: requestConditions, cancellationToken: cancellationToken).ConfigureAwait(false);

                        claimedOwnerships.Add(new EventProcessorPartitionOwnership()
                        {
                            ConsumerGroup = ownership.ConsumerGroup,
                            EventHubName = ownership.EventHubName,
                            FullyQualifiedNamespace = ownership.FullyQualifiedNamespace,
                            LastModifiedTime = info.LastModified,
                            OwnerIdentifier = ownership.OwnerIdentifier,
                            PartitionId = ownership.PartitionId,
                            Version = info.ETag.ToString()
                        });
                    }
                    else
                    {
                        // In this case, the partition is owned by some other host. The ownership file already exists, so we just need to change metadata on it. But we should only do this if the metadata has not
                        // changed between when we listed ownership and when we are trying to claim ownership, i.e. the ETag for the file has not changed.               
                        BlobRequestConditions requestConditions = new BlobRequestConditions() { IfMatch = new ETag(ownership.Version) };
                        BlobInfo info = await ownershipBlobClient.SetMetadataAsync(ownershipMetadata, requestConditions, cancellationToken).ConfigureAwait(false);

                        claimedOwnerships.Add(new EventProcessorPartitionOwnership()
                        {
                            ConsumerGroup = ownership.ConsumerGroup,
                            EventHubName = ownership.EventHubName,
                            FullyQualifiedNamespace = ownership.FullyQualifiedNamespace,
                            LastModifiedTime = info.LastModified,
                            OwnerIdentifier = ownership.OwnerIdentifier,
                            PartitionId = ownership.PartitionId,
                            Version = info.ETag.ToString()
                        });
                    }
                }
                catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobAlreadyExists || e.ErrorCode == BlobErrorCode.ConditionNotMet)
                {
                    // In this case, another host has claimed the partition before we did. That's safe to ignore. We'll still try to claim other partitions.
                }
            }

            return claimedOwnerships;
        }

        private const string CheckpointPrefixFormat = "{0}/{1}/{2}/checkpoint/";
        private const string OffsetMetadataKey = "offset";

        protected override async Task<IEnumerable<EventProcessorCheckpoint>> ListCheckpointsAsync(CancellationToken cancellationToken = default)
        {
            List<EventProcessorCheckpoint> checkpoints = new List<EventProcessorCheckpoint>();
            string checkpointBlobsPrefix = string.Format(CheckpointPrefixFormat, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant());

            await foreach (BlobItem item in StorageContainer.GetBlobsAsync(traits: BlobTraits.Metadata, prefix: checkpointBlobsPrefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                if (long.TryParse(item.Metadata[OffsetMetadataKey], NumberStyles.Integer, CultureInfo.InvariantCulture, out long offset))
                {
                    checkpoints.Add(new EventProcessorCheckpoint()
                    {
                        ConsumerGroup = ConsumerGroup,
                        EventHubName = EventHubName,
                        FullyQualifiedNamespace = FullyQualifiedNamespace,
                        PartitionId = item.Name.Substring(checkpointBlobsPrefix.Length),
                        StartingPosition = EventPosition.FromOffset(offset, isInclusive: false)
                    });
                }
            }

            return checkpoints;
        }

        private const string CheckpointBlobNameFormat = "{0}/{1}/{2}/checkpoint/{3}";

        protected override async Task<EventProcessorCheckpoint> GetCheckpointAsync(string partitionId, CancellationToken cancellationToken)
        {
            string checkpointName = string.Format(CheckpointBlobNameFormat, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant(), partitionId);

            try
            {
                BlobProperties properties = await StorageContainer.GetBlobClient(checkpointName).GetPropertiesAsync().ConfigureAwait(false);

                if (long.TryParse(properties.Metadata[OffsetMetadataKey], NumberStyles.Integer, CultureInfo.InvariantCulture, out long offset))
                {
                    return new EventProcessorCheckpoint()
                    {
                        ConsumerGroup = ConsumerGroup,
                        EventHubName = EventHubName,
                        FullyQualifiedNamespace = FullyQualifiedNamespace,
                        PartitionId = partitionId,
                        StartingPosition = EventPosition.FromOffset(offset, isInclusive: false)
                    };
                }
            }
            catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // There's no checkpoint for this partition partiton yet, but that's okay, so we ignore this exception.
            }

            return null;
        }

        protected async Task CheckpointAsync(TPartition partition, EventData data, CancellationToken cancellationToken = default)
        {
            string checkpointBlob = string.Format(CheckpointPrefixFormat + partition.PartitionId, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant());
            Dictionary<string, string> checkpointMetadata = new Dictionary<string, string>()
            {
                { OffsetMetadataKey, data.Offset.ToString(CultureInfo.InvariantCulture) },
            };

            using MemoryStream emptyStream = new MemoryStream(Array.Empty<byte>());
            await StorageContainer.GetBlobClient(checkpointBlob).UploadAsync(emptyStream, metadata: checkpointMetadata, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
    
}
