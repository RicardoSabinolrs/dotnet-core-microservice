namespace SabinoLabs.Configuration.Consul
{
    public class ConsulOptions
    {
        public bool Enabled { get; set; }
        public string Host { get; set; }
        public string Service { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool PingEnabled { get; set; }
    }
}
