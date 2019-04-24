using MongoDB.Driver;
using System.Security.Authentication;

namespace BotApp
{
    public class DBHelper
    {
        private MongoDBConnectionInfo databaseInfo = null;

        public DBHelper(MongoDBConnectionInfo databaseInfo)
        {
            this.databaseInfo = databaseInfo;
        }

        public MongoDBConnectionInfo GetMongoDBConnectionInfo()
        {
            return databaseInfo;
        }

        public IMongoDatabase GetMongoDatabase()
        {
            IMongoDatabase database;
            string connectionString = GetMongoDBConnectionInfo().ConnectionString;
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            MongoClient mongoClient = new MongoClient(settings);
            database = mongoClient.GetDatabase(GetMongoDBConnectionInfo().DatabaseId);
            return database;
        }
    }
}