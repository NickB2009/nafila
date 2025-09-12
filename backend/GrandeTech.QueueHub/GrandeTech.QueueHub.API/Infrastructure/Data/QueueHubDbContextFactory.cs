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
            
            // Use MySQL Database connection string for design-time
            var connectionString = "Server=localhost;Database=QueueHubDb;User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            
            return new QueueHubDbContext(optionsBuilder.Options);
        }
    }
} 