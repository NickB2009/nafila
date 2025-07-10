using System;
using System.Collections.Generic;

namespace Grande.Fila.API.Domain.Common.ValueObjects
{    public record BrandingConfig : ValueObject
    {
        public string PrimaryColor { get; private set; }
        public string SecondaryColor { get; private set; }
        public string LogoUrl { get; private set; }
        public string FaviconUrl { get; private set; }
        public string CompanyName { get; private set; }
        public string TagLine { get; private set; }
        public string FontFamily { get; private set; }

        // Parameterless constructor for Entity Framework
        private BrandingConfig()
        {
            PrimaryColor = string.Empty;
            SecondaryColor = string.Empty;
            LogoUrl = string.Empty;
            FaviconUrl = string.Empty;
            CompanyName = string.Empty;
            TagLine = string.Empty;
            FontFamily = string.Empty;
        }        private BrandingConfig(
            string primaryColor,
            string secondaryColor,
            string logoUrl,
            string faviconUrl,
            string companyName,
            string tagLine,
            string fontFamily)
        {
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            LogoUrl = logoUrl;
            FaviconUrl = faviconUrl;
            CompanyName = companyName;
            TagLine = tagLine;
            FontFamily = fontFamily;
        }public static BrandingConfig Create(
            string primaryColor,
            string secondaryColor,
            string logoUrl,
            string faviconUrl,
            string companyName,
            string tagLine,
            string fontFamily = "Arial, sans-serif")
        {
            if (string.IsNullOrWhiteSpace(primaryColor))
                throw new ArgumentException("Primary color cannot be empty", nameof(primaryColor));

            if (string.IsNullOrWhiteSpace(companyName))
                throw new ArgumentException("Company name cannot be empty", nameof(companyName));

            return new BrandingConfig(
                ValidateHexColor(primaryColor),
                ValidateHexColor(secondaryColor ?? "#FFFFFF"),
                logoUrl?.Trim() ?? string.Empty,
                faviconUrl?.Trim() ?? string.Empty,                companyName.Trim(),
                tagLine?.Trim() ?? string.Empty,
                fontFamily?.Trim() ?? "Arial, sans-serif"
            );
        }

        private static string ValidateHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return "#000000";

            color = color.Trim();

            // If it doesn't start with #, add it
            if (!color.StartsWith("#"))
                color = "#" + color;

            // If it's a short form hex color (#RGB), convert to long form (#RRGGBB)
            if (color.Length == 4)
            {
                color = "#" +
                       color[1] + color[1] +
                       color[2] + color[2] +
                       color[3] + color[3];
            }

            // Check if it's a valid hex color
            if (color.Length != 7 || !System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$"))
                throw new ArgumentException("Invalid hex color format. Must be #RRGGBB", nameof(color));

            return color.ToUpperInvariant();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return PrimaryColor;
            yield return SecondaryColor;
            yield return LogoUrl;
            yield return FaviconUrl;
            yield return CompanyName;
            yield return TagLine;
        }

        public static BrandingConfig Default => Create("#3B82F6", "#1E40AF", "", "", "Queue Hub", "Queue management made easy");
    }
}
