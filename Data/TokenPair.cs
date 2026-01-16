using System.Security.Principal;
#pragma warning disable

namespace BackendOrar.Data
{
    public class TokenPair
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
