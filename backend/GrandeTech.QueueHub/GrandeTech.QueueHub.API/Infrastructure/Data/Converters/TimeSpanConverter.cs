using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Grande.Fila.API.Infrastructure.Data.Converters
{
    /// <summary>
    /// Custom JSON converter for TimeSpan that handles "HH:mm" format
    /// </summary>
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }

            var timeString = reader.GetString();
            if (string.IsNullOrEmpty(timeString))
            {
                return TimeSpan.Zero;
            }

            // Handle "HH:mm" format
            if (TimeSpan.TryParseExact(timeString, "hh\\:mm", null, out var timeSpan))
            {
                return timeSpan;
            }

            // Fallback to standard parsing
            if (TimeSpan.TryParse(timeString, out timeSpan))
            {
                return timeSpan;
            }

            throw new JsonException($"Unable to parse TimeSpan from: {timeString}");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("hh\\:mm"));
        }
    }

    /// <summary>
    /// Custom JSON converter for nullable TimeSpan that handles "HH:mm" format
    /// </summary>
    public class NullableTimeSpanConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }

            var timeString = reader.GetString();
            if (string.IsNullOrEmpty(timeString))
            {
                return null;
            }

            // Handle "HH:mm" format
            if (TimeSpan.TryParseExact(timeString, "hh\\:mm", null, out var timeSpan))
            {
                return timeSpan;
            }

            // Fallback to standard parsing
            if (TimeSpan.TryParse(timeString, out timeSpan))
            {
                return timeSpan;
            }

            throw new JsonException($"Unable to parse TimeSpan from: {timeString}");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString("hh\\:mm"));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
