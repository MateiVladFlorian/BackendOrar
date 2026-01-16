#pragma warning disable

namespace BackendOrar.Models
{
    public class JwtSettings : IJwtSettings
    {
        public string HttpServer { get; set; }
        public string HttpsServer { get; set; }
        public string SigningKey { get; set; }
        public int AccessTokenExpirationMinutes { get; set; }
        public int RefreshTokenExpirationDays { get; set; }
        public bool? ValidateLifetime { get; set; } = false;
        public bool? UseHttpsServerAsDefault { get; set; } = false;
    }
}
