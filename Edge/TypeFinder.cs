using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RaaLabs.Edge.Modules
{
    public static class TypeFinder
    {
        public static IEnumerable<Type> ImplementationsOf<T>()
        {
            var type = typeof(T);
            var dataAccess = Assembly.GetEntryAssembly();
            var definedTypes = dataAccess.DefinedTypes;

            var implementations = definedTypes
                .Where(t => {
                    bool containsInteface = !t.IsInterface && t.GetInterfaces().Contains(typeof(T));
                    return containsInteface;
                }).ToList();

            return implementations;
        }

        public static IEnumerable<Type> ImplementationsOf(Type type)
        {
            var dataAccess = Assembly.GetEntryAssembly();
            var definedTypes = dataAccess.DefinedTypes;

            return definedTypes
                .Where(t => t.GetInterfaces().Contains(type));
        }
    }
}
