using System;
using System.Collections.Generic;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// An event handler for the given event type T. This is responsible for the "plumbing" between event producers
    /// and event consumers. It exposes a function "Produce(...)", which will be called by all event producers.
    /// 
    /// On a new incoming event, the EventHandler class will iterate through all the consumers for its event type T,
    /// calling the "Handle(T @event)" function of all these classes.
    /// 
    /// For normal development, this class can be ignored by the developer.
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class EventHandler<T> : IEventHandler
        where T: IEvent
    {
        private List<IConsumeEvent<T>> _observers;
        private List<Action<T>> _observerFunctions;
        public EventHandler()
        {
            _observers = new List<IConsumeEvent<T>>();
            _observerFunctions = new List<Action<T>>();
        }

        /// <summary>
        /// Called by the subsciber class to start subscribing to event
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IConsumeEvent<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber<T>(_observers, observer);
        }

        /// <summary>
        /// Called by the subsciber function to start subscribing to event.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(Action<T> observerFunction)
        {
            if (!_observerFunctions.Contains(observerFunction))
            {
                _observerFunctions.Add(observerFunction);
            }

            return new UnsubscriberFunction<T>(_observerFunctions, observerFunction);
        }

        /// <summary>
        /// Called by the event emitter to produce a new event.
        /// </summary>
        /// <param name="event"></param>
        public void Produce(T @event)
        {
            _observers.ForEach(_ => _.Handle(@event));
            _observerFunctions.ForEach(handlerFunction => handlerFunction(@event));
        }
    }

    /// <summary>
    /// Remove consumer class from list of subscribers when it is deleted
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class Unsubscriber<T> : IDisposable
    {
        private List<IConsumeEvent<T>> _observers;
        private IConsumeEvent<T> _observer;
        internal Unsubscriber(List<IConsumeEvent<T>> observers, IConsumeEvent<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }

    /// <summary>
    /// Remove consumer function from list of subscribers when it is deleted
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class UnsubscriberFunction<T> : IDisposable
    {
        private List<Action<T>> _observerFunctions;
        private Action<T> _observerFunction;
        internal UnsubscriberFunction(List<Action<T>> observerFunctions, Action<T> observerFunction)
        {
            _observerFunctions = observerFunctions;
            _observerFunction = observerFunction;
        }

        public void Dispose()
        {
            if (_observerFunctions.Contains(_observerFunction))
            {
                _observerFunctions.Remove(_observerFunction);
            }
        }
    }

    public interface IEventHandler { }

}
