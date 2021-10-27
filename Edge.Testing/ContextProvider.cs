using System;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using BoDi;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// Class setting up context from scanned assemblies
    /// </summary>
    [Binding]
    public sealed class ContextProvider
    {
        private readonly IObjectContainer _container;
        private readonly ComponentAssemblies _assemblies;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        public ContextProvider(IObjectContainer container, ComponentAssemblies assemblies)
        {
            _container = container;
            _assemblies = assemblies;
        }

        /// <summary>
        /// 
        /// </summary>
        [BeforeScenario(Order = 100)]
        public void RegisterClassesFromAssemblies()
        {
            var allTypes = _assemblies.SelectMany(assembly => assembly.GetTypes()).ToList();
            RegisterEventInstanceFactories(allTypes);
            RegisterProducedEventVerifiers(allTypes);
            RegisterProducedEventVerifiersFullTable(allTypes);
        }

        /// <summary>
        /// Scan assemblies for implementations of IEventInstanceFactory, and register each of these types
        /// into the test context.
        /// </summary>
        /// <param name="allTypes">a list of all types found in scanned assemblies</param>
        public void RegisterEventInstanceFactories(IEnumerable<Type> allTypes)
        {
            var eventInstanceFactoryTypes = allTypes
                .Where(type => GetEventInstanceFactoryInterface(type) != null)
                .ToList();

            foreach(var eventInstanceFactoryType in eventInstanceFactoryTypes)
            {
                var factoryForType = GetEventInstanceFactoryInterface(eventInstanceFactoryType);
                var registerTypeAs = typeof(IObjectContainer).GetMethod("RegisterTypeAs").MakeGenericMethod(eventInstanceFactoryType, factoryForType);
                registerTypeAs.Invoke(_container, new object[] { null });
            }
        }

        /// <summary>
        /// Scan assemblies for implementations of IProducedEventVerifier, and register each of these types
        /// into the test context.
        /// </summary>
        /// <param name="allTypes">a list of all types found in scanned assemblies</param>
        public void RegisterProducedEventVerifiers(IEnumerable<Type> allTypes)
        {
            var producedEventVerifierTypes = allTypes
                .Where(type => GetProducedEventVerifierInterface(type) != null)
                .ToList();

            foreach (var producedEventVerifierType in producedEventVerifierTypes)
            {
                var factoryForType = GetProducedEventVerifierInterface(producedEventVerifierType);
                var registerTypeAs = typeof(IObjectContainer).GetMethod("RegisterTypeAs").MakeGenericMethod(producedEventVerifierType, factoryForType);
                registerTypeAs.Invoke(_container, new object[] { null });
            }
        }

        /// <summary>
        /// Scan assemblies for implementations of IProducedEventVerifierFullTable, and register each of these types
        /// into the test context.
        /// </summary>
        /// <param name="allTypes">a list of all types found in scanned assemblies</param>
        public void RegisterProducedEventVerifiersFullTable(IEnumerable<Type> allTypes)
        {
            var producedEventVerifierFullTableTypes = allTypes
                .Where(type => GetProducedEventVerifierFullTableInterface(type) != null)
                .ToList();

            foreach (var producedEventVerifierType in producedEventVerifierFullTableTypes)
            {
                var factoryForType = GetProducedEventVerifierFullTableInterface(producedEventVerifierType);
                var registerTypeAs = typeof(IObjectContainer).GetMethod("RegisterTypeAs").MakeGenericMethod(producedEventVerifierType, factoryForType);
                registerTypeAs.Invoke(_container, new object[] { null });
            }
        }

        private static Type GetEventInstanceFactoryInterface(Type type)
        {
            return type.GetInterfaces().FirstOrDefault(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(IEventInstanceFactory<>));
        }

        private static Type GetProducedEventVerifierInterface(Type type)
        {
            return type.GetInterfaces().FirstOrDefault(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(IProducedEventVerifier<>));
        }

        private static Type GetProducedEventVerifierFullTableInterface(Type type)
        {
            return type.GetInterfaces().FirstOrDefault(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(IAllProducedEventsVerifier<>));
        }
    }
}
