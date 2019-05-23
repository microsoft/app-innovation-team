using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotApp
{
    public class PersonDBHelper : DBHelper
    {
        private IMongoCollection<Person> collection = null;

        public PersonDBHelper(MongoDBConnectionInfo databaseInfo) : base(databaseInfo)
        {
            collection = GetMongoDatabase().GetCollection<Person>(GetMongoDBConnectionInfo().PersonCollection);
        }

        public async Task<List<Person>> GetPersonListByFaceAsync(string faceId)
        {
            List<Person> result = new List<Person>();
            try
            {
                var filter = new FilterDefinitionBuilder<Person>().Eq<string>(p => p.FaceAPIFaceId, faceId);
                result = await collection.FindSync<Person>(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"EXCEPTION: {ex.Message}. STACKTRACE: {ex.StackTrace}");
            }
            return result;
        }

        public async Task<List<Person>> GetPersonListByHashAsync(string hash)
        {
            List<Person> result = new List<Person>();
            try
            {
                var filter = new FilterDefinitionBuilder<Person>().Eq<string>(p => p.Hash, hash);
                result = await collection.FindSync<Person>(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"EXCEPTION: {ex.Message}. STACKTRACE: {ex.StackTrace}");
            }
            return result;
        }

        public async Task CreatePersonAsync(Person person)
        {
            await collection.InsertOneAsync(person);
        }

        public async Task<bool> DeletePersonAsync(string hash)
        {
            DeleteResult result = await collection.DeleteOneAsync<Person>(x => x.Hash == hash);
            return (result.DeletedCount > 0) ? true : false;
        }
    }
}