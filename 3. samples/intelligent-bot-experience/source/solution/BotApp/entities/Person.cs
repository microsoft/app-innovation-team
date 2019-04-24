using MongoDB.Bson;

namespace BotApp
{
    public class Person
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Hash { get; set; }
        public string FaceAPIFaceId { get; set; }
        public string CreatedDate { get; set; }
        public string PasscodeHash { get; set; }
    }
}