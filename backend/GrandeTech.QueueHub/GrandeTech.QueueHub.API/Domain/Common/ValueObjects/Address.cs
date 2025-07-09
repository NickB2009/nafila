using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{    public record Address : ValueObject
    {
        public string Street { get; private set; } = string.Empty;
        public string Number { get; private set; } = string.Empty;
        public string Complement { get; private set; } = string.Empty;
        public string Neighborhood { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;
        public string PostalCode { get; private set; } = string.Empty;
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }

        // Parameterless constructor for Entity Framework
        private Address()
        {
        }

        private Address(
            string street, 
            string number, 
            string complement,
            string neighborhood,
            string city, 
            string state, 
            string country, 
            string postalCode,
            double? latitude = null,
            double? longitude = null)
        {
            Street = street;
            Number = number;
            Complement = complement;
            Neighborhood = neighborhood;
            City = city;
            State = state;
            Country = country;
            PostalCode = postalCode;
            Latitude = latitude;
            Longitude = longitude;
        }public static Address Create(
            string street,
            string number,
            string complement,
            string neighborhood,
            string city,
            string state,
            string country,
            string postalCode,
            double? latitude = null,
            double? longitude = null)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street cannot be empty", nameof(street));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("State cannot be empty", nameof(state));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country cannot be empty", nameof(country));

            return new Address(
                street.Trim(),
                number?.Trim() ?? string.Empty,
                complement?.Trim() ?? string.Empty,
                neighborhood?.Trim() ?? string.Empty,
                city.Trim(),
                state.Trim(),
                country.Trim(),
                postalCode?.Trim() ?? string.Empty
            );
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street.ToLowerInvariant();
            yield return Number.ToLowerInvariant();
            yield return Complement.ToLowerInvariant();
            yield return Neighborhood.ToLowerInvariant();
            yield return City.ToLowerInvariant();
            yield return State.ToLowerInvariant();
            yield return Country.ToLowerInvariant();
            yield return PostalCode.ToLowerInvariant();
        }

        public override string ToString()
        {
            string address = $"{Street}, {Number}";
            if (!string.IsNullOrEmpty(Complement))
                address += $", {Complement}";
            if (!string.IsNullOrEmpty(Neighborhood))
                address += $", {Neighborhood}";
            address += $", {City}, {State}, {Country}";
            if (!string.IsNullOrEmpty(PostalCode))
                address += $" - {PostalCode}";

            return address;
        }
    }
}
