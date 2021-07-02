namespace RaaLabs.Edge.Modules.Mqtt.Client.Authentication
{
    public class UsernameAndPasswordAuthentication : IAuthentication
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
