namespace RF.Identity.Domain.Entities.Identity_Api
{
    public class UserRegistrationRequest
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}