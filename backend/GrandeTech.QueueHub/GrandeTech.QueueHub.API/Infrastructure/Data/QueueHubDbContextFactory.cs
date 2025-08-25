using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Grande.Fila.API.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for QueueHubDbContext to enable EF Core migrations
    /// </summary>
    public class QueueHubDbContextFactory : IDesignTimeDbContextFactory<QueueHubDbContext>
    {
        public QueueHubDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueueHubDbContext>();
            
            // Build configuration for design-time
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            
            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("AzureSqlConnection");
            
            // Fallback to local development connection if Azure connection not available
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=QueueHubDb;Integrated Security=True";
            }
            
            optionsBuilder.UseSqlServer(connectionString);
            
            return new QueueHubDbContext(optionsBuilder.Options);
        }
    }
} 