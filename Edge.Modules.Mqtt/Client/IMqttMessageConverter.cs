using MQTTnet;
using RaaLabs.Edge.Modules.EventHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    public interface IMqttMessageConverter
    {
        public IEvent ToEvent(Type connection, MqttApplicationMessage message);
        public (Type connection, MqttApplicationMessage message)? ToMessage(IEvent @event);
    }

}
