using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Grande.Fila.API.Infrastructure.Services
{
    /// <summary>
    /// Service for anonymizing personal data to ensure GDPR compliance
    /// </summary>
    public interface IDataAnonymizationService
    {
        /// <summary>
        /// Anonymizes a phone number by hashing it
        /// </summary>
        string AnonymizePhoneNumber(string phoneNumber);
        
        /// <summary>
        /// Anonymizes an email address by hashing the local part
        /// </summary>
        string AnonymizeEmail(string email);
        
        /// <summary>
        /// Anonymizes a name by keeping only the first letter
        /// </summary>
        string AnonymizeName(string name);
        
        /// <summary>
        /// Checks if data should be anonymized based on retention policy
        /// </summary>
        bool ShouldAnonymize(DateTime dataCreated, DateTime? dataLastAccessed = null);
        
        /// <summary>
        /// Gets the anonymized identifier for tracking without personal data
        /// </summary>
        string GetAnonymizedIdentifier(string originalId, string salt = null);
    }

    /// <summary>
    /// Implementation of data anonymization service
    /// </summary>
    public class DataAnonymizationService : IDataAnonymizationService
    {
        private readonly ILogger<DataAnonymizationService> _logger;
        private readonly DataAnonymizationOptions _options;

        public DataAnonymizationService(
            ILogger<DataAnonymizationService> logger,
            IOptions<DataAnonymizationOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public string AnonymizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            try
            {
                // Keep country code and last 2 digits, hash the rest
                if (phoneNumber.Length <= 4)
                    return HashString(phoneNumber);

                var countryCode = phoneNumber.Substring(0, Math.Min(3, phoneNumber.Length));
                var lastDigits = phoneNumber.Length >= 2 ? phoneNumber.Substring(phoneNumber.Length - 2) : phoneNumber;
                var middlePart = phoneNumber.Substring(countryCode.Length, phoneNumber.Length - countryCode.Length - lastDigits.Length);
                
                var hashedMiddle = HashString(middlePart);
                return $"{countryCode}{hashedMiddle.Substring(0, Math.Min(3, hashedMiddle.Length))}{lastDigits}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing phone number: {PhoneNumber}", phoneNumber);
                return HashString(phoneNumber);
            }
        }

        public string AnonymizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            try
            {
                var atIndex = email.IndexOf('@');
                if (atIndex <= 0)
                    return HashString(email);

                var localPart = email.Substring(0, atIndex);
                var domain = email.Substring(atIndex);

                // Keep first and last character of local part, hash the middle
                if (localPart.Length <= 2)
                    return HashString(localPart) + domain;

                var firstChar = localPart[0];
                var lastChar = localPart[localPart.Length - 1];
                var middlePart = localPart.Substring(1, localPart.Length - 2);
                var hashedMiddle = HashString(middlePart);

                return $"{firstChar}{hashedMiddle.Substring(0, Math.Min(3, hashedMiddle.Length))}{lastChar}{domain}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing email: {Email}", email);
                return HashString(email);
            }
        }

        public string AnonymizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            try
            {
                var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return string.Empty;

                if (parts.Length == 1)
                    return parts[0].Length > 1 ? $"{parts[0][0]}***" : parts[0];

                var anonymizedParts = new string[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Length > 1)
                        anonymizedParts[i] = $"{parts[i][0]}***";
                    else
                        anonymizedParts[i] = parts[i];
                }

                return string.Join(" ", anonymizedParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing name: {Name}", name);
                return HashString(name);
            }
        }

        public bool ShouldAnonymize(DateTime dataCreated, DateTime? dataLastAccessed = null)
        {
            var now = DateTime.UtcNow;
            var retentionPeriod = TimeSpan.FromDays(_options.DataRetentionDays);
            
            // Check if data has exceeded retention period
            if (now.Subtract(dataCreated) > retentionPeriod)
                return true;

            // Check if data hasn't been accessed recently
            if (dataLastAccessed.HasValue)
            {
                var accessRetentionPeriod = TimeSpan.FromDays(_options.InactiveDataRetentionDays);
                if (now.Subtract(dataLastAccessed.Value) > accessRetentionPeriod)
                    return true;
            }

            return false;
        }

        public string GetAnonymizedIdentifier(string originalId, string salt = null)
        {
            if (string.IsNullOrWhiteSpace(originalId))
                return string.Empty;

            try
            {
                var combined = salt != null ? $"{originalId}:{salt}" : originalId;
                return HashString(combined);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating anonymized identifier for: {OriginalId}", originalId);
                return HashString(originalId);
            }
        }

        private string HashString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Substring(0, Math.Min(16, hash.Length));
            }
        }
    }

    /// <summary>
    /// Configuration options for data anonymization
    /// </summary>
    public class DataAnonymizationOptions
    {
        /// <summary>
        /// Number of days to retain personal data before anonymization
        /// </summary>
        public int DataRetentionDays { get; set; } = 90; // 3 months

        /// <summary>
        /// Number of days to retain inactive data before anonymization
        /// </summary>
        public int InactiveDataRetentionDays { get; set; } = 30; // 1 month

        /// <summary>
        /// Whether to enable automatic anonymization
        /// </summary>
        public bool EnableAutomaticAnonymization { get; set; } = true;

        /// <summary>
        /// Salt for additional security in hashing
        /// </summary>
        public string HashSalt { get; set; } = "GrandeTechQueueHub2024";
    }
}
