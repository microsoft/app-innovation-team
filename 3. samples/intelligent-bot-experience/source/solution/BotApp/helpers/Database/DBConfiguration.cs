namespace BotApp
{
    public class DBConfiguration
    {
        public static MongoDBConnectionInfo GetMongoDbConnectionInfo()
        {
            MongoDBConnectionInfo mongoDbConnectionInfo = new MongoDBConnectionInfo()
            {
                ConnectionString = Settings.MongoDBConnectionString,
                DatabaseId = Settings.MongoDBDatabaseId,
                PersonCollection = Settings.PersonCollection
            };
            return mongoDbConnectionInfo;
        }
    }
}