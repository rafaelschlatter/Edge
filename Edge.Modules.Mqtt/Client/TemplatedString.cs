using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TemplatedString<T>
    {
        private readonly string _pattern;
        private readonly Regex _tokenPattern = new(@"{(?<token>[\d\w_]+)}");
        private readonly Regex _extractionPattern;

        public TemplatedString(string pattern)
        {
            ValidateTemplate(pattern);

            _pattern = pattern;
            
            // Build regex expression for extracting string into properties of type T
            _extractionPattern = new(_tokenPattern.Replace(pattern, match => BuildExtractionSubpattern(match)));
        }

        public string BuildFrom(T source)
        {
            return _tokenPattern.Replace(_pattern, match => BuildSubpattern(match, source));
        }

        public void ExtractTo(string input, T target)
        {
            var match = _extractionPattern.Match(input);
            var groupNames = _extractionPattern.GetGroupNames().Where(group => group != "0");
            foreach (var token in groupNames)
            {
                typeof(T).GetProperty(token).SetMethod.Invoke(target, new object[] { match.Groups[token].Value });
            }
        }

        private void ValidateTemplate(string pattern)
        {
            var undefinedProps = _tokenPattern.Matches(pattern)
                .Select(match => match.Groups["token"].Value)
                .Where(token => token != "_")
                .Where(token => typeof(T).GetProperty(token) == null)
                .ToList();

            if (undefinedProps.Count > 0)
            {
                throw new Exception($"Type {typeof(T).Name} is missing the following properties: [{string.Join(", ", undefinedProps)}]");
            }
        }

        private static string BuildSubpattern(Match match, T source)
        {
            var token = match.Groups["token"].Value;

            return token switch
            {
                "_" => "{_}",
                _ => (string)typeof(T).GetProperty(match.Groups["token"].Value).GetMethod.Invoke(source, null)
            };
        }

        private static string BuildExtractionSubpattern(Match match)
        {
            var token = match.Groups["token"].Value;

            return token switch
            {
                "_" => @"[\d\w_]+",
                _ => $"(?<{match.Groups["token"].Value}>[\\d\\w_]+)"
            };
        }
    }
}
