using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Grande.Fila.API.Infrastructure.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Grande.Fila.Tests.Infrastructure;

/// <summary>
/// Base class for MySQL-specific unit tests
/// Provides a clean MySQL database context for each test
/// </summary>
public abstract class MySqlTestBase : IDisposable
{
    protected QueueHubDbContext DbContext { get; private set; }
    protected ServiceProvider ServiceProvider { get; private set; }
    private readonly string _databaseName;

    protected MySqlTestBase()
    {
        _databaseName = $"TestDb_{Guid.NewGuid():N}";
        SetupDatabase();
    }

    private void SetupDatabase()
    {
        var services = new ServiceCollection();
        
        // Add MySQL DbContext with test configuration
        services.AddDbContext<QueueHubDbContext>(options =>
        {
            var connectionString = $"Server=localhost;Database={_databaseName};User=root;Password=DevPassword123!;Port=3306;CharSet=utf8mb4;SslMode=None;ConnectionTimeout=30;";
            
            options.UseMySql(connectionString, ServerVersion.Create(8, 0, 0, ServerType.MySql), mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(maxRetryCount: 1, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                mySqlOptions.EnableStringComparisonTranslations();
            });

            // Enable sensitive data logging for tests
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<QueueHubDbContext>();
        
        // Create database and run migrations
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Clean up test data after each test
    /// </summary>
    protected virtual void CleanupTestData()
    {
        if (DbContext != null)
        {
            // Clear all data from all tables
            DbContext.Organizations.RemoveRange(DbContext.Organizations);
            DbContext.Locations.RemoveRange(DbContext.Locations);
            DbContext.Queues.RemoveRange(DbContext.Queues);
            DbContext.QueueEntries.RemoveRange(DbContext.QueueEntries);
            DbContext.Customers.RemoveRange(DbContext.Customers);
            DbContext.StaffMembers.RemoveRange(DbContext.StaffMembers);
            DbContext.SubscriptionPlans.RemoveRange(DbContext.SubscriptionPlans);
            
            DbContext.SaveChanges();
        }
    }

    public virtual void Dispose()
    {
        try
        {
            // Clean up test data
            CleanupTestData();
            
            // Drop the test database
            if (DbContext != null)
            {
                DbContext.Database.EnsureDeleted();
                DbContext.Dispose();
            }
        }
        catch (Exception ex)
        {
            // Log cleanup errors but don't fail the test
            Console.WriteLine($"Error during test cleanup: {ex.Message}");
        }
        finally
        {
            ServiceProvider?.Dispose();
        }
    }
}