using System;
using System.ComponentModel.DataAnnotations;
using GrandeTech.QueueHub.API.Domain.Common;

namespace GrandeTech.QueueHub.API.Domain.Users
{
    public class User : BaseEntity, IAggregateRoot
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public bool IsLocked { get; private set; }
        public bool RequiresTwoFactor { get; private set; }

        // For EF Core and Bogus
        private User() 
        {
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            Role = string.Empty;
            IsActive = true;
            IsLocked = false;
            RequiresTwoFactor = false;
        }

        public User(string username, string email, string passwordHash, string role)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
            IsLocked = false;
            RequiresTwoFactor = role == UserRoles.PlatformAdmin || role == UserRoles.Admin;
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
    }
} 