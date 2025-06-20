using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{    public record PhoneNumber : ValueObject
    {
        public string Value { get; private set; } = string.Empty;
        public string CountryCode { get; private set; } = string.Empty;
        public string NationalNumber { get; private set; } = string.Empty;

        // Parameterless constructor for Entity Framework
        private PhoneNumber()
        {
        }

        private PhoneNumber(string value, string countryCode, string nationalNumber)
        {
            Value = value;
            CountryCode = countryCode;
            NationalNumber = nationalNumber;
        }

        public static PhoneNumber Create(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentNullException(nameof(phoneNumber));
            }

            phoneNumber = phoneNumber.Trim();
            
            // Remove any non-digit characters for validation
            string digitsOnly = Regex.Replace(phoneNumber, @"\D", "");

            if (digitsOnly.Length < 8 || digitsOnly.Length > 15)
            {
                throw new ArgumentException("Phone number length is invalid", nameof(phoneNumber));
            }

            // Simple parsing - this could be enhanced with a proper phone number library
            string countryCode = digitsOnly.Length > 10 ? digitsOnly.Substring(0, digitsOnly.Length - 10) : "";
            string nationalNumber = digitsOnly.Length > 10 ? digitsOnly.Substring(digitsOnly.Length - 10) : digitsOnly;

            return new PhoneNumber(phoneNumber, countryCode, nationalNumber);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Extract only digits for equality comparison
            string digitsOnly = Regex.Replace(Value, @"\D", "");
            yield return digitsOnly;
        }

        public static implicit operator string(PhoneNumber phone) => phone.Value;

        public override string ToString() => Value;

        public string ToFormattedString()
        {
            if (string.IsNullOrEmpty(CountryCode))
            {
                return NationalNumber;
            }
            return $"+{CountryCode} {NationalNumber}";
        }
    }
}
