using BoDi;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using FluentAssertions;
using Autofac;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A class defining steps for building an application context
    /// </summary>
    [Binding]
    public sealed class ApplicationSteps
    {
        private readonly IObjectContainer _container;
        private ApplicationContext _appContext;
        private readonly TypeMapping _typeMapping;
        private Dictionary<Type, List<IEvent>> _producedEventsByType;
        private readonly List<(Type eventType, IEvent @event)> _producedEvents;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typeMapping"></param>
        public ApplicationSteps(IObjectContainer container, TypeMapping typeMapping)
        {
            _container = container;
            _typeMapping = typeMapping;
            _producedEvents = new List<(Type, IEvent)>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        [Given(@"an application with the following registrations")]
        public void GivenAnApplicationWithTheFollowingRegistrations(Table table)
        {
            _appContext = ApplicationContext.FromTable(table, _typeMapping);
            _container.RegisterInstanceAs(_appContext);
        }

        /// <summary>
        /// 
        /// </summary>
        [Given(@"the application is running")]
        public void GivenTheApplicationIsRunning()
        {
            _appContext.Start();
            var allEventTypes = _typeMapping
                .Select(mapping => mapping.Value)
                .Where(type => type.IsAssignableTo<IEvent>()).ToList();

            _producedEventsByType = allEventTypes.ToDictionary(type => type, type => new List<IEvent>());

            foreach (var type in allEventTypes)
            {
                var handler = _appContext.Scope.Resolve(typeof(Modules.EventHandling.EventHandler<>).MakeGenericType(type));
                var subscribeFunction = handler.GetType().GetMethod("Subscribe", new Type[] { typeof(Action<>).MakeGenericType(type) });
                Action<IEvent> subscriberFunction = (IEvent ev) =>
                {
                    _producedEventsByType[type].Add(ev);
                    _producedEvents.Add((type, ev));
                };
                subscribeFunction.Invoke(handler, new object[] { subscriberFunction });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [When(@"the following events are produced")]
        public void WhenTheFollowingEventsAreProduced(Table table)
        {
            HandleEventsFromTable(table);
        }

        /// <summary>
        /// 
        /// </summary>
        [Then(@"the following events are produced")]
        public void ThenTheFollowingEventsAreProduced(Table table)
        {
            Task.Delay(20).Wait();
            int eventIndex = 0;
            for (var i = 0; i < table.RowCount; i++)
            {
                var type = _typeMapping[table.Rows[i]["EventType"]];

                // Find the next event of the specified event type, skipping all events between.
                while (eventIndex < _producedEvents.Count && _producedEvents[eventIndex].eventType != type) eventIndex++;

                // If we reached the end of the list of produced events without matching all events in table, throw Exception
                if (eventIndex >= _producedEvents.Count) throw new Exception($"Reached end of produced events list without finding all expected events. First missing element at row {i+1}: {table.Rows[i].Values}");

                var verifier = _container.Resolve(typeof(IProducedEventVerifier<>).MakeGenericType(type));
                var verifyFunction = verifier.GetType().GetMethod("VerifyFromTableRow", BindingFlags.Public | BindingFlags.Instance);

                var producedEvent = _producedEvents[eventIndex].@event;

                verifyFunction.Invoke(verifier, new object[] { producedEvent, table.Rows[i] });

                eventIndex++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Then(@"the following events are produced in any order")]
        public void ThenTheFollowingEventsAreProducedInAnyOrder(Table table)
        {
            Task.Delay(20).Wait();
            var expectedEventsByEventType = table.Rows.GroupBy(row => _typeMapping[row["EventType"]], row => row);
            foreach (var expectedEventsForEventType in expectedEventsByEventType)
            {
                var eventType = expectedEventsForEventType.Key;
                var verifyFunction = (Func<IEvent, TableRow, bool>)GetType().GetMethod("MakeEventVerifierFunction", BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(eventType).Invoke(this, Array.Empty<object>());
                
                foreach (var expectedEvent in expectedEventsForEventType)
                {
                    _producedEventsByType[eventType].Any(@event => verifyFunction!(@event, expectedEvent)).Should().BeTrue();
                }
            }
        }

        /// <summary>
        /// Function called to simulate incoming events. This function can be ignored by the developer.
        /// </summary>
        /// <param name="table"></param>
        private void HandleEventsFromTable(Table table)
        {
            var allEventTypesInTable = table.Rows
                .Select(row => row["EventType"])
                .Distinct()
                .ToDictionary(type => type, type => _typeMapping[type]);

            var allEventInstanceFactories = allEventTypesInTable.ToDictionary(eventType => eventType.Key, eventType => _container.Resolve(typeof(IEventInstanceFactory<>).MakeGenericType(eventType.Value)));

            var allEventHandlers = allEventTypesInTable.ToDictionary(eventType => eventType.Key, eventType => _appContext.Scope.Resolve(typeof(Modules.EventHandling.EventHandler<>).MakeGenericType(eventType.Value)));

            foreach (var row in table.Rows)
            {
                var eventTypeName = row["EventType"];
                var eventType = allEventTypesInTable[eventTypeName];
                var factory = allEventInstanceFactories[eventTypeName];
                var factoryMethod = typeof(IEventInstanceFactory<>).MakeGenericType(eventType).GetMethod("FromTableRow");
                var @event = factoryMethod.Invoke(factory, new object[] { row });

                var eventHandler = allEventHandlers[eventTypeName];
                var eventProducerMethod = typeof(Modules.EventHandling.EventHandler<>).MakeGenericType(eventType).GetMethod("Produce");
                eventProducerMethod.Invoke(eventHandler, new object[] { @event });
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection")]
        private Func<IEvent, TableRow, bool> MakeEventVerifierFunction<EventType>() where EventType : IEvent
        {
            var verifier = _container.Resolve<IProducedEventVerifier<EventType>>();

            return (@event, row) =>
            {
                try
                {
                    verifier.VerifyFromTableRow((EventType) @event, row);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            };
        }
    }
}
