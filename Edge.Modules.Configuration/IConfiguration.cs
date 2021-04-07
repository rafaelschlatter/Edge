using System;

namespace RaaLabs.Edge.Modules.Configuration
{
    /// <summary>
    /// An interface marking the type as a configuration type.
    /// </summary>
    public interface IConfiguration
    {
    }

    /// <summary>
    /// An attribute for naming the configuration file to load into the type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NameAttribute : Attribute
    {
        public string Name { get; }
        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}
