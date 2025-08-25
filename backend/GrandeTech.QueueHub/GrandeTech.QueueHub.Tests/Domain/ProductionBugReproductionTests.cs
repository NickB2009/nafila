using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Domain.Common.ValueObjects;
using Grande.Fila.API.Domain.Locations;
using System;
using System.Text.Json;

namespace GrandeTech.QueueHub.Tests.Domain
{
    [TestClass]
    public class ProductionBugReproductionTests
    {
        [TestMethod]
        public void ReproduceProductionIssue_ClassicCutsMain()
        {
            // Reproduce the exact scenario from production API
            // Classic Cuts Main: Monday 09:00-23:59, should be open at 21:14
            
            Console.WriteLine("=== Testing Classic Cuts Main Production Issue ===");
            
            // Test the exact business hours that are working in production API
            var monday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("23:59:00"));
            var tuesday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("18:00:00"));
            var wednesday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("18:00:00"));
            var thursday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("18:00:00"));
            var friday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("18:00:00"));
            var saturday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("18:00:00"));
            var sunday = DayBusinessHours.Closed();

            var weeklyHours = WeeklyBusinessHours.Create(monday, tuesday, wednesday, thursday, friday, saturday, sunday);
            
            Console.WriteLine($"Created WeeklyBusinessHours:");
            Console.WriteLine($"Monday: {monday.IsOpen} - {monday.OpenTime} to {monday.CloseTime}");
            
            // Test the exact timezone conversion logic from Location.IsOpen()
            Console.WriteLine("\n=== Testing Timezone Conversion ===");
            var utcNow = DateTime.UtcNow;
            Console.WriteLine($"UTC Now: {utcNow:yyyy-MM-dd HH:mm:ss}");
            
            TimeZoneInfo brazilTimeZone;
            try
            {
                brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                Console.WriteLine($"Brazil TimeZone found: {brazilTimeZone.DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TIMEZONE ERROR: {ex.Message}");
                
                // Try alternative timezone IDs
                Console.WriteLine("Trying alternative timezone IDs...");
                try
                {
                    brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
                    Console.WriteLine($"✅ Alternative timezone found: {brazilTimeZone.DisplayName}");
                }
                catch
                {
                    Console.WriteLine("❌ Alternative timezone also failed");
                    return; // Skip the rest of the test
                }
            }
            
            var brazilTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, brazilTimeZone);
            Console.WriteLine($"Brazil Time: {brazilTime:yyyy-MM-dd HH:mm:ss} ({brazilTime.DayOfWeek})");
            Console.WriteLine($"Brazil TimeOfDay: {brazilTime.TimeOfDay}");
            
            // Test WeeklyHours.IsOpenAt
            Console.WriteLine("\n=== Testing WeeklyHours.IsOpenAt ===");
            var isOpenWeekly = weeklyHours.IsOpenAt(brazilTime);
            Console.WriteLine($"WeeklyHours.IsOpenAt({brazilTime:yyyy-MM-dd HH:mm:ss}): {isOpenWeekly}");
            
            // Test the specific day hours
            var dayHours = weeklyHours.GetDayHours(brazilTime.DayOfWeek);
            Console.WriteLine($"Day hours for {brazilTime.DayOfWeek}: IsOpen={dayHours.IsOpen}, Open={dayHours.OpenTime}, Close={dayHours.CloseTime}");
            
            if (dayHours.IsOpen && dayHours.OpenTime.HasValue && dayHours.CloseTime.HasValue)
            {
                var isOpenDay = dayHours.IsOpenAt(brazilTime.TimeOfDay);
                Console.WriteLine($"DayBusinessHours.IsOpenAt({brazilTime.TimeOfDay}): {isOpenDay}");
                
                // Manual check
                var manualCheck = brazilTime.TimeOfDay >= dayHours.OpenTime.Value && 
                                 brazilTime.TimeOfDay <= dayHours.CloseTime.Value;
                Console.WriteLine($"Manual time check: {manualCheck} (TimeOfDay >= {dayHours.OpenTime.Value} && TimeOfDay <= {dayHours.CloseTime.Value})");
            }
            
            // Create a mock Location object to test the full IsOpen() method
            Console.WriteLine("\n=== Testing Location.IsOpen() ===");
            try
            {
                // This is tricky because Location constructor requires many parameters
                // Let's test the timezone lookup specifically
                TestTimezoneIssue();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Location.IsOpen() test failed: {ex.Message}");
            }
            
            // Expected result: Should be TRUE on Monday at current time
            if (brazilTime.DayOfWeek == DayOfWeek.Monday)
            {
                Assert.IsTrue(isOpenWeekly, "Should be open on Monday between 09:00-23:59");
            }
        }

        [TestMethod]
        public void TestExactProductionApiData()
        {
            // Test with the exact JSON data structure we see in the API
            Console.WriteLine("=== Testing Exact Production API Data Format ===");
            
            // Simulate the exact business hours from API response
            var apiMondayHours = "09:00-23:59";
            Console.WriteLine($"API Monday Hours: {apiMondayHours}");
            
            // Parse like the system would
            var parts = apiMondayHours.Split('-');
            var openTime = TimeSpan.Parse(parts[0] + ":00");
            var closeTime = TimeSpan.Parse(parts[1] + ":00");
            
            Console.WriteLine($"Parsed: Open={openTime}, Close={closeTime}");
            
            var dayHours = DayBusinessHours.Create(openTime, closeTime);
            
            // Test specific times
            var currentBrazilTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow, 
                TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
            
            Console.WriteLine($"Current Brazil Time: {currentBrazilTime.TimeOfDay}");
            
            var isOpen = dayHours.IsOpenAt(currentBrazilTime.TimeOfDay);
            Console.WriteLine($"Is Open: {isOpen}");
            
            if (currentBrazilTime.DayOfWeek == DayOfWeek.Monday)
            {
                Assert.IsTrue(isOpen, "Should be open on Monday at current time");
            }
        }

        private void TestTimezoneIssue()
        {
            Console.WriteLine("\n=== Detailed Timezone Analysis ===");
            
            // Test if the timezone lookup is the issue
            var timeZoneIds = new[]
            {
                "E. South America Standard Time",  // Windows
                "America/Sao_Paulo",              // IANA
                "Brazil/East",                    // IANA alternative
                "BRT"                             // Short name
            };
            
            foreach (var tzId in timeZoneIds)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                    Console.WriteLine($"✅ {tzId}: {localTime:yyyy-MM-dd HH:mm:ss} ({localTime.DayOfWeek})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ {tzId}: {ex.Message}");
                }
            }
            
            // Test what happens on different operating systems
            Console.WriteLine($"\nOperating System: {Environment.OSVersion}");
            Console.WriteLine($"Framework: {Environment.Version}");
        }

        [TestMethod] 
        public void TestBusinessHoursJsonSerialization()
        {
            // Test if JSON serialization/deserialization is the issue
            Console.WriteLine("=== Testing JSON Serialization ===");
            
            var monday = DayBusinessHours.Create(TimeSpan.Parse("09:00:00"), TimeSpan.Parse("23:59:00"));
            var weeklyHours = WeeklyBusinessHours.CreateMondayToSaturday(
                TimeSpan.Parse("09:00:00"), TimeSpan.Parse("23:59:00"));
            
            // Serialize to JSON like EF Core does
            var json = JsonSerializer.Serialize(weeklyHours);
            Console.WriteLine($"Serialized JSON: {json}");
            
            // Deserialize back
            var deserialized = JsonSerializer.Deserialize<WeeklyBusinessHours>(json);
            Console.WriteLine($"Deserialized Monday: IsOpen={deserialized?.Monday.IsOpen}, Open={deserialized?.Monday.OpenTime}, Close={deserialized?.Monday.CloseTime}");
            
            // Test the deserialized version
            if (deserialized != null)
            {
                var currentTime = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow, 
                    TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
                
                var isOpen = deserialized.IsOpenAt(currentTime);
                Console.WriteLine($"Deserialized IsOpenAt: {isOpen}");
                
                if (currentTime.DayOfWeek == DayOfWeek.Monday)
                {
                    Assert.IsTrue(isOpen, "Deserialized object should work correctly");
                }
            }
        }
    }
}