using System;
using System.Collections.Generic;
using Grande.Fila.API.Domain.Common;

namespace Grande.Fila.API.Domain.Customers
{
    /// <summary>
    /// Represents a customer (client) who can join queues
    /// </summary>
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsAnonymous { get; set; }
        public bool NotificationsEnabled { get; set; }
        public string? PreferredNotificationChannel { get; set; }
        public string? UserId { get; set; }

        // For EF Core
        public Customer() { }

        public Customer(
            string name,
            string? phoneNumber,
            string? email,
            bool isAnonymous,
            string? userId = null)
        {
            if (isAnonymous && string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Even anonymous customers must have a name", nameof(name));

            if (!isAnonymous && (string.IsNullOrWhiteSpace(phoneNumber) && string.IsNullOrWhiteSpace(email)))
                throw new ArgumentException("Non-anonymous customers must provide either phone number or email");

            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            IsAnonymous = isAnonymous;
            NotificationsEnabled = !isAnonymous;
            PreferredNotificationChannel = DeterminePreferredNotificationChannel(phoneNumber, email);
            UserId = userId;
        }

        private string DeterminePreferredNotificationChannel(string? phoneNumber, string? email)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
                return "whatsapp";

            if (!string.IsNullOrWhiteSpace(email))
                return "email";

            return "none";
        }

    }
}
