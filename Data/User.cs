#pragma warning disable

using System.ComponentModel.DataAnnotations.Schema;

namespace BackendOrar.Data
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public int UserRole { get; set; }
        public bool? IsActive { get; set; }

        [ForeignKey("UserId")]
        public virtual ICollection<TokenPair> TokenPairs { get; set; }
    }
}
