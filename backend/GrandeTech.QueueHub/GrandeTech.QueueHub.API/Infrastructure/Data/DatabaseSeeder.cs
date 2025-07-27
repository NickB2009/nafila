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

                // Always clear and reseed - don't skip if data exists
                _logger.LogInformation("Force clearing and reseeding database...");

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
                // Remove comments first
                var lines = sqlScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                var cleanLines = lines.Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("--"));
                var cleanScript = string.Join(Environment.NewLine, cleanLines);

                // Split by semicolons but be more careful about multiline statements
                var statements = new List<string>();
                var currentStatement = new System.Text.StringBuilder();
                
                var scriptLines = cleanScript.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                
                foreach (var line in scriptLines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                        continue;

                    currentStatement.AppendLine(line);

                    // Check if this line ends a statement (contains semicolon)
                    if (trimmedLine.EndsWith(";"))
                    {
                        var statement = currentStatement.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(statement))
                        {
                            statements.Add(statement);
                        }
                        currentStatement.Clear();
                    }
                }

                // Add any remaining statement
                if (currentStatement.Length > 0)
                {
                    var statement = currentStatement.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        statements.Add(statement);
                    }
                }

                _logger.LogInformation("Found {Count} SQL statements to execute", statements.Count);

                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();
                    if (string.IsNullOrEmpty(trimmedStatement))
                        continue;

                    try
                    {
                        _logger.LogInformation("Executing SQL: {Statement}", 
                            trimmedStatement.Length > 100 ? trimmedStatement.Substring(0, 100) + "..." : trimmedStatement);
                        
                        await _context.Database.ExecuteSqlRawAsync(trimmedStatement);
                        
                        _logger.LogInformation("Successfully executed SQL statement");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing SQL statement: {Error}. Statement: {Statement}", 
                            ex.Message, trimmedStatement.Length > 200 ? trimmedStatement.Substring(0, 200) + "..." : trimmedStatement);
                        // Don't continue on error - we want to know about failures
                        throw;
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