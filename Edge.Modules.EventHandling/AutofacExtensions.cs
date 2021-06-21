using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;

namespace RaaLabs.Edge.Modules.EventHandling
{
    /// <summary>
    /// 
    /// </summary>
    public static class AutofacExtensions
    {
        //
        // Summary:
        //     Register a bridge to be created through reflection.
        //
        // Parameters:
        //   builder:
        //     Container builder.
        //
        // Type parameters:
        //   TImplementer:
        //     The type of the IBridge implementation.
        //
        // Returns:
        //     Registration builder allowing the registration to be configured.
        public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterBridge<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementer>(this ContainerBuilder builder) where TImplementer : IBridge
        {
            return builder.RegisterType<TImplementer>().AsSelf().As<IBridge>().InstancePerRuntime();
        }
    }
}
