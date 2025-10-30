using System;
using System.ComponentModel.DataAnnotations;
using Grande.Fila.API.Domain.Common;
using System.Collections.Generic;

namespace Grande.Fila.API.Domain.Users
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsLocked { get; set; }
        public bool RequiresTwoFactor { get; set; }

        // For EF Core
        public User()
        {
            IsActive = true;
            IsLocked = false;
            RequiresTwoFactor = false;
        }

        public User(string fullName, string email, string phoneNumber, string passwordHash, string role)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
            IsLocked = false;
            RequiresTwoFactor = role == UserRoles.PlatformAdmin || role == UserRoles.Owner;
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }
    }
} 