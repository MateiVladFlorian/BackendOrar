using BackendOrar.Data;
using BackendOrar.Models;
using System.Security.Principal;

namespace BackendOrar.Services
{
    public interface IUserService
    {
        Task<User?> GetAccountAsync(int id);
        Task<bool> VerifyAccount(string address);
        Task<int?> GetAccountIdFromAccessTokenAsync(string accessToken);
        Task<User?> GetAccountFromAccessTokenAsync(string accessToken);
        Task<UserResponseModel> SignInAsync(UserRequestModel accountRequestModel);
        Task<UserResponseModel> RegisterAsync(UserRequestModel accountRequestModel);
        Task<bool> IsAccessTokenExpired(string accessToken);
        Task<UserResponseModel> SendWelcomeAsync(int id);
    }
}
