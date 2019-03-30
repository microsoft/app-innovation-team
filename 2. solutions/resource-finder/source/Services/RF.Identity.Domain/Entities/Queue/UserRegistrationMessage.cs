namespace RF.Identity.Domain.Entities.Queue
{
    public class UserRegistrationMessage
    {
        public string fullname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}