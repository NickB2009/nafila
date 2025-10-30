using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Grande.Fila.API.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for EF migrations - allows migrations without running database
    /// </summary>
    public class QueueHubDbContextFactory : IDesignTimeDbContextFactory<QueueHubDbContext>
    {
        public QueueHubDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QueueHubDbContext>();
            
            // Use a dummy connection string for design-time only
            var connectionString = "Server=localhost;Database=QueueHubDb;User=root;Password=dummy;";
            
            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.Parse("8.0.0"),
                mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null);
                });

            return new QueueHubDbContext(optionsBuilder.Options);
        }
    }
}
