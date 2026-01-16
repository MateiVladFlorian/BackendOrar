using BackendOrar.Definitions;

namespace BackendOrar.Models
{
    public class UserResponseModel
    {
        public int? id { get; set; }
        public string? fullName { get; set; }
        public string? address { get; set; }
        public string? accessToken { get; set; }
        public string? refreshToken { get; set; }
        public UserRole UserRole { get; set; }
        public int? status { get; set; }
    }
}
