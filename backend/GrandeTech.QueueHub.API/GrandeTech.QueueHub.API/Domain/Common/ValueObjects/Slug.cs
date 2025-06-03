using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GrandeTech.QueueHub.API.Domain.Common.ValueObjects
{
    public record Slug : ValueObject
    {
        public string Value { get; }

        private Slug(string value)
        {
            Value = value;
        }

        public static Slug Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input for slug cannot be empty", nameof(input));

            // Remove all non-alphanumeric characters
            string slug = input.ToLowerInvariant()
                .Trim()
                .Replace(" ", "-")
                .Replace("_", "-");

            // Remove diacritics (accents)
            slug = RemoveDiacritics(slug);
            
            // Remove any non-alphanumeric characters except hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", string.Empty);
            
            // Remove multiple consecutive hyphens
            slug = Regex.Replace(slug, @"-{2,}", "-");
            
            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            if (string.IsNullOrEmpty(slug))
                throw new ArgumentException("Slug generation resulted in an empty string", nameof(input));

            if (slug.Length > 100)
                slug = slug.Substring(0, 100).TrimEnd('-');

            return new Slug(slug);
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static implicit operator string(Slug slug) => slug.Value;

        public override string ToString() => Value;
    }
}
