using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.Mqtt.Client;

namespace RaaLabs.Edge.Modules.Mqtt
{
    /// <summary>
    /// The module for registering the EventHub bridge for the application.
    /// </summary>
    public class Mqtt : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MqttTopicMapper>()
                .AsSelf()
                .As<IMqttTopicMapper>()
                .InstancePerRuntime();

            builder.RegisterGeneric(typeof(MqttBrokerClient<>))
                .AsSelf()
                .As(typeof(IMqttBrokerClient<>))
                .InstancePerRuntime();

            builder.RegisterType<MqttMessageConverter>()
                .AsSelf()
                .As<IMqttMessageConverter>()
                .InstancePerRuntime();

            builder.RegisterBridge<MqttBridge>();
        }
    }
}
