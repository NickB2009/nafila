using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Grande.Fila.API.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Grande.Fila.API.Tests.Integration;
using Grande.Fila.API.Infrastructure.Repositories.Bogus;

namespace Grande.Fila.API.Tests.Integration.Controllers
{
    [TestClass]
    [TestCategory("Integration")]
    public class ProductionDataDebugTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static BogusLocationRepository _locationRepository;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddTransient<BogusLocationRepository>();
                    });
                });

            _locationRepository = _factory.Services.GetRequiredService<BogusLocationRepository>();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task DebugLocationOpenStatus_WithRealDatabaseData()
        {
            // Arrange
            var dbContext = _factory.Services.GetRequiredService<QueueHubDbContext>();
            
            // Get all locations from the actual database
            var locations = await dbContext.Locations
                .Where(l => l.IsActive)
                .ToListAsync();

            Console.WriteLine($"Found {locations.Count} active locations");
            Console.WriteLine($"Current UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

            // Test each location
            foreach (var location in locations)
            {
                Console.WriteLine($"\n--- {location.Name} ---");
                Console.WriteLine($"Location ID: {location.Id}");
                Console.WriteLine($"IsActive: {location.IsActive}");
                
                // Check the raw WeeklyBusinessHours JSON data
                var weeklyHoursJson = GetWeeklyHoursJson(dbContext, location.Id);
                Console.WriteLine($"WeeklyBusinessHours JSON: {weeklyHoursJson}");
                
                // Get the Brazil time manually
                var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                var brazilTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
                Console.WriteLine($"Brazil Time: {brazilTime:yyyy-MM-dd HH:mm:ss} ({brazilTime.DayOfWeek})");
                Console.WriteLine($"Brazil TimeOfDay: {brazilTime.TimeOfDay}");
                
                // Test the actual Location.IsOpen() method
                var isOpen = location.IsOpen();
                Console.WriteLine($"Location.IsOpen(): {isOpen}");
                
                // Test the WeeklyHours.IsOpenAt directly
                if (location.WeeklyHours != null)
                {
                    var isOpenDirect = location.WeeklyHours.IsOpenAt(brazilTime);
                    Console.WriteLine($"WeeklyHours.IsOpenAt(): {isOpenDirect}");
                    
                    // Get today's day hours
                    var todayHours = location.WeeklyHours.GetDayHours(brazilTime.DayOfWeek);
                    Console.WriteLine($"Today's Hours - IsOpen: {todayHours.IsOpen}, Open: {todayHours.OpenTime}, Close: {todayHours.CloseTime}");
                    
                    if (todayHours.IsOpen && todayHours.OpenTime.HasValue && todayHours.CloseTime.HasValue)
                    {
                        var isOpenTime = todayHours.IsOpenAt(brazilTime.TimeOfDay);
                        Console.WriteLine($"DayBusinessHours.IsOpenAt(): {isOpenTime}");
                    }
                }
                else
                {
                    Console.WriteLine("WeeklyHours is NULL!");
                }
                
                // Expected result based on API data
                var expectedOpen = brazilTime.DayOfWeek == DayOfWeek.Monday && 
                                 brazilTime.TimeOfDay >= TimeSpan.Parse("08:00:00") && 
                                 brazilTime.TimeOfDay <= TimeSpan.Parse("23:59:00");
                
                Console.WriteLine($"Expected IsOpen (Monday 08:00-23:59): {expectedOpen}");
                
                if (expectedOpen && !isOpen)
                {
                    Console.WriteLine("🚨 MISMATCH: Should be open but showing as closed!");
                }
            }
        }

        private string GetWeeklyHoursJson(QueueHubDbContext dbContext, Guid locationId)
        {
            try
            {
                // Get the raw JSON from the database
                var query = $"SELECT WeeklyBusinessHours FROM Locations WHERE Id = '{locationId}'";
                var connection = dbContext.Database.GetDbConnection();
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = query;
                var result = command.ExecuteScalar();
                connection.Close();
                
                return result?.ToString() ?? "NULL";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        [TestMethod]
        public async Task CheckActualDatabaseData()
        {
            // Check what's actually in the database
            var dbContext = _factory.Services.GetRequiredService<QueueHubDbContext>();
            
            Console.WriteLine("Checking raw database data...");
            
            try
            {
                var locations = await dbContext.Locations.Take(3).ToListAsync();
                
                foreach (var location in locations)
                {
                    Console.WriteLine($"\n--- {location.Name} ---");
                    Console.WriteLine($"IsActive: {location.IsActive}");
                    
                    // Test Location.IsOpen() directly
                    var isOpen = location.IsOpen();
                    Console.WriteLine($"Location.IsOpen(): {isOpen}");
                    
                    // Check WeeklyHours property
                    if (location.WeeklyHours != null)
                    {
                        var mondayHours = location.WeeklyHours.Monday;
                        Console.WriteLine($"Monday Hours - IsOpen: {mondayHours.IsOpen}, Open: {mondayHours.OpenTime}, Close: {mondayHours.CloseTime}");
                        
                        var brazilTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, 
                            TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
                        
                        if (brazilTime.DayOfWeek == DayOfWeek.Monday && mondayHours.IsOpen)
                        {
                            var isOpenAtCurrentTime = mondayHours.IsOpenAt(brazilTime.TimeOfDay);
                            Console.WriteLine($"Should be open now (Monday {brazilTime.TimeOfDay}): {isOpenAtCurrentTime}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("WeeklyHours is NULL!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}