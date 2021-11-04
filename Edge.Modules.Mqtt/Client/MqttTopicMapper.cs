using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    /// <summary>
    /// A mapper to translate from an MQTT topic to the associated event type for that topic.
    /// </summary>
    public class MqttTopicMapper : IMqttTopicMapper
    {
        /// <summary>
        /// A dictionary holding the topic routers for all connections.
        /// </summary>
        private readonly Dictionary<Type, MqttRouter<Type>> _topicRoutes;

        public MqttTopicMapper(IEventHandler<IMqttIncomingEvent> incomingHandler, IEventHandler<IMqttOutgoingEvent> outgoingHandler)
        {
            var allEventTypes = incomingHandler.GetSubtypes().Union(outgoingHandler.GetSubtypes()).ToHashSet();
            _topicRoutes = BuildRouters(allEventTypes);
        }

        /// <inheritdoc/>
        public Type Resolve(Type connection, string topic)
        {
            return _topicRoutes[connection].ResolvePath(topic);
        }

        private static Dictionary<Type, MqttRouter<Type>> BuildRouters(IEnumerable<Type> eventTypes)
        {
            return eventTypes
                .GroupBy(type => type.GetAttribute<MqttBrokerConnectionAttribute>().BrokerConnection)
                .ToDictionary(typesForBroker => typesForBroker.Key, typesForBroker => BuildRouter(typesForBroker));
        }

        private static MqttRouter<Type> BuildRouter(IEnumerable<Type> eventTypes)
        {
            var routes = eventTypes
                .Select(type => (topic: type.GetAttribute<MqttBrokerConnectionAttribute>().Topic, type))
                .Select(type => (topic: ToRegularTopic(type.topic), type.type));

            return new MqttRouter<Type>(routes);
        }

        /// <summary>
        /// Regex pattern for detecting placeholders inside MQTT topics
        /// </summary>
        private static readonly Regex _topicTokenPattern = new(@"{(?<token>[\d\w_]+)}");

        /// <summary>
        /// Convert from topic containing placeholders, to a regular topic containing only wildcards
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public static string ToRegularTopic(string topic)
        {
            return _topicTokenPattern.Replace(topic, "+");
        }
    }

    public interface IMqttTopicMapper
    {
        /// <summary>
        /// Resolve what event type is associated with a given topic.
        /// </summary>
        /// <param name="connection">the connection that received the MQTT message</param>
        /// <param name="topic">the topic for the MQTT message</param>
        /// <returns>the event type associated with the specified topic</returns>
        public Type Resolve(Type connection, string topic);
    }

    /// <summary>
    /// Route an MQTT topic to an instance of a given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MqttRouter<T> where T : class
    {
        private readonly List<(string pattern, T target)> _routes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routes"></param>
        public MqttRouter(IEnumerable<(string pattern, T target)> routes)
        {
            _routes = routes.ToList();
        }

        /// <summary>
        /// Resolve the value associated with a given topic
        /// </summary>
        /// <param name="topic">the topic to resolve</param>
        /// <returns>the value associated with the topic, if any. Else, returns null.</returns>
        public T ResolvePath(string topic)
        {
            return _routes
                .Where(route => TopicMatches(topic, route.pattern))
                .Select(route => route.target)
                .FirstOrDefault();
        }

        /// <summary>
        /// Compares a topic with a given MQTT pattern
        /// </summary>
        /// <param name="topic">the topic</param>
        /// <param name="pattern">the pattern to match the topic with</param>
        /// <returns>true if the topic matches the given pattern, else returns false</returns>
        private static bool TopicMatches(string topic, string pattern)
        {
            if (topic == pattern) return true;
            var topicLevels = topic.Split("/");
            var patternLevels = pattern.Split("/");

            var hasMultilevelWildcard = patternLevels.Last() == "#";

            var mismatchingTopicLevels = !hasMultilevelWildcard && topicLevels.Length != patternLevels.Length;
            var topicEndsBeforeMultilevelWildcard = hasMultilevelWildcard && topicLevels.Length < patternLevels.Length;

            if (mismatchingTopicLevels || topicEndsBeforeMultilevelWildcard)
            {
                return false;
            }

            foreach (var (t, p) in topicLevels.Zip(patternLevels))
            {
                if (p == "#") return true;
                if (p == "+" || t == p) continue;
                return false;
            }

            return true;
        }
    }
}
