using System;
using System.Collections.Generic;

namespace GrandeTech.QueueHub.API.Domain.Common.ValueObjects
{    public record Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = string.Empty;

        // Parameterless constructor for Entity Framework
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
}
