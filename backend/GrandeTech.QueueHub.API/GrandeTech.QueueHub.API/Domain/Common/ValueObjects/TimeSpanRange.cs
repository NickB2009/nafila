using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{
    public record TimeSpanRange : ValueObject
    {
        public TimeSpan Start { get; }
        public TimeSpan End { get; }

        private TimeSpanRange(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        public static TimeSpanRange Create(TimeSpan start, TimeSpan end)
        {
            if (end < start)
                throw new ArgumentException("End time cannot be before start time");

            return new TimeSpanRange(start, end);
        }

        public static TimeSpanRange Create(string start, string end)
        {
            if (!TimeSpan.TryParse(start, out var startTime))
                throw new ArgumentException("Invalid start time format. Use HH:MM:SS", nameof(start));

            if (!TimeSpan.TryParse(end, out var endTime))
                throw new ArgumentException("Invalid end time format. Use HH:MM:SS", nameof(end));

            return Create(startTime, endTime);
        }

        public TimeSpan Duration => End - Start;

        public bool Contains(TimeSpan time)
        {
            return time >= Start && time <= End;
        }

        public bool Overlaps(TimeSpanRange other)
        {
            return Start < other.End && End > other.Start;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }

        public override string ToString()
        {
            return $"{Start:hh\\:mm} - {End:hh\\:mm}";
        }
    }
}
