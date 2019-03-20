using MongoDB.Driver;
using RF.ContractDeployment.App.Helpers.Base;
using RF.Contracts.Domain.Entities.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RF.ContractDeployment.App.Helpers.Data
{
    public class ContractDeploymentDataHelper : DBBaseHelper
    {
        public ContractDeploymentDataHelper(MongoDBConnectionInfo databaseInfo) : base(databaseInfo)
        {
        }

        public Contract GetContract(string hash)
        {
            Contract result = null;
            IMongoCollection<Contract> contractCollection = GetMongoDatabase().GetCollection<Contract>(GetMongoDBConnectionInfo().ContractCollection);
            result = contractCollection.AsQueryable<Contract>().Where<Contract>(e => e.transaction_hash == hash).SingleOrDefault();
            return result;
        }

        public async Task UpdateContractAsync(Contract entity)
        {
            DateTime datetimestamp = DateTime.UtcNow;
            IMongoCollection<Contract> contractCollection = GetMongoDatabase().GetCollection<Contract>(GetMongoDBConnectionInfo().ContractCollection);
            entity.updated_date = datetimestamp;
            await contractCollection.ReplaceOneAsync(e => e._id == entity._id, entity);
        }

        public async Task RegisterContractAsync(Contract entity)
        {
            DateTime datetimestamp = DateTime.UtcNow;
            IMongoCollection<Contract> contractCollection = GetMongoDatabase().GetCollection<Contract>(GetMongoDBConnectionInfo().ContractCollection);
            entity.created_date = datetimestamp;
            entity.updated_date = datetimestamp;
            await contractCollection.InsertOneAsync(entity);
        }
    }
}