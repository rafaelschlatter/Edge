using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHandling
{
    #nullable enable
    public static class TypeExtensions
    {
        /// <summary>
        /// Get a specified attribute for the given class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T? GetAttribute<T>(this Type type)
        {
            return (T?)type.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        /// <summary>
        /// Get all interfaces implemented for this class that is _not_ implemented in inherited interfaces.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetImmediateInterfaces(this Type type)
        {
            var allInterfaces = type.GetInterfaces();
            var allInterfacesFromInheritedInterfaces = allInterfaces.SelectMany(ifce => ifce.GetInterfaces()).Distinct();
            
            return allInterfaces.Except(allInterfacesFromInheritedInterfaces).ToArray();
        }
    }
}
