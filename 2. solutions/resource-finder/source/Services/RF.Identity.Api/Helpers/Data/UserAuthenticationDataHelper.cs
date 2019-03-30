using MongoDB.Driver;
using RF.Identity.Api.Helpers.Base;
using RF.Identity.Domain.Entities.Data;
using System.Linq;

namespace RF.Identity.Api.Helpers.Data
{
    public class UserAuthenticationDataHelper : DBBaseHelper
    {
        public UserAuthenticationDataHelper(MongoDBConnectionInfo databaseInfo) : base(databaseInfo)
        {
        }

        public User GetUser(string email)
        {
            User result = null;
            IMongoCollection<User> userCollection = GetMongoDatabase().GetCollection<User>(GetMongoDBConnectionInfo().UserCollection);
            var filter = new FilterDefinitionBuilder<User>().Eq<string>(e => e.email, email);
            result = userCollection.FindSync<User>(filter).SingleOrDefault();
            return result;
        }
    }
}