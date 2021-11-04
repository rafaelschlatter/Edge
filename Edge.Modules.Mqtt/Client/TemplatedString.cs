using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RaaLabs.Edge.Modules.Mqtt
{
    /// <summary>
    /// A small templating util class for parsing and building strings using a specified template.
    /// 
    /// Example: a template string "{SomeValue} is cooler than {AnotherValue}", and a class with properties 'SomeValue' and 'AnotherValue'.
    /// </summary>
    /// <typeparam name="T">the type associated with the template</typeparam>
    public class TemplatedString<T>
    {
        private readonly string _pattern;
        private readonly Regex _tokenPattern = new(@"{(?<token>[\d\w_]+)}");
        private readonly Regex _extractionPattern;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pattern"></param>
        public TemplatedString(string pattern)
        {
            ValidateTemplate(pattern);

            _pattern = pattern;
            
            // Build regex expression for extracting string into properties of type T
            _extractionPattern = new(_tokenPattern.Replace(pattern, match => BuildExtractionSubpattern(match)));
        }

        /// <summary>
        /// Build a string from a given instance of the associated type. 
        /// </summary>
        /// <param name="source">the instance of the associated type</param>
        /// <returns>a string built from the instance of the associated type</returns>
        public string BuildFrom(T source)
        {
            return _tokenPattern.Replace(_pattern, match => BuildSubpattern(match, source));
        }

        /// <summary>
        /// Extract the pattern variables of the string into an instance of the associated type.
        /// </summary>
        /// <param name="input">the input string matching the pattern</param>
        /// <param name="target">the instance to extract the variables to</param>
        public void ExtractTo(string input, T target)
        {
            var match = _extractionPattern.Match(input);
            var groupNames = _extractionPattern.GetGroupNames().Where(group => group != "0");
            foreach (var token in groupNames)
            {
                typeof(T).GetProperty(token).SetMethod.Invoke(target, new object[] { match.Groups[token].Value });
            }
        }

        /// <summary>
        /// If the associated type does not contain properties for the specified variables in the template, validation should fail.
        /// </summary>
        /// <param name="pattern">the pattern to validate</param>
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
                _ => (string)typeof(T).GetProperty(token).GetMethod.Invoke(source, null)
            };
        }

        private static string BuildExtractionSubpattern(Match match)
        {
            var token = match.Groups["token"].Value;

            return token switch
            {
                "_" => @"[\d\w_]+",
                _ => $"(?<{token}>[\\d\\w_\\s]+)"
            };
        }
    }
}
