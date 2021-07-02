using RaaLabs.Edge.Modules.Mqtt.Client.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.Mqtt
{
    public interface IMqttBrokerConnection
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string ClientId { get; set; }

        public IAuthentication Authentication { get; set; }
    }
}
