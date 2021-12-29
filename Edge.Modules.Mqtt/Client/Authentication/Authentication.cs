namespace RaaLabs.Edge.Modules.Mqtt.Client.Authentication
{
    public class Authentication : IAuthentication
    {
        public string Method { get; set; }
        public byte[] Data { get; set; }
    }
}
