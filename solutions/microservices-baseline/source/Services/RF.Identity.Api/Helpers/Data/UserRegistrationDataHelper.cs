using MongoDB.Driver;
using RF.Identity.Api.Helpers.Base;
using RF.Identity.Domain.Entities.Data;
using System.Linq;

namespace RF.Identity.Api.Helpers.Data
{
    public class UserRegistrationDataHelper : DBBaseHelper
    {
        public UserRegistrationDataHelper(MongoDBConnectionInfo databaseInfo) : base(databaseInfo)
        {
        }

        public User GetUser(string email)
        {
            User result = null;
            IMongoCollection<User> userCollection = GetMongoDatabase().GetCollection<User>(GetMongoDBConnectionInfo().UserCollection);
            result = userCollection.AsQueryable<User>().Where<User>(e => e.email == email).SingleOrDefault();
            return result;
        }
    }
}