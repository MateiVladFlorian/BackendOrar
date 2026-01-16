namespace BackendOrar.Models
{
    public interface IJwtSettings
    {
        string HttpServer { get; set; }
        string HttpsServer { get; set; }
        string SigningKey { get; set; }
        int AccessTokenExpirationMinutes { get; set; }
        int RefreshTokenExpirationDays { get; set; }
        bool? ValidateLifetime { get; set; }
        bool? UseHttpsServerAsDefault { get; set; }
    }
}
