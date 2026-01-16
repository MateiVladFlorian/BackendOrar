using System.Security.Claims;

namespace BackendOrar.Services
{
    public interface IJwtTokenService
    {
        string GenAccessToken(IEnumerable<Claim> claims);
        string GenRefreshToken();
    }
}
