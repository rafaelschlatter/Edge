using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// An event handler for the given event type T. This is responsible for the "plumbing" between event producers
    /// and event consumers. It exposes a function "Produce(...)", which will be called by all event producers.
    /// 
    /// On a new incoming event, the EventHandler class will iterate through all the consumers for its event type T,
    /// calling the "Handle(T @event)"/"HandleAsync(T @event)" function of all these classes.
    /// 
    /// For normal development, this class can be ignored by the developer.
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class EventHandler<T> : IEventHandler
        where T: IEvent
    {
        private readonly List<IConsumeEvent<T>> _observers;
        private readonly List<IConsumeEventAsync<T>> _asyncObservers;
        private readonly List<Action<T>> _observerFunctions;
        private readonly List<Func<T, Task>> _asyncObserverFunctions;

        public EventHandler()
        {
            _observers = new List<IConsumeEvent<T>>();
            _asyncObservers = new List<IConsumeEventAsync<T>>();
            _observerFunctions = new List<Action<T>>();
            _asyncObserverFunctions = new List<Func<T, Task>>();
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
        /// Called by the subsciber class to start subscribing to event
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IConsumeEventAsync<T> observer)
        {
            if (!_asyncObservers.Contains(observer))
            {
                _asyncObservers.Add(observer);
            }

            return new Unsubscriber<T>(_asyncObservers, observer);
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
        /// Called by the subsciber function to start subscribing to event.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(Func<T, Task> observerFunction)
        {
            if (!_asyncObserverFunctions.Contains(observerFunction))
            {
                _asyncObserverFunctions.Add(observerFunction);
            }

            return new UnsubscriberFunction<T>(_asyncObserverFunctions, observerFunction);
        }

        /// <summary>
        /// Called by the event emitter to produce a new event.
        /// </summary>
        /// <param name="event"></param>
        public void Produce(T @event)
        {
            _observers.ForEach(_ => _.Handle(@event));
            _observerFunctions.ForEach(handlerFunction => handlerFunction(@event));
            Task.WhenAll(_asyncObservers.Select(async _ => await _.HandleAsync(@event))).Wait();
            Task.WhenAll(_asyncObserverFunctions.Select(async handlerFunction => await handlerFunction(@event))).Wait();
        }

        /// <summary>
        /// Called by the event emitter to produce a new async event.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task ProduceAsync(T @event)
        {
            _observers.ForEach(_ => _.Handle(@event));
            _observerFunctions.ForEach(handlerFunction => handlerFunction(@event));
            var asyncObservers = Task.WhenAll(_asyncObservers.Select(async _ => await _.HandleAsync(@event)));
            var asyncFunctions = Task.WhenAll(_asyncObserverFunctions.Select(async handlerFunction => await handlerFunction(@event)));

            await Task.WhenAll(asyncObservers, asyncFunctions);
        }
    }

    /// <summary>
    /// Remove consumer class from list of subscribers when it is deleted
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class Unsubscriber<T> : IDisposable
    {
        private readonly List<IConsumeEvent<T>> _observers;
        private readonly List<IConsumeEventAsync<T>> _asyncObservers;
        private readonly IConsumeEvent _observer;

        internal Unsubscriber(List<IConsumeEvent<T>> observers, IConsumeEvent<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        internal Unsubscriber(List<IConsumeEventAsync<T>> observers, IConsumeEventAsync<T> observer)
        {
            _asyncObservers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer is IConsumeEvent<T> && (_observers?.Contains((IConsumeEvent<T>)_observer) ?? false))
            {
                _observers.Remove((IConsumeEvent<T>)_observer);
            }

            if (_observer is IConsumeEventAsync<T> && (_asyncObservers?.Contains((IConsumeEventAsync<T>)_observer) ?? false))
            {
                _asyncObservers.Remove((IConsumeEventAsync<T>)_observer);
            }
        }
    }

    /// <summary>
    /// Remove consumer function from list of subscribers when it is deleted
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class UnsubscriberFunction<T> : IDisposable
    {
        private readonly List<Action<T>> _observerFunctions;
        private readonly List<Func<T, Task>> _asyncObserverFunctions;
        private readonly Action<T> _observerFunction;
        private readonly Func<T, Task> _asyncObserverFunction;
        internal UnsubscriberFunction(List<Action<T>> observerFunctions, Action<T> observerFunction)
        {
            _observerFunctions = observerFunctions;
            _observerFunction = observerFunction;
        }

        internal UnsubscriberFunction(List<Func<T, Task>> observerFunctions, Func<T, Task> observerFunction)
        {
            _asyncObserverFunctions = observerFunctions;
            _asyncObserverFunction = observerFunction;
        }

        public void Dispose()
        {
            if (_observerFunctions?.Contains(_observerFunction) ?? false)
            {
                _observerFunctions.Remove(_observerFunction);
            }
            if (_asyncObserverFunctions?.Contains(_asyncObserverFunction) ?? false)
            {
                _asyncObserverFunctions.Remove(_asyncObserverFunction);
            }
        }
    }

    public interface IEventHandler { }

}
