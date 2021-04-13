using System.Collections.Generic;
using System.Reflection;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A class holding all assemblies to perform component scan on
    /// </summary>
    public class ComponentAssemblies : HashSet<Assembly>
    {

        /// <summary>
        /// 
        /// </summary>
        public ComponentAssemblies() : base() { }
    }
}
