using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt.Client
{
    public interface IMqttMessageConverter<T>
    {
        public T ToEvent(MqttApplicationMessage message);
        public MqttApplicationMessage ToMessage(T @event);
    }
}
