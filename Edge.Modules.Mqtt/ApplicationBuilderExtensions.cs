using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaaLabs.Edge;

namespace RaaLabs.Edge.Modules.Mqtt
{
    public static class ApplicationBuilderExtensions
    {
        public static ApplicationBuilder WithMqttBroker<T>(this ApplicationBuilder builder)
            where T : IMqttBrokerConnection
        {
            builder.WithSingletonType<T, IMqttBrokerConnection>();
            return builder;
        }
    }
}
