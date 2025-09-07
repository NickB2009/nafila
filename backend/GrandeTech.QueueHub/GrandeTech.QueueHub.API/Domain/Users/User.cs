using System;
using System.ComponentModel.DataAnnotations;
using Grande.Fila.API.Domain.Common;
using System.Collections.Generic;

namespace Grande.Fila.API.Domain.Users
{
    public class User : BaseEntity, IAggregateRoot
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; private set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; private set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsLocked { get; private set; }
        public bool RequiresTwoFactor { get; private set; }

        public List<string> Permissions { get; private set; } = new List<string>();

        // For EF Core and Bogus
        private User()
        {
            FullName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            PasswordHash = string.Empty;
            Role = string.Empty;
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

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }

        public void EnableTwoFactor()
        {
            RequiresTwoFactor = true;
        }

        public void DisableTwoFactor()
        {
            RequiresTwoFactor = false;
        }

        public void AddPermission(string permission)
        {
            if (!Permissions.Contains(permission))
            {
                Permissions.Add(permission);
            }
        }

        public void RemovePermission(string permission)
        {
            Permissions.Remove(permission);
        }
    }
} 