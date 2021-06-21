using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;

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
    }
}
