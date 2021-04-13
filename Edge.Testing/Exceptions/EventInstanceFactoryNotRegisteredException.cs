using System;

namespace RaaLabs.Edge.Testing.Exceptions
{
    /// <summary>
    /// This class will be thrown if none of the scanned assemblies contains a class implementing IEventInstanceFactory<Type>.
    /// </summary>
    public class EventInstanceFactoryNotRegisteredException : Exception
    {
        /// <summary>
        /// The event type the IEventInstanceFactory class should construct
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public EventInstanceFactoryNotRegisteredException(Type type) : base($"IEventInstanceFactory<{type.Name}> has not been registered.")
        {
            Type = type;
        }
    }
}
