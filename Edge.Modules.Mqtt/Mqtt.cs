using Autofac;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.Mqtt.Client;
using RaaLabs.Edge.Modules.Mqtt.Client.Consumer;
using RaaLabs.Edge.Modules.Mqtt.Client.Producer;

namespace RaaLabs.Edge.Modules.Mqtt
{
    /// <summary>
    /// The module for registering the EventHub bridge for the application.
    /// </summary>
    public class Mqtt : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(MqttBrokerClient<>))
                .AsSelf()
                .As(typeof(IMqttBrokerClient<>))
                .InstancePerRuntime();

            builder.RegisterGeneric(typeof(MqttConsumerClient<>))
                .AsSelf()
                .As(typeof(IMqttConsumerClient<>))
                .InstancePerRuntime();

            builder.RegisterGeneric(typeof(MqttProducerClient<>))
                .AsSelf()
                .As(typeof(IMqttProducerClient<>))
                .InstancePerRuntime();

            builder.RegisterGeneric(typeof(MqttMessageConverter<>))
                .AsSelf()
                .As(typeof(IMqttMessageConverter<>))
                .InstancePerRuntime();

            builder.RegisterBridge<MqttBridge>();
        }
    }
}
