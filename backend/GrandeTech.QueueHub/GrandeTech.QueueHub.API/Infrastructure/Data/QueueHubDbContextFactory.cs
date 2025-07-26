using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
            
            // Use Docker SQL Server connection string for design-time
            var connectionString = "Server=localhost,1433;Database=GrandeTechQueueHub;User Id=sa;Password=DevPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;Encrypt=False;";
            optionsBuilder.UseSqlServer(connectionString);
            
            return new QueueHubDbContext(optionsBuilder.Options);
        }
    }
} 