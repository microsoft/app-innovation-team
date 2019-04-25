namespace RF.ContentSearch.Domain.Entities.Data
{
    public class MongoDBConnectionInfo
    {
        public string ConnectionString { get; set; }
        public string DatabaseId { get; set; }
        public string ContractCollection { get; set; }
        public string UserCollection { get; set; }
    }
}