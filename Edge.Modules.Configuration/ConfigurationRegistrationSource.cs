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
    class ConfigurationRegistrationSource : IRegistrationSource
    {
        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            var ts = service as TypedService;
            if (ts == null)
            {
                return Enumerable.Empty<IComponentRegistration>();
            }
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
                    return LoadConfigurationObject(serviceType, fs);
                }),
                new CurrentScopeLifetime(),
                InstanceSharing.None,
                InstanceOwnership.OwnedByLifetimeScope,
                new[] { service },
                new Dictionary<string, object>());

            return new IComponentRegistration[] { registration };
        }

        private static IConfiguration LoadConfigurationObject(Type type, IFileSystem fs)
        {
            string filename = type.GetCustomAttribute<NameAttribute>().Name;
            string path = Path.Join(fs.Directory.GetCurrentDirectory(), "data", filename);
            string content = fs.File.ReadAllText(path);
            IConfiguration configuration = (IConfiguration)JsonConvert.DeserializeObject(content, type);

            return configuration;
        }

    }
}
