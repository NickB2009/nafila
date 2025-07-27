using Microsoft.EntityFrameworkCore;

namespace Grande.Fila.API.Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly QueueHubDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseSeeder(QueueHubDbContext context, ILogger<DatabaseSeeder> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                // Check if we should force reseed (useful for development)
                var forceReseed = _configuration.GetValue<bool>("Database:ForceReseed", false);

                if (!forceReseed)
                {
                    // Check if data already exists
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
                }
                else
                {
                    _logger.LogInformation("Force reseed enabled. Clearing and reseeding database...");
                }

                // Read from file system
                var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "insert_test_data.sql");
                if (File.Exists(sqlFilePath))
                {
                    var fileContent = await File.ReadAllTextAsync(sqlFilePath);
                    await ExecuteSqlScript(fileContent);
                }
                else
                {
                    _logger.LogWarning("Could not find insert_test_data.sql file for seeding");
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

                _logger.LogInformation("Executing {Count} SQL statements for database seeding", statements.Count);

                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();
                    if (string.IsNullOrEmpty(trimmedStatement))
                        continue;

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(trimmedStatement);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing SQL statement: {Error}", ex.Message);
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