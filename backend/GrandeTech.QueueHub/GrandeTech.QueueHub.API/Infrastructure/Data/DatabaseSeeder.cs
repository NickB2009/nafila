using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Grande.Fila.API.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly QueueHubDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(QueueHubDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                // Check if data already exists by checking if any tables have data
                try
                {
                    var hasUserData = await _context.Users.AnyAsync();
                    if (hasUserData)
                    {
                        _logger.LogInformation("Database already contains data. Skipping seeding.");
                        return;
                    }
                }
                catch (Exception)
                {
                    _logger.LogInformation("Tables might not exist yet, proceeding with seeding...");
                }

                // Read from file system
                var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "insert_test_data.sql");
                if (File.Exists(sqlFilePath))
                {
                    var fileContent = await File.ReadAllTextAsync(sqlFilePath);
                    await ExecuteSqlScript(fileContent);
                    return;
                }
                else
                {
                    _logger.LogWarning("Could not find insert_test_data.sql file for seeding");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }

        private async Task ExecuteSqlScript(string sqlScript)
        {
            try
            {
                // Split script by semicolons and execute each statement
                var statements = sqlScript.Split(new[] { ";\r\n", ";\n", ";" }, 
                    StringSplitOptions.RemoveEmptyEntries);

                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();
                    if (string.IsNullOrEmpty(trimmedStatement) || trimmedStatement.StartsWith("--"))
                        continue;

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(trimmedStatement);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error executing SQL statement: {Error}. Statement: {Statement}", 
                            ex.Message, trimmedStatement.Substring(0, Math.Min(100, trimmedStatement.Length)));
                        // Continue with other statements
                    }
                }

                _logger.LogInformation("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute SQL script during seeding");
                throw;
            }
        }
    }
} 