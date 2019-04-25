using MongoDB.Driver;
using RF.ContentSearch.Api.Helpers.Base;
using RF.ContentSearch.Domain.Entities.Data;
using RF.Identity.Domain.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDBConnectionInfo = RF.ContentSearch.Domain.Entities.Data.MongoDBConnectionInfo;

namespace RF.ContentSearch.App.Helpers.Data
{
    public class UserDataHelper : DBBaseHelper
    {
        public UserDataHelper(MongoDBConnectionInfo databaseInfo) : base(databaseInfo)
        {
        }

        public User GetUser(string email)
        {
            IMongoCollection<User> userCollection = GetMongoDatabase().GetCollection<User>(GetMongoDBConnectionInfo().UserCollection);
            User result = null;
            result = userCollection.AsQueryable<User>().Where<User>(e => e.email == email).SingleOrDefault();
            return result;
        }

        public async Task RegisterUserAsync(User entity)
        {
            DateTime datetimestamp = DateTime.UtcNow;
            IMongoCollection<User> userCollection = GetMongoDatabase().GetCollection<User>(GetMongoDBConnectionInfo().UserCollection);
            entity.created_date = datetimestamp;
            entity.updated_date = datetimestamp;
            await userCollection.InsertOneAsync(entity);
        }
    }
}
