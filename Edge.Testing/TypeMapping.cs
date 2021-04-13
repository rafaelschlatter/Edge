using System;
using System.Collections.Generic;

namespace RaaLabs.Edge.Testing
{
    /// <summary>
    /// A dictionary for getting a type from a string representation of said type.
    /// The developer is responsible for populating the dictionary with the types used in the test.
    /// </summary>
    public class TypeMapping : Dictionary<string, Type>
    {
        /// <summary>
        /// 
        /// </summary>
        public TypeMapping()
        {
        }
    }
}
