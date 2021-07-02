using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using RaaLabs.Edge.Serialization;

namespace RaaLabs.Edge
{
    /// <summary>
    /// 
    /// </summary>
    public static class AutofacExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> InstancePerRuntime<TLimit, TActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.InstancePerMatchingLifetimeScope("runtime");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterTask<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementer>(this ContainerBuilder builder)
            where TImplementer : IRunAsync
        {
            return builder.RegisterType<TImplementer>().AsSelf().As<IRunAsync>().InstancePerRuntime();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static ISerializer<T> ResolveSerializer<T, R>(this ILifetimeScope scope)
        {
            return
                   scope.ResolveOptionalNamed<ISerializer<T>>(typeof(R).Name)
                ?? scope.ResolveSerializer<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static ISerializer<T> ResolveSerializer<T>(this ILifetimeScope scope, Type receiver)
        {
            return
                   scope.ResolveOptionalNamed<ISerializer<T>>(receiver.Name)
                ?? scope.ResolveSerializer<T>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static ISerializer<T> ResolveSerializer<T>(this ILifetimeScope scope)
        {
            return
                   scope.ResolveOptional<ISerializer<T>>()
                ?? new JsonSerializer<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <typeparam name="R">The receiver for the deserializer</typeparam>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IDeserializer<T> ResolveDeserializer<T, R>(this ILifetimeScope scope)
        {
            return
                   scope.ResolveOptionalNamed<IDeserializer<T>>(typeof(R).Name)
                ?? scope.ResolveDeserializer<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <param name="scope"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static IDeserializer<T> ResolveDeserializer<T>(this ILifetimeScope scope, Type receiver)
        {
            return
                   scope.ResolveOptionalNamed<IDeserializer<T>>(receiver.Name)
                ?? scope.ResolveDeserializer<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IDeserializer<T> ResolveDeserializer<T>(this ILifetimeScope scope)
        {
            return
                   scope.ResolveOptional<IDeserializer<T>>()
                ?? new JsonDeserializer<T>();
        }
    }
}
