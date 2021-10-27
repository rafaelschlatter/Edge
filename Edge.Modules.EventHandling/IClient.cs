using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public interface IReceiverClient<ConnectionType, DataType> : IReceiverClient<DataType>, IClient<ConnectionType>
        where ConnectionType : IClientConnection
    {
    }

    public interface IReceiverClient<DataType> : IClient
    {
        public event DataReceivedDelegate<DataType> OnDataReceived;
    }

    public interface ISubscribingReceiverClient<DataType, TopicType> : IReceiverClient<DataType>
    {
        public Task Subscribe(TopicType topic);
    }

    public interface ISubscribingReceiverClient<ConnectionType, DataType, TopicType> : ISubscribingReceiverClient<DataType, TopicType>, IReceiverClient<ConnectionType, DataType>
        where ConnectionType : IClientConnection
    {
    }

    public delegate Task DataReceivedDelegate<DataType>(Type connectionType, DataType data);

    public interface ISenderClient<ConnectionType, DataType> : ISenderClient<DataType>, IClient<ConnectionType>
        where ConnectionType : IClientConnection
    {
    }

    public interface ISenderClient<DataType> : IClient
    {
        public Task SendAsync(DataType data);
    }

    public interface IBatchedSenderClient<ConnectionType, DataType> : IBatchedSenderClient<DataType>, IClient<ConnectionType>
        where ConnectionType : IClientConnection
    {
    }

    public interface IBatchedSenderClient<DataType> : IClient
    {
        public Task SendBatchAsync(IEnumerable<DataType> data);
    }


    public interface IClientConnection { }
}
