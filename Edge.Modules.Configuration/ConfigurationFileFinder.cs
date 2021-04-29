using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Configuration
{
    public static class ConfigurationFileFinder
    {
        private static readonly string[] _searchPaths = { "data", "config" };

        public static string FindConfigurationFilePath(IFileSystem fs, string filename)
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

            return matchedFiles.First();
        }

    }
}
