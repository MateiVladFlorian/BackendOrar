using BackendOrar.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BackendOrar.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        readonly IJwtSettings jwtSettings;
        string usedServer = string.Empty;

        public JwtTokenService(IJwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;

            usedServer = (jwtSettings.UseHttpsServerAsDefault != null && jwtSettings.UseHttpsServerAsDefault.Value)
                ? jwtSettings.HttpsServer : jwtSettings.HttpServer;
        }

        /* Generates a new access token based on the last one. */
        public string GenAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: usedServer,
                audience: usedServer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /* Use secure random number generator to fill a new refresh token. */
        public string GenRefreshToken()
        {
            var buffer = new byte[32];
            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(buffer);
                return Convert.ToBase64String(buffer);
            }
        }
    }
}
