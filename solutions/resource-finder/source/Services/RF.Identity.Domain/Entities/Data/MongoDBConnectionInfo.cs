namespace RF.Identity.Domain.Entities.Data
{
    public class MongoDBConnectionInfo
    {
        public string ConnectionString { get; set; }
        public string DatabaseId { get; set; }
        public string UserCollection { get; set; }
    }
}