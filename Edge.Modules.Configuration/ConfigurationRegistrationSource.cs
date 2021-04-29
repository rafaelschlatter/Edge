using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace RaaLabs.Edge.Modules.Configuration
{
    /// <summary>
    /// An Autofac registration source for resolving all types implementing IConfiguration.
    /// </summary>
    class ConfigurationRegistrationSource : IRegistrationSource
    {
        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            // If service is not a TypedService, don't provide any registrations.
            var ts = service as TypedService;
            if (ts == null)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            // If the concrete type requested is not assignable to an IConfiguration variable, don't provide any registrations.
            var serviceType = ts.ServiceType;
            if (!typeof(IConfiguration).IsAssignableFrom(serviceType))
            {
                return Enumerable.Empty<IComponentRegistration>();
            }

            var registration = new ComponentRegistration(
                Guid.NewGuid(),
                new DelegateActivator(serviceType, (c, p) =>
                {
                    var fs = c.Resolve<IFileSystem>();
                    var config = LoadConfigurationObject(serviceType, fs);

                    if (serviceType.GetCustomAttribute<RestartOnChangeAttribute>() != null)
                    {
                        var fileWatcher = c.Resolve<ConfigurationFileChangedWatcher>();
                        fileWatcher.WatchConfigurationClass(serviceType);
                    }

                    return config;
                }),
                new CurrentScopeLifetime(),
                InstanceSharing.None,
                InstanceOwnership.OwnedByLifetimeScope,
                new[] { service },
                new Dictionary<string, object>());

            return new IComponentRegistration[] { registration };
        }

        /// <summary>
        /// Load a configuration file into a type instance implementing the IConfiguration interface.
        /// Uses the Name(...) attribute of the type as a filename for the configuration file.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        private static IConfiguration LoadConfigurationObject(Type type, IFileSystem fs)
        {
            string filename = type.GetCustomAttribute<NameAttribute>().Name;
            var path = ConfigurationFileFinder.FindConfigurationFilePath(fs, filename);
            string content = fs.File.ReadAllText(path);
            IConfiguration configuration = (IConfiguration)JsonConvert.DeserializeObject(content, type);

            return configuration;
        }

    }
}
