using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using TechTalk.SpecFlow;
using Serilog;
using BoDi;

namespace RaaLabs.Edge.Testing
{
    [Binding]
    public sealed class ContextProvider
    {
        private readonly IObjectContainer _container;
        private readonly ComponentAssemblies _assemblies;

        public ContextProvider(IObjectContainer container, ComponentAssemblies assemblies)
        {
            _container = container;
            _assemblies = assemblies;
        }

        [BeforeScenario]
        public void RegisterStuff()
        {
            var allTypes = _assemblies.SelectMany(assembly => assembly.GetTypes()).ToList();
            RegisterEventInstanceFactories(allTypes);
            RegisterProducedEventVerifiers(allTypes);
        }

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

        private Type GetEventInstanceFactoryInterface(Type type)
        {
            return type.GetInterfaces().FirstOrDefault(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(IEventInstanceFactory<>));
        }

        private Type GetProducedEventVerifierInterface(Type type)
        {
            return type.GetInterfaces().FirstOrDefault(ifce => ifce.IsGenericType && ifce.GetGenericTypeDefinition() == typeof(IProducedEventVerifier<>));
        }
    }
}
