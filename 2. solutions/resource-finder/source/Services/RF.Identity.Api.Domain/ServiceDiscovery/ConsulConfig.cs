namespace RF.Identity.Api.Domain.ServiceDiscovery
{
    public class ConsulConfig
    {
        public string Address { get; set; }
        public string ServiceName { get; set; }
        public string ServiceID { get; set; }
        public string ServiceTag { get; set; }
    }
}