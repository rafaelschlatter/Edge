using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Testing.Exceptions
{
    class ProducedEventVerifierNotRegistered : Exception
    {
        public Type Type { get; }
        public ProducedEventVerifierNotRegistered(Type type) : base($"IProducedEventVerifier<{type.Name}> has not been registered.")
        {
            Type = type;
        }
    }
}
