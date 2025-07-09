using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{    public record Email : ValueObject
    {
        public string Value { get; private set; } = string.Empty;

        // Parameterless constructor for Entity Framework
        private Email()
        {
        }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            email = email.Trim();

            if (email.Length > 320)
            {
                throw new ArgumentException("Email is too long", nameof(email));
            }

            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Email is invalid", nameof(email));
            }

            return new Email(email);
        }

        private static bool IsValidEmail(string email)
        {
            // Simple regex for email validation
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public static implicit operator string(Email email) => email.Value;

        public override string ToString() => Value;
    }
}
