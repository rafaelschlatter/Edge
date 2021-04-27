using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using RaaLabs.Edge;
using Serilog;
using System.Threading.Tasks;
using Autofac;
using System.IO.Abstractions;

namespace RaaLabs.Edge.Modules.Configuration
{

    /// <summary>
    /// This class will monitor all files associated with configuration classes implementing ITriggerAppRestartOnChange,
    /// and halts the application if any of these files change.
    /// </summary>
    class ConfigurationFileChangedWatcher : IRunAsync
    {
        private static readonly string[] _searchPaths = { "data" };
        private readonly IApplicationRestartTrigger _restartTrigger;
        private readonly ILogger _logger;
        private readonly IFileSystem _fs;
        private readonly ISet<Type> _watchedConfigurationClasses;
        //private List<PhysicalFileProvider> _watchers;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeFinder"></param>
        /// <param name="logger"></param>
        /// <param name="fileProvider"></param>
        public ConfigurationFileChangedWatcher(IFileSystem fs, IApplicationRestartTrigger restartTrigger, ILogger logger)
        {
            _fs = fs;
            _logger = logger;
            _restartTrigger = restartTrigger;
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

            var pwd = _fs.Directory.GetCurrentDirectory();

            var filesToWatch = filenames.Select(_ => (_.clazz, path: FindConfigurationFilePath(_.Name))).ToArray();

            // Neither FileSystemWatcher nor PhysicalFileProvider have worked platform-independently at watching files asynchronously,
            // not even with DOTNET_USE_POLLING_FILE_WATCHER=1. Because of this, we will watch all configuration files manually instead.
            var filesChangedAt = filesToWatch.ToDictionary(file => file.path, file => _fs.File.GetLastWriteTimeUtc(file.path));

            while (true)
            {
                if (filesChangedAt.Any(file => _fs.File.GetLastWriteTimeUtc(file.Key) != file.Value))
                {
                    _logger.Information($"Configuration changed, restarting application...");
                    _restartTrigger.RestartApplication();
                }
                await Task.Delay(1000);
            }
        }

        private string FindConfigurationFilePath(string filename)
        {
            var pwd = _fs.Directory.GetCurrentDirectory();
            var dirs = _searchPaths
                .Select(_ => _fs.Path.Combine(pwd, _))
                .Where(_ => _fs.Directory.Exists(_))
                .ToArray();

            var matchedFiles = dirs
                .SelectMany(_ => _fs.Directory.EnumerateFiles(_fs.Path.Combine(pwd, _)))
                .Where(_ => _fs.Path.GetFileName(_) == filename)
                .ToArray();

            return matchedFiles.First();
        }
    }
}
