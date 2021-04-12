using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Testing.Exceptions
{
    class EventInstanceFactoryNotRegistered : Exception
    {
        public Type Type { get; }
        public EventInstanceFactoryNotRegistered(Type type) : base($"IEventInstanceFactory<{type.Name}> has not been registered.")
        {
            Type = type;
        }
    }
}
