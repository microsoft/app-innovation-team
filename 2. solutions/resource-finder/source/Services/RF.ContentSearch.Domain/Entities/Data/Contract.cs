using MongoDB.Bson;
using System;

namespace RF.ContentSearch.Domain.Entities.Data
{
    public class Contract
    {
        public ObjectId _id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string address { get; set; }
        public string status { get; set; }
        public string transaction_hash { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
    }
}