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
                // Check if we're in production and seeding is not explicitly enabled
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var isProduction = environment == "Production";
                var forceReseed = _configuration.GetValue<bool>("Database:ForceReseed", false);
                
                if (isProduction && !forceReseed)
                {
                    _logger.LogWarning("Database seeding skipped in production environment. Use ForceReseed=true to override.");
                    return;
                }

                var hasUserData = await _context.Users.AnyAsync();
                if (hasUserData && !forceReseed)
                {
                    _logger.LogInformation("Database already contains data. Skipping seeding.");
                    return;
                }

                if (forceReseed)
                {
                    _logger.LogInformation("Force reseed enabled. Clearing existing data...");
                    await ClearExistingData();
                }

                _logger.LogInformation("Starting database seeding...");

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
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }

        private async Task ClearExistingData()
        {
            // Clear data in proper order due to foreign key constraints
            _context.QueueEntries.RemoveRange(_context.QueueEntries);
            _context.Queues.RemoveRange(_context.Queues);
            _context.StaffMembers.RemoveRange(_context.StaffMembers);
            _context.ServicesOffered.RemoveRange(_context.ServicesOffered);
            _context.Customers.RemoveRange(_context.Customers);
            _context.Locations.RemoveRange(_context.Locations);
            _context.Organizations.RemoveRange(_context.Organizations);
            _context.SubscriptionPlans.RemoveRange(_context.SubscriptionPlans);
            _context.Users.RemoveRange(_context.Users);
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Existing data cleared successfully.");
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