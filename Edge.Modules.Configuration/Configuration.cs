using Autofac;
using System.IO.Abstractions;

namespace RaaLabs.Edge.Modules.Configuration
{
    public class Configuration : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>().As<IFileSystem>();
            builder.RegisterSource<ConfigurationRegistrationSource>();
        }
    }
}
