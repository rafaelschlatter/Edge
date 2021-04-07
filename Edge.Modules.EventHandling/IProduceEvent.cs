using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling
{
    public interface IProduceEvent { }

    /// <summary>
    /// A class implementing this interface will produce an event of the given type.
    /// 
    /// The Event Handling module requires all classes implementing this interface to have a delegate function with
    /// type signature "public event EventEmitter<T>" associated with the event, and will throw an exception during
    /// application booting if it is not present.
    /// 
    /// </summary>
    /// <typeparam name="T">the type to consume</typeparam>
    public interface IProduceEvent<T> : IProduceEvent
    {
    }

    /// <summary>
    /// A delegate function connected to the "Produce(...)" function of the Event Handler for the specified data type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="event"></param>
    public delegate void EventEmitter<in T>(T @event) where T: IEvent;
}
