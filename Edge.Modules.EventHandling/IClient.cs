using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling
{
    public interface IClient
    {
        public Task Connect();
    }

    public interface IClient<ConnectionType> : IClient
        where ConnectionType : IClientConnection
    {
    }

    public interface IReceiverClient<DataType> : IClient
    {
        public event DataReceivedDelegate<DataType> OnDataReceived;
    }

    public delegate Task DataReceivedDelegate<in DataType>(Type connectionType, DataType data);

    public interface IReceiverClient<ConnectionType, DataType> : IReceiverClient<DataType>, IClient<ConnectionType>
        where ConnectionType : IClientConnection
    {
    }

    public interface ISubscribingReceiverClient<DataType, TopicType> : IReceiverClient<DataType>
    {
        public Task Subscribe(TopicType topic);
    }

    public interface ISubscribingReceiverClient<ConnectionType, DataType, TopicType> : ISubscribingReceiverClient<DataType, TopicType>, IReceiverClient<ConnectionType, DataType>
        where ConnectionType : IClientConnection
    {
    }


    public interface ISenderClient<in DataType> : IClient
    {
        public Task SendAsync(DataType data);
    }

    public interface ISenderClient<ConnectionType, DataType> : ISenderClient<DataType>, IClient<ConnectionType>
        where ConnectionType : IClientConnection
    {
    }

    public interface IBatchedSenderClient<in DataType> : IClient
    {
        public Task SendBatchAsync(IEnumerable<DataType> data);
    }

    public interface IBatchedSenderClient<ConnectionType, DataType> : IBatchedSenderClient<DataType>, IClient<ConnectionType>
        where ConnectionType : IClientConnection
    {
    }

    public interface IClientConnection { }
}
