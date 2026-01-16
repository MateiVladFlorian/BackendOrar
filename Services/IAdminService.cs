namespace BackendOrar.Services
{
    public interface IAdminService
    {
        Task<int> SendEmail(string to, string subject, string body);
    }
}
