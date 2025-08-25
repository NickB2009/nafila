using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grande.Fila.API.Domain.Common.ValueObjects;
using System;

namespace GrandeTech.QueueHub.Tests.Domain
{
    [TestClass]
    public class BusinessHoursTests
    {
        [TestMethod]
        public void DayBusinessHours_ShouldBeOpen_At2108_WhenClosingAt2359()
        {
            // Arrange
            var openTime = TimeSpan.Parse("09:00:00");
            var closeTime = TimeSpan.Parse("23:59:00");
            var businessHours = DayBusinessHours.Create(openTime, closeTime);
            var currentTime = TimeSpan.Parse("21:08:00"); // 9:08 PM

            // Act
            var isOpen = businessHours.IsOpenAt(currentTime);

            // Assert
            Assert.IsTrue(isOpen, "Business should be open at 21:08 when closing at 23:59");
        }

        [TestMethod]
        public void DayBusinessHours_ShouldBeOpen_At0800_WhenClosingAt2359()
        {
            // Arrange
            var openTime = TimeSpan.Parse("08:00:00");
            var closeTime = TimeSpan.Parse("23:59:00");
            var businessHours = DayBusinessHours.Create(openTime, closeTime);
            var currentTime = TimeSpan.Parse("08:00:00"); // Opening time

            // Act
            var isOpen = businessHours.IsOpenAt(currentTime);

            // Assert
            Assert.IsTrue(isOpen, "Business should be open at opening time");
        }

        [TestMethod]
        public void DayBusinessHours_ShouldBeClosed_AfterClosingTime()
        {
            // Arrange
            var openTime = TimeSpan.Parse("09:00:00");
            var closeTime = TimeSpan.Parse("18:00:00");
            var businessHours = DayBusinessHours.Create(openTime, closeTime);
            var currentTime = TimeSpan.Parse("19:00:00"); // After closing

            // Act
            var isOpen = businessHours.IsOpenAt(currentTime);

            // Assert
            Assert.IsFalse(isOpen, "Business should be closed after closing time");
        }

        [TestMethod]
        public void WeeklyBusinessHours_IsOpenAt_Monday2108_ShouldReturnTrue()
        {
            // Arrange - Simulate business hours similar to API data
            var openTime = TimeSpan.Parse("09:00:00");
            var closeTime = TimeSpan.Parse("23:59:00");
            var weeklyHours = WeeklyBusinessHours.CreateMondayToSaturday(openTime, closeTime);
            
            // Monday 21:08 (9:08 PM) - Current Brazil time
            var mondayEvening = new DateTime(2025, 1, 27, 21, 8, 0); // Monday

            // Act
            var isOpen = weeklyHours.IsOpenAt(mondayEvening);

            // Assert
            Assert.IsTrue(isOpen, "Should be open on Monday at 21:08 when business closes at 23:59");
        }

        [TestMethod]
        public void WeeklyBusinessHours_IsOpenAt_Saturday2108_ShouldReturnTrue()
        {
            // Arrange
            var openTime = TimeSpan.Parse("10:00:00");
            var closeTime = TimeSpan.Parse("23:59:00");
            var weeklyHours = WeeklyBusinessHours.CreateMondayToSaturday(openTime, closeTime);
            
            // Saturday 21:08 (9:08 PM)
            var saturdayEvening = new DateTime(2025, 2, 1, 21, 8, 0); // Saturday

            // Act
            var isOpen = weeklyHours.IsOpenAt(saturdayEvening);

            // Assert
            Assert.IsTrue(isOpen, "Should be open on Saturday at 21:08 when business closes at 23:59");
        }

        [TestMethod]
        public void WeeklyBusinessHours_IsOpenAt_Sunday_ShouldReturnFalse()
        {
            // Arrange
            var openTime = TimeSpan.Parse("09:00:00");
            var closeTime = TimeSpan.Parse("23:59:00");
            var weeklyHours = WeeklyBusinessHours.CreateMondayToSaturday(openTime, closeTime);
            
            // Sunday (closed day)
            var sunday = new DateTime(2025, 2, 2, 15, 0, 0); // Sunday afternoon

            // Act
            var isOpen = weeklyHours.IsOpenAt(sunday);

            // Assert
            Assert.IsFalse(isOpen, "Should be closed on Sunday");
        }

        [TestMethod]
        public void Location_IsOpen_ShouldReturnCorrectStatus()
        {
            // This will test the full chain including timezone conversion
            // We'll add this after fixing the basic issue
        }
    }
}