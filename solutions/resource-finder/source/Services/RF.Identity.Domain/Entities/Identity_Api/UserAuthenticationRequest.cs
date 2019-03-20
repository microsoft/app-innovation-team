namespace RF.Identity.Domain.Entities.Identity_Api
{
    public class UserAuthenticationRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}