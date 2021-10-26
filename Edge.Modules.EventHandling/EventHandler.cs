using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Autofac;
using static RaaLabs.Edge.Application;

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
    public class EventHandler<T> : IEventHandler<T>, IEventPropagator<T>
        where T: IEvent
    {
        private readonly IList<IConsumeEvent<T>> _observers;
        private readonly IList<IConsumeEventAsync<T>> _asyncObservers;
        private readonly IList<Action<T>> _observerFunctions;
        private readonly IList<Func<T, Task>> _asyncObserverFunctions;
        private readonly IList<IEventPropagator<T>> _supertypeHandlers;
        private readonly IDictionary<Type, Action<T, ISet<object>>> _subtypeHandlers;
        private readonly IDictionary<Type, Func<T, ISet<object>, Task>> _asyncSubtypeHandlers;
        private readonly ILogger _logger;

        public EventHandler(ILifetimeScope scope, ILogger logger)
        {
            _logger = logger;
            _observers = new List<IConsumeEvent<T>>();
            _asyncObservers = new List<IConsumeEventAsync<T>>();
            _observerFunctions = new List<Action<T>>();
            _asyncObserverFunctions = new List<Func<T, Task>>();
            _subtypeHandlers = new Dictionary<Type, Action<T, ISet<object>>>();
            _asyncSubtypeHandlers = new Dictionary<Type, Func<T, ISet<object>, Task>>();

            // If T is an event implementing ISomeEvent, and ISomeEvent implements IEvent,
            // we don't want IEvent to be a supertype of T, but rather of ISomeEvent, because this would
            // cause IEvent to be triggered twice for this event.
            // GetImmediateInterfaces would in this case return only ISomeEvent, and not IEvent.
            var superTypes = typeof(T).GetImmediateInterfaces();

            _supertypeHandlers = superTypes.Select(iface =>
            {
                var initializeSupertypeEventHandlerMethod = GetType().GetMethod("InitializeSupertypeEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).MakeGenericMethod(typeof(T), iface);
                return (IEventPropagator<T>)initializeSupertypeEventHandlerMethod.Invoke(null, new object[] { scope, this });
            })
            .ToList();
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
            var visitedHandlers = new HashSet<object>();
            PropagateEvent(@event, visitedHandlers);
        }

        /// <summary>
        /// Called by the event emitter to produce a new async event.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task ProduceAsync(T @event)
        {
            var visitedHandlers = new HashSet<object>();
            await PropagateEventAsync(@event, visitedHandlers);
        }

        public void PropagateEvent(T @event, ISet<object> visitedHandlers)
        {
            if (visitedHandlers.Contains(this)) return;
            else visitedHandlers.Add(this);

            var eventType = @event.GetType();

            foreach (var observer in _observers)
            {
                observer.Handle(@event);
            }
            foreach (var observerFunction in _observerFunctions)
            {
                observerFunction(@event);
            }

            Task.WhenAll(_asyncObservers.Select(async _ => await _.HandleAsync(@event))).Wait();
            Task.WhenAll(_asyncObserverFunctions.Select(async handlerFunction => await handlerFunction(@event))).Wait();
        
            foreach (var supertypeHandler in _supertypeHandlers)
            {
                supertypeHandler.PropagateEvent(@event, visitedHandlers);
            }

            var subtypeFunc = _subtypeHandlers.ContainsKey(eventType) ? _subtypeHandlers[eventType] : (_, _) => { };
            subtypeFunc(@event, visitedHandlers);
        }

        public async Task PropagateEventAsync(T @event, ISet<object> visitedHandlers)
        {
            if (visitedHandlers.Contains(this)) return;
            else visitedHandlers.Add(this);

            var eventType = @event.GetType();

            foreach (var observer in _observers)
            {
                observer.Handle(@event);
            }
            foreach (var observerFunction in _observerFunctions)
            {
                observerFunction(@event);
            }
            var asyncObservers = Task.WhenAll(_asyncObservers.Select(async _ => await _.HandleAsync(@event)));
            var asyncFunctions = Task.WhenAll(_asyncObserverFunctions.Select(async handlerFunction => await handlerFunction(@event)));
            var asyncSupertypeHandlers = Task.WhenAll(_supertypeHandlers.Select(async handler => await handler.PropagateEventAsync(@event, visitedHandlers)));

            var asyncSubtypeFunc = _asyncSubtypeHandlers.ContainsKey(eventType) ? _asyncSubtypeHandlers[eventType] : (_, _) => Task.CompletedTask;
            var asyncSubtypeTask = asyncSubtypeFunc(@event, visitedHandlers);

            await Task.WhenAll(asyncObservers, asyncFunctions, asyncSupertypeHandlers, asyncSubtypeTask);
        }

        public ISet<Type> GetSubtypes()
        {
            return _subtypeHandlers.Keys.ToHashSet();
        }

        /// <summary>
        /// Initialize supertype EventHandler for this EventHandler.
        /// 
        /// IMPORTANT: Static code analysis will claim that this function is never called, but it will be invoked from the constructor in runtime using reflection.
        /// </summary>
        /// <typeparam name="Ty">This type</typeparam>
        /// <typeparam name="IFace">The supertype to initialize</typeparam>
        /// <param name="scope">The current scope</param>
        /// <param name="child">The current EventHandler</param>
        /// <returns></returns>
        private static IEventPropagator<IFace> InitializeSupertypeEventHandler<Ty, IFace>(ILifetimeScope scope, IEventPropagator<Ty> child)
            where Ty : IFace
            where IFace : IEvent
        {
            var parent = scope.Resolve<EventHandler<IFace>>();
            Action<IFace, ISet<object>> childPropagationFunction = (@event, visitedHandlers) => child.PropagateEventAsync((Ty)@event, visitedHandlers);
            Func<IFace, ISet<object>, Task> childPropagationFunctionAsync = (@event, visitedHandlers) => child.PropagateEventAsync((Ty)@event, visitedHandlers);
            parent.RegisterSubtypeHandler(typeof(Ty), childPropagationFunction);
            parent.RegisterAsyncSubtypeHandler(typeof(Ty), childPropagationFunctionAsync);

            return parent;
        }

        protected void RegisterSubtypeHandler(Type type, Action<T, ISet<object>> handler)
        {
            _subtypeHandlers.Add(type, handler);
        }

        protected void RegisterAsyncSubtypeHandler(Type type, Func<T, ISet<object>, Task> handler)
        {
            _asyncSubtypeHandlers.Add(type, handler);
        }
    }

    /// <summary>
    /// Remove consumer class from list of subscribers when it is deleted
    /// </summary>
    /// <typeparam name="T">the event type</typeparam>
    public class Unsubscriber<T> : IDisposable
    {
        private readonly IList<IConsumeEvent<T>> _observers;
        private readonly IList<IConsumeEventAsync<T>> _asyncObservers;
        private readonly IConsumeEvent _observer;

        internal Unsubscriber(IList<IConsumeEvent<T>> observers, IConsumeEvent<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        internal Unsubscriber(IList<IConsumeEventAsync<T>> observers, IConsumeEventAsync<T> observer)
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
        private readonly IList<Action<T>> _observerFunctions;
        private readonly IList<Func<T, Task>> _asyncObserverFunctions;
        private readonly Action<T> _observerFunction;
        private readonly Func<T, Task> _asyncObserverFunction;
        internal UnsubscriberFunction(IList<Action<T>> observerFunctions, Action<T> observerFunction)
        {
            _observerFunctions = observerFunctions;
            _observerFunction = observerFunction;
        }

        internal UnsubscriberFunction(IList<Func<T, Task>> asyncObserverFunctions, Func<T, Task> asyncObserverFunction)
        {
            _asyncObserverFunctions = asyncObserverFunctions;
            _asyncObserverFunction = asyncObserverFunction;
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

    public interface IEventHandler
    {
        public ISet<Type> GetSubtypes();
    }

    public interface IEventHandler<T> : IEventHandler
        where T : IEvent
    {
        public IDisposable Subscribe(IConsumeEvent<T> observer);
        public IDisposable Subscribe(IConsumeEventAsync<T> observer);
        public IDisposable Subscribe(Action<T> observerFunction);
        public IDisposable Subscribe(Func<T, Task> observerFunction);

        public void Produce(T @event);
        public Task ProduceAsync(T @event);

    }

    public interface IEventPropagator<in T>
        where T : IEvent
    {
        public void PropagateEvent(T @event, ISet<object> visitedHandlers);
        public Task PropagateEventAsync(T @event, ISet<object> visitedHandlers);
    }
}
