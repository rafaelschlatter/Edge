using System;

namespace RaaLabs.Edge.Testing.Exceptions
{
    /// <summary>
    /// This class will be thrown if none of the scanned assemblies contains a class implementing IProducedEventVerifier<Type>.
    /// </summary>
    public class ProducedEventVerifierNotRegisteredException : Exception
    {
        /// <summary>
        /// The event type the IProducedEventVerifier class should verify
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public ProducedEventVerifierNotRegisteredException(Type type) : base($"IProducedEventVerifier<{type.Name}> has not been registered.")
        {
            Type = type;
        }
    }
}
