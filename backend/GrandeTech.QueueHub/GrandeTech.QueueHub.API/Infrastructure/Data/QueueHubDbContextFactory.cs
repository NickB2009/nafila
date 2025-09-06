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
            
            // Use Azure SQL Database connection string for design-time
            var connectionString = "Server=tcp:grande.database.windows.net,1433;Initial Catalog=GrandeTechQueueHub;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";
            optionsBuilder.UseSqlServer(connectionString);
            
            return new QueueHubDbContext(optionsBuilder.Options);
        }
    }
} 