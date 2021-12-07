using Microsoft.Azure.Devices.Client;
using RaaLabs.Edge.Modules.EventHandling;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EdgeHub
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIotModuleClient : ISubscribingReceiverClient<(string inputName, Message message), string>, ISenderClient<(string outputName, Message message)>
    {
    }
}
