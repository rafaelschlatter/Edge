using Autofac;
using System.IO.Abstractions;

namespace RaaLabs.Edge.Modules.Configuration
{
    /// <summary>
    /// An Autofac module providing the application with a registration source for resolving all classes
    /// implementing the IConfiguration interface.
    /// </summary>
    public class Configuration : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterSource<ConfigurationRegistrationSource>();
        }
    }
}
