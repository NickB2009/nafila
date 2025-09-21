using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{
    /// <summary>
    /// Value object representing business hours for each day of the week
    /// </summary>
    public record WeeklyBusinessHours : ValueObject
    {
        [JsonPropertyName("monday")]
        public DayBusinessHours Monday { get; }
        
        [JsonPropertyName("tuesday")]
        public DayBusinessHours Tuesday { get; }
        
        [JsonPropertyName("wednesday")]
        public DayBusinessHours Wednesday { get; }
        
        [JsonPropertyName("thursday")]
        public DayBusinessHours Thursday { get; }
        
        [JsonPropertyName("friday")]
        public DayBusinessHours Friday { get; }
        
        [JsonPropertyName("saturday")]
        public DayBusinessHours Saturday { get; }
        
        [JsonPropertyName("sunday")]
        public DayBusinessHours Sunday { get; }

        // For EF Core
        private WeeklyBusinessHours() 
        {
            Monday = DayBusinessHours.Closed();
            Tuesday = DayBusinessHours.Closed();
            Wednesday = DayBusinessHours.Closed();
            Thursday = DayBusinessHours.Closed();
            Friday = DayBusinessHours.Closed();
            Saturday = DayBusinessHours.Closed();
            Sunday = DayBusinessHours.Closed();
        }

        [JsonConstructor]
        private WeeklyBusinessHours(
            DayBusinessHours? monday = null,
            DayBusinessHours? tuesday = null,
            DayBusinessHours? wednesday = null,
            DayBusinessHours? thursday = null,
            DayBusinessHours? friday = null,
            DayBusinessHours? saturday = null,
            DayBusinessHours? sunday = null)
        {
            Monday = monday ?? DayBusinessHours.Closed();
            Tuesday = tuesday ?? DayBusinessHours.Closed();
            Wednesday = wednesday ?? DayBusinessHours.Closed();
            Thursday = thursday ?? DayBusinessHours.Closed();
            Friday = friday ?? DayBusinessHours.Closed();
            Saturday = saturday ?? DayBusinessHours.Closed();
            Sunday = sunday ?? DayBusinessHours.Closed();
        }

        /// <summary>
        /// Creates weekly business hours with the same hours for all days
        /// </summary>
        public static WeeklyBusinessHours CreateUniform(TimeSpan openTime, TimeSpan closeTime)
        {
            var dayHours = DayBusinessHours.Create(openTime, closeTime);
            return new WeeklyBusinessHours(
                dayHours, dayHours, dayHours, dayHours, dayHours, dayHours, dayHours);
        }

        /// <summary>
        /// Creates weekly business hours with same hours Monday-Saturday, closed Sunday
        /// </summary>
        public static WeeklyBusinessHours CreateMondayToSaturday(TimeSpan openTime, TimeSpan closeTime)
        {
            var workDayHours = DayBusinessHours.Create(openTime, closeTime);
            var closedSunday = DayBusinessHours.Closed();
            
            return new WeeklyBusinessHours(
                workDayHours, workDayHours, workDayHours, workDayHours, workDayHours, workDayHours, closedSunday);
        }



        /// <summary>
        /// Creates custom weekly business hours
        /// </summary>
        public static WeeklyBusinessHours Create(
            DayBusinessHours monday,
            DayBusinessHours tuesday,
            DayBusinessHours wednesday,
            DayBusinessHours thursday,
            DayBusinessHours friday,
            DayBusinessHours saturday,
            DayBusinessHours sunday)
        {
            return new WeeklyBusinessHours(monday, tuesday, wednesday, thursday, friday, saturday, sunday);
        }

        /// <summary>
        /// Gets business hours for a specific day of the week
        /// </summary>
        public DayBusinessHours GetDayHours(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => Monday,
                DayOfWeek.Tuesday => Tuesday,
                DayOfWeek.Wednesday => Wednesday,
                DayOfWeek.Thursday => Thursday,
                DayOfWeek.Friday => Friday,
                DayOfWeek.Saturday => Saturday,
                DayOfWeek.Sunday => Sunday,
                _ => throw new ArgumentException($"Invalid day of week: {dayOfWeek}")
            };
        }

        /// <summary>
        /// Checks if the location is open at the current time
        /// </summary>
        public bool IsOpenAt(DateTime dateTime)
        {
            var dayHours = GetDayHours(dateTime.DayOfWeek);
            return dayHours.IsOpenAt(dateTime.TimeOfDay);
        }

        /// <summary>
        /// Gets business hours as a dictionary (for API responses)
        /// </summary>
        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                ["monday"] = Monday.ToDisplayString(),
                ["tuesday"] = Tuesday.ToDisplayString(),
                ["wednesday"] = Wednesday.ToDisplayString(),
                ["thursday"] = Thursday.ToDisplayString(),
                ["friday"] = Friday.ToDisplayString(),
                ["saturday"] = Saturday.ToDisplayString(),
                ["sunday"] = Sunday.ToDisplayString()
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Monday;
            yield return Tuesday;
            yield return Wednesday;
            yield return Thursday;
            yield return Friday;
            yield return Saturday;
            yield return Sunday;
        }
    }

    /// <summary>
    /// Value object representing business hours for a single day
    /// </summary>
    public record DayBusinessHours : ValueObject
    {
        [JsonPropertyName("isOpen")]
        public bool IsOpen { get; }
        
        [JsonPropertyName("start")]
        public TimeSpan? OpenTime { get; }
        
        [JsonPropertyName("end")]
        public TimeSpan? CloseTime { get; }

        // For EF Core
        private DayBusinessHours()
        {
            IsOpen = false;
            OpenTime = null;
            CloseTime = null;
        }

        [JsonConstructor]
        private DayBusinessHours(bool isOpen, TimeSpan? openTime, TimeSpan? closeTime)
        {
            IsOpen = isOpen;
            OpenTime = openTime;
            CloseTime = closeTime;
        }

        /// <summary>
        /// Creates business hours for an open day
        /// </summary>
        public static DayBusinessHours Create(TimeSpan openTime, TimeSpan closeTime)
        {
            if (closeTime < openTime)
                throw new ArgumentException("Close time cannot be before open time");

            return new DayBusinessHours(true, openTime, closeTime);
        }

        /// <summary>
        /// Creates business hours for a closed day
        /// </summary>
        public static DayBusinessHours Closed()
        {
            return new DayBusinessHours(false, null, null);
        }

        /// <summary>
        /// Checks if the business is open at a specific time
        /// </summary>
        public bool IsOpenAt(TimeSpan time)
        {
            if (!IsOpen || !OpenTime.HasValue || !CloseTime.HasValue)
                return false;

            var openTime = OpenTime.Value;
            var closeTime = CloseTime.Value;

            // Handle midnight closing (00:00:00) as end of day
            if (closeTime == TimeSpan.Zero)
            {
                // Open until midnight means open from openTime through 23:59:59
                return time >= openTime;
            }

            // Handle closing after midnight (e.g., 02:00 for 2 AM)
            if (closeTime < openTime)
            {
                // Business spans midnight (e.g., 22:00-02:00)
                return time >= openTime || time <= closeTime;
            }

            // Normal case: open and close within same day
            return time >= openTime && time <= closeTime;
        }

        /// <summary>
        /// Returns a display string for the hours
        /// </summary>
        public string ToDisplayString()
        {
            if (!IsOpen)
                return "closed";

            return $"{OpenTime:hh\\:mm}-{CloseTime:hh\\:mm}";
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return IsOpen;
            yield return OpenTime ?? TimeSpan.Zero;
            yield return CloseTime ?? TimeSpan.Zero;
        }
    }
}