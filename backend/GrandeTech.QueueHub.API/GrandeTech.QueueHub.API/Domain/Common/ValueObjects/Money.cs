using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{    
    [JsonConverter(typeof(MoneyJsonConverter))]
    public record Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = string.Empty;

        // Parameterless constructor for Entity Framework and JSON deserialization
        private Money()
        {
        }

        private Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string currency = "BRL")
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency cannot be empty", nameof(currency));

            if (currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

            return new Money(amount, currency.ToUpper());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public Money Add(Money other)
        {
            if (other.Currency != Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return Create(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (other.Currency != Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");

            return Create(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return Create(Amount * factor, Currency);
        }

        public override string ToString()
        {
            return $"{Amount.ToString("F2")} {Currency}";
        }

        public static Money Zero(string currency = "BRL") => Create(0, currency);

        public static Money operator +(Money left, Money right) => left.Add(right);
        public static Money operator -(Money left, Money right) => left.Subtract(right);
        public static Money operator *(Money left, decimal right) => left.Multiply(right);
        public static Money operator *(decimal left, Money right) => right.Multiply(left);
    }

    public class MoneyJsonConverter : JsonConverter<Money>
    {
        public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            decimal amount = 0;
            string currency = "BRL";

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "amount":
                        amount = reader.GetDecimal();
                        break;
                    case "currency":
                        currency = reader.GetString() ?? "BRL";
                        break;
                }
            }

            return Money.Create(amount, currency);
        }

        public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("amount", value.Amount);
            writer.WriteString("currency", value.Currency);
            writer.WriteEndObject();
        }
    }
}
