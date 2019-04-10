using MongoDB.Bson;
using System;

namespace RF.Identity.Domain.Entities.Data
{
    public class User
    {
        public ObjectId _id { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string dataenc { get; set; }
        public string datakey { get; set; }
        public string identicon { get; set; }
        public DateTime created_date { get; set; }
        public DateTime updated_date { get; set; }
    }
}