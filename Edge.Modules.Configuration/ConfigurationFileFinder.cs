using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace RaaLabs.Edge.Modules.Configuration
{
    public static class ConfigurationFileFinder
    {
        private static readonly string[] _searchPaths = { "data", "config" };

        public static string FindConfigurationFilePath(IFileSystem fs, string filename, ILogger logger)
        {
            var pwd = fs.Directory.GetCurrentDirectory();
            var dirs = _searchPaths
                .Select(_ => fs.Path.Combine(pwd, _))
                .Where(_ => fs.Directory.Exists(_))
                .ToArray();

            var matchedFiles = dirs
                .SelectMany(_ => fs.Directory.EnumerateFiles(fs.Path.Combine(pwd, _)))
                .Where(_ => fs.Path.GetFileName(_) == filename)
                .ToArray();

            if (matchedFiles.Length == 0)
            {
                var searchDirs = string.Join(", ", dirs);
                logger.Error("Could not find configuration file '{Filename}' in any of the following directories: {Dirs}", filename, searchDirs);
                throw new Exception($"Unable to find configuration file '{filename}' in any of the following directories: {searchDirs}");
            }

            return matchedFiles.First();
        }

    }
}
