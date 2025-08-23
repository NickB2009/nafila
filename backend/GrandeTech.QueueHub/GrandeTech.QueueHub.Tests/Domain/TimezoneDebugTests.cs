using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Domain.Common.ValueObjects;
using System;

namespace GrandeTech.QueueHub.Tests.Domain
{
    [TestClass]
    public class TimezoneDebugTests
    {
        [TestMethod]
        public void TestBrazilTimezoneConversion()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            Console.WriteLine($"UTC Now: {utcNow:yyyy-MM-dd HH:mm:ss}");

            // Act - Mimic the exact logic from Location.IsOpen()
            var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var brazilTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, brazilTimeZone);
            
            Console.WriteLine($"Brazil Time: {brazilTime:yyyy-MM-dd HH:mm:ss} ({brazilTime.DayOfWeek})");
            Console.WriteLine($"Brazil TimeOfDay: {brazilTime.TimeOfDay}");
            
            // Assert timezone is working
            Assert.IsNotNull(brazilTimeZone);
            Assert.IsTrue(brazilTime < utcNow); // Brazil should be behind UTC
        }

        [TestMethod]
        public void TestMondayBusinessHours_WithActualCurrentTime()
        {
            // Arrange - Use actual current Brazil time
            var utcNow = DateTime.UtcNow;
            var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var brazilTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, brazilTimeZone);
            
            // Test each salon's Monday hours with current time
            TestSalonHours("Classic Cuts Main", "09:00", "23:59", brazilTime);
            TestSalonHours("Grande Tech Downtown", "08:00", "23:59", brazilTime);
            TestSalonHours("Grande Tech Mall", "10:00", "23:59", brazilTime);
        }

        private void TestSalonHours(string salonName, string openTimeStr, string closeTimeStr, DateTime currentTime)
        {
            // Arrange
            var openTime = TimeSpan.Parse(openTimeStr + ":00");
            var closeTime = TimeSpan.Parse(closeTimeStr + ":00");
            var businessHours = DayBusinessHours.Create(openTime, closeTime);
            
            Console.WriteLine($"\n--- {salonName} ---");
            Console.WriteLine($"Open: {openTime}, Close: {closeTime}");
            Console.WriteLine($"Current Time: {currentTime:HH:mm:ss} on {currentTime.DayOfWeek}");
            Console.WriteLine($"Current TimeOfDay: {currentTime.TimeOfDay}");
            
            // Act
            var isOpen = businessHours.IsOpenAt(currentTime.TimeOfDay);
            
            Console.WriteLine($"IsOpen: {isOpen}");
            
            // If it's Monday and we're between the hours, it should be open
            if (currentTime.DayOfWeek == DayOfWeek.Monday && 
                currentTime.TimeOfDay >= openTime && 
                currentTime.TimeOfDay <= closeTime)
            {
                Assert.IsTrue(isOpen, $"{salonName} should be open on Monday at {currentTime.TimeOfDay}");
            }
        }

        [TestMethod]
        public void TestSpecific2359ClosingTime()
        {
            // Arrange
            var openTime = TimeSpan.Parse("09:00:00");
            var closeTime = TimeSpan.Parse("23:59:00");
            var businessHours = DayBusinessHours.Create(openTime, closeTime);
            
            // Test specific times
            var testTimes = new[]
            {
                TimeSpan.Parse("21:08:00"), // Current Brazil time
                TimeSpan.Parse("09:00:00"), // Opening
                TimeSpan.Parse("23:59:00"), // Closing
                TimeSpan.Parse("23:58:00"), // Just before closing
                TimeSpan.Parse("00:00:00"), // Midnight (should be closed)
                TimeSpan.Parse("08:59:00")  // Just before opening
            };

            foreach (var testTime in testTimes)
            {
                var isOpen = businessHours.IsOpenAt(testTime);
                Console.WriteLine($"Time {testTime}: IsOpen = {isOpen}");
                
                // Specific assertions
                if (testTime >= openTime && testTime <= closeTime)
                {
                    Assert.IsTrue(isOpen, $"Should be open at {testTime}");
                }
                else
                {
                    Assert.IsFalse(isOpen, $"Should be closed at {testTime}");
                }
            }
        }
    }
}