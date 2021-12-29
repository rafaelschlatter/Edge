using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Serilog;
using System.Threading.Tasks;
using Autofac;
using System.IO.Abstractions;
using static RaaLabs.Edge.Modules.Configuration.ConfigurationFileFinder;

namespace RaaLabs.Edge.Modules.Configuration
{

    /// <summary>
    /// This class will monitor all files associated with configuration classes implementing ITriggerAppRestartOnChange,
    /// and halts the application if any of these files change.
    /// </summary>
    class ConfigurationFileChangedWatcher : IRunAsync
    {
        private readonly IApplicationShutdownTrigger _shutdownTrigger;
        private readonly ILogger _logger;
        private readonly IFileSystem _fs;
        private readonly ISet<Type> _watchedConfigurationClasses;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs">an abstraction of the file system</param>
        /// <param name="shutdownTrigger">a shutdown button for the application</param>
        /// <param name="logger">a logger</param>
        public ConfigurationFileChangedWatcher(IFileSystem fs, IApplicationShutdownTrigger shutdownTrigger, ILogger logger)
        {
            _fs = fs;
            _logger = logger;
            _shutdownTrigger = shutdownTrigger;
            _watchedConfigurationClasses = new HashSet<Type>();
        }

        public void WatchConfigurationClass(Type type)
        {
            _watchedConfigurationClasses.Add(type);
        }

        public async Task Run()
        {
            var filenames = _watchedConfigurationClasses
                .Select(clazz => (clazz, attribute: clazz.GetCustomAttribute<NameAttribute>(true)))
                .Select(cc => (cc.clazz, cc.attribute.Name))
                .ToArray();

            var filesToWatch = filenames.Select(_ => (_.clazz, path: FindConfigurationFilePath(_fs, _.Name, _logger))).ToArray();

            // Neither FileSystemWatcher nor PhysicalFileProvider have worked platform-independently at watching files asynchronously,
            // not even with DOTNET_USE_POLLING_FILE_WATCHER=1. Because of this, we will watch all configuration files manually instead.
            var filesChangedAt = filesToWatch.ToDictionary(file => file.path, file => _fs.File.GetLastWriteTimeUtc(file.path));

            while (true)
            {
                if (filesChangedAt.Any(file => _fs.File.GetLastWriteTimeUtc(file.Key) != file.Value))
                {
                    _logger.Information($"Configuration changed, restarting application...");
                    _shutdownTrigger.ShutdownApplication();
                }
                await Task.Delay(1000);
            }
        }

    }
}
