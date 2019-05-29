namespace BotApp.Extensions.Common.Consul.Domain
{
    public class ConsulConfig
    {
        public string Address { get; set; }
        public string ServiceName { get; set; }
        public string ServiceID { get; set; }
        public string ServiceTag { get; set; }
    }
}