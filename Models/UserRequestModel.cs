namespace BackendOrar.Models
{
    public class UserRequestModel
    {
        public int? Id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? fullName { get; set; }

        public string? address { get; set; }
        public string? password { get; set; }
        public string? confirmPassword { get; set; }
        public string? phoneNumber { get; set; }

        public string? description { get; set; }
        public int? userRole { get; set; }
    }
}
