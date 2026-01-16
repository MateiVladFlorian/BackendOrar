#pragma warning disable
namespace BackendOrar.Models
{
    public class AppSettings : IAppSettings
    {
        public string key { get; set; }
        public string salt { get; set; }
    }
}
