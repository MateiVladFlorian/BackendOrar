namespace BackendOrar.Models
{
    public interface IAppSettings
    {
        string key { get; set; }
        string salt { get; set; }
    }
}
