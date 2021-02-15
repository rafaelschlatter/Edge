using Autofac;
using Autofac.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RaaLabs.Edge.Modules.Configuration
{
    public class Configuration : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configurationTypes = TypeFinder.ImplementationsOf<IConfiguration>();

            configurationTypes
                .Select(c => LoadConfigurationObject(c)).ToList()
                .ForEach(c => builder.RegisterInstance(c).AsSelf());
        }

        private static IConfiguration LoadConfigurationObject(Type type)
        {
            string filename = type.GetCustomAttribute<NameAttribute>().Name;
            string path = Path.Join(Directory.GetCurrentDirectory(), "data", filename);
            IConfiguration configuration = (IConfiguration)JsonConvert.DeserializeObject(File.ReadAllText(path), type);

            return configuration;
        }

        private static bool IsConfiguration(IComponentRegistration registration, out Type configurationType)
        {
            configurationType = null;
            var configurationTypes = registration.Services
                .Where(s => s is IServiceWithType && typeof(IConfiguration).IsAssignableFrom(((IServiceWithType)s).ServiceType))
                .Select(s => ((IServiceWithType)s).ServiceType)
                .ToList();

            if (configurationTypes.Count == 0)
            {
                return false;
            }

            configurationType = configurationTypes.First();

            return true;
        }

    }
}
