using Microsoft.EntityFrameworkCore;

namespace BackendOrar.Data
{
    public class OrarContextFactory
    {
        public OrarContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            var builderContext = new DbContextOptionsBuilder<OrarContext>();
            var connectionString = configurationRoot.GetConnectionString("BackendOrarDB");

            builderContext.UseNpgsql(connectionString);
            return new OrarContext(builderContext.Options);
        }
    }
}
