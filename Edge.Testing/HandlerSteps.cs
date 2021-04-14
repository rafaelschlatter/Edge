using BoDi;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;
using FluentAssertions;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A class defining steps for testing a handler
    /// </summary>
    [Binding]
    public sealed class HandlerSteps
    {
        private readonly IObjectContainer _container;
        private readonly TypeMapping _typeMapping;

        private IConsumeEvent _handler;
        private Dictionary<Type, List<IEvent>> _emittedEvents;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typeMapping"></param>
        public HandlerSteps(IObjectContainer container, TypeMapping typeMapping)
        {
            _container = container;
            _typeMapping = typeMapping;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typename">The name of the handler type. This is the name of the type in the TypeMapping class</param>
        [Given(@"a handler of type (.*)")]
        public void GivenAHandlerOfType(string typename)
        {
            var type = _typeMapping[typename];
            _handler = (IConsumeEvent) _container.Resolve(type);
            _emittedEvents = new Dictionary<Type, List<IEvent>>();

            var emitters = type.GetEvents()
                .Where(i => i.EventHandlerType.IsGenericType)
                .Where(i => i.EventHandlerType.GetGenericTypeDefinition() == typeof(EventEmitter<>))
                .ToList();

            emitters.ForEach(emitter => SetupEventListeningForEmitter(emitter, _handler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typename">The name of the event type. This is the name of the type in the TypeMapping class</param>
        /// <param name="table">A table containing the parameters for constructing the events. An EventInstanceFactory must be implemented for this type</param>
        [When(@"the following events of type (.*) is produced")]
        public void WhenTheFollowingEventsOfTypeIsProduced(string typename, Table table)
        {
            var eventType = _typeMapping[typename];
            GetType().GetMethod("HandleEventsFromTable").MakeGenericMethod(eventType).Invoke(this, new object[] { table });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typename">The name of the event type. This is the name of the type in the TypeMapping class</param>
        /// <param name="table">A table containing parameters for verifying the produced event. An ProducedEventVerifier must be implemented for this type</param>
        [Then(@"the following events of type (.*) is produced")]
        public void ThenTheFollowingEventsOfTypeIsProduced(string typename, Table table)
        {
            var eventType = _typeMapping[typename];
            GetType().GetMethod("VerifyEventsProducedFromTable").MakeGenericMethod(eventType).Invoke(this, new object[] { table });
        }

        private void SetupEventListeningForEmitter(EventInfo emitter, IConsumeEvent handler)
        {
            var eventType = emitter.EventHandlerType.GetGenericArguments().First();
            _emittedEvents[eventType] = new List<IEvent>();
            Delegate del = Delegate.CreateDelegate(emitter.EventHandlerType, this, GetType().GetMethod("HandleEvent").MakeGenericMethod(eventType));
            emitter.AddEventHandler(handler, del);
        }

        /// <summary>
        /// Function called when an event is triggered within the handler. This function can be ignored by the developer.
        ///
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        public void HandleEvent<T>(T @event) where T: IEvent
        {
            _emittedEvents[typeof(T)].Add(@event);
        }

        /// <summary>
        /// Function called to simulate incoming events to the handler. This function can be ignored by the developer.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        public void HandleEventsFromTable<T>(Table table)
            where T: IEvent
        {
            if (!_container.IsRegistered<IEventInstanceFactory<T>>())
            {
                throw new Exceptions.EventInstanceFactoryNotRegisteredException(typeof(T));
            }

            var eventTypeFactory = _container.Resolve<IEventInstanceFactory<T>>();
            var handler = (IConsumeEvent<T>) _handler;
            foreach (var row in table.Rows)
            {
                var @event = eventTypeFactory.FromTableRow(row);
                handler.Handle(@event);
            }
        }

        /// <summary>
        /// Function called to verify outgoing events from the handler. This function can be ignored by the developer.
        /// 
        /// IMPORTANT: This function appears to not be in use, but will be called at runtime using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        public void VerifyEventsProducedFromTable<T>(Table table)
            where T: IEvent
        {
            if (!_container.IsRegistered<IProducedEventVerifier<T>>())
            {
                throw new Exceptions.ProducedEventVerifierNotRegisteredException(typeof(T));
            }
            var verifier = _container.Resolve<IProducedEventVerifier<T>>();
            var emittedEvents = _emittedEvents[typeof(T)].Select(_ => (T) _).ToList();
            emittedEvents.Count.Should().Be(table.Rows.Count);
            foreach (var (@event, expected) in emittedEvents.Zip(table.Rows))
            {
                verifier.VerifyFromTableRow(@event, expected);
            }
        }
    }
}
