using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Users;
using Grande.Fila.API.Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Grande.Fila.API.Application.Auth
{
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepo, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _userRepo = userRepo;
            _config = config;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var result = new LoginResult { Success = false };

            // Normalize phone number
            var phoneNumber = request.PhoneNumber?.Trim();
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                result.Error = "Invalid phone number or password.";
                return result;
            }

            // Get user by phone number
            var user = await _userRepo.GetByPhoneNumberAsync(phoneNumber, cancellationToken);
            if (user == null)
            {
                result.Error = "Invalid phone number or password.";
                return result;
            }

            if (!user.IsActive)
            {
                result.Error = "Account is deactivated.";
                return result;
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                result.Error = "Invalid username or password.";
                return result;
            }

            // Admin-specific validation
            if (user.Role == UserRoles.PlatformAdmin || user.Role == UserRoles.Owner)
            {
                // Check if admin account is locked
                if (user.IsLocked)
                {
                    result.Error = "Account is locked. Please contact support.";
                    return result;
                }

                // Check if admin account requires 2FA
                if (user.RequiresTwoFactor)
                {
                    result.RequiresTwoFactor = true;
                    result.TwoFactorToken = GenerateTwoFactorToken(user);
                    return result;
                }
            }

            user.UpdateLastLogin();
            await _userRepo.UpdateAsync(user, cancellationToken);

            var token = GenerateJwtToken(user);

            result.Success = true;
            result.Token = token;
            result.Username = user.FullName; // Using FullName since Username no longer exists
            result.Role = user.Role;
            result.Permissions = GetUserPermissions(user.Role);

            return result;
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var result = new RegisterResult
            {
                Success = false,
                FieldErrors = new Dictionary<string, string>()
            };

            // Validation
            if (string.IsNullOrWhiteSpace(request.FullName))
                result.FieldErrors["FullName"] = "Full name is required.";
            if (string.IsNullOrWhiteSpace(request.Email))
                result.FieldErrors["Email"] = "Email is required.";
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                result.FieldErrors["PhoneNumber"] = "Phone number is required.";

            // Password strength validation
            var (passwordValid, passwordError) = ValidatePasswordStrength(request.Password);
            if (!passwordValid)
                result.FieldErrors["Password"] = passwordError;

            if (result.FieldErrors.Count > 0)
                return result;

            // Normalize inputs
            var normalizedFullName = request.FullName?.Trim();
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();
            var normalizedPhoneNumber = request.PhoneNumber?.Trim();

            // Check uniqueness
            var emailExists = await _userRepo.ExistsByEmailAsync(normalizedEmail!, cancellationToken);
            if (emailExists)
            {
                result.FieldErrors["Email"] = "Email is already registered.";
                return result;
            }

            var phoneExists = await _userRepo.ExistsByPhoneNumberAsync(normalizedPhoneNumber!, cancellationToken);
            if (phoneExists)
            {
                result.FieldErrors["PhoneNumber"] = "Phone number is already registered.";
                return result;
            }

            // Create user
            var passwordHash = HashPassword(request.Password);
            var user = new User(normalizedFullName!, normalizedEmail!, normalizedPhoneNumber!, passwordHash, "Customer");

            try
            {
                await _userRepo.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                result.Success = true;
            }
            catch (Exception)
            {
                result.Error = "Failed to create account. Please try again.";
            }

            return result;
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Map old roles to new roles for backward compatibility
            var mappedRole = MapToNewRole(user.Role);

            var claims = new List<Claim>
            {
                new Claim(TenantClaims.UserId, user.Id.ToString()),
                new Claim(TenantClaims.Username, user.FullName), // Using FullName since Username no longer exists
                new Claim(TenantClaims.Email, user.Email),
                new Claim("PhoneNumber", user.PhoneNumber), // Add phone number to JWT claims
                new Claim(TenantClaims.Role, mappedRole)
            };

            foreach (var permission in GetUserPermissions(mappedRole))
            {
                claims.Add(new Claim(TenantClaims.Permissions, permission));
            }

            // Add service account flag for service accounts
            if (mappedRole == UserRoles.ServiceAccount)
            {
                claims.Add(new Claim(TenantClaims.IsServiceAccount, "true"));
            }
            else
            {
                claims.Add(new Claim(TenantClaims.IsServiceAccount, "false"));
            }

            // Add tenant context for roles that need it (excluding platform admin and service account)
            if (mappedRole != UserRoles.PlatformAdmin && mappedRole != UserRoles.ServiceAccount)
            {
                var organizationId = Guid.NewGuid().ToString();
                claims.Add(new Claim(TenantClaims.OrganizationId, organizationId));
                claims.Add(new Claim(TenantClaims.TenantSlug, "test-tenant"));

                // Add location context for staff role
                if (mappedRole == UserRoles.Staff)
                {
                    var locationId = Guid.NewGuid().ToString();
                    claims.Add(new Claim(TenantClaims.LocationId, locationId));
                }
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string MapToNewRole(string oldRole)
        {
            return oldRole.ToLower() switch
            {
                "platformadmin" => UserRoles.PlatformAdmin,
                "admin" => UserRoles.Owner, // Admin merged into Owner
                "owner" => UserRoles.Owner,
                "barber" => UserRoles.Staff, // Barber renamed to Staff
                "staff" => UserRoles.Staff,
                "client" => UserRoles.Customer, // Client renamed to Customer
                "customer" => UserRoles.Customer,
                "user" => UserRoles.Customer, // Default user becomes Customer
                "system" => UserRoles.ServiceAccount,
                "serviceaccount" => UserRoles.ServiceAccount,
                _ => UserRoles.Customer // Default fallback
            };
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password is required.");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long.");

            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter.");

            if (!password.Any(char.IsLower))
                return (false, "Password must contain at least one lowercase letter.");

            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one number.");

            return (true, string.Empty);
        }

        private string[] GetUserPermissions(string role)
        {
            return role switch
            {
                UserRoles.PlatformAdmin => new[]
                {
                    "manage:organizations",
                    "manage:subscriptions",
                    "view:analytics",
                    "manage:system",
                    "manage:users"
                },
                UserRoles.Owner => new[]
                {
                    "manage:locations",
                    "manage:staff",
                    "manage:services",
                    "view:metrics",
                    "manage:branding",
                    "view:analytics" // Added analytics for business owners
                },
                UserRoles.Staff => new[]
                {
                    "manage:queue",
                    "view:queue",
                    "manage:appointments"
                },
                UserRoles.Customer => new[]
                {
                    "view:queue",
                    "join:queue"
                },
                UserRoles.ServiceAccount => new[]
                {
                    "calculate:wait-time",
                    "access:analytics",
                    "system:operations"
                },
                _ => Array.Empty<string>()
            };
        }

        private string GenerateTwoFactorToken(User user)
        {
            // In a real implementation, this would generate a secure 2FA token
            // For now, we'll return a dummy token for testing
            return $"2FA-{Guid.NewGuid()}";
        }

        public async Task<AdminVerificationResult> VerifyAdminAsync(AdminVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var result = new AdminVerificationResult { Success = false };

            var user = await _userRepo.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
            if (user == null)
            {
                result.Error = "Invalid phone number.";
                return result;
            }

            if (!user.IsActive)
            {
                result.Error = "Account is deactivated.";
                return result;
            }

            if (user.Role != UserRoles.PlatformAdmin && user.Role != UserRoles.Owner)
            {
                result.Error = "User is not an admin.";
                return result;
            }

            // In a real implementation, verify the 2FA code
            // For now, we'll just check if it's not empty
            if (string.IsNullOrEmpty(request.TwoFactorCode))
            {
                result.Error = "Invalid 2FA code.";
                return result;
            }

            user.UpdateLastLogin();
            await _userRepo.UpdateAsync(user, cancellationToken);

            var token = GenerateJwtToken(user);

            result.Success = true;
            result.Token = token;
            result.Permissions = GetUserPermissions(user.Role);

            return result;
        }

        public async Task<LoginResult> VerifyTwoFactorAsync(VerifyTwoFactorRequest request, CancellationToken cancellationToken = default)
        {
            var result = new LoginResult { Success = false };

            var user = await _userRepo.GetByPhoneNumberAsync(request.PhoneNumber, cancellationToken);
            if (user == null)
            {
                result.Error = "Invalid phone number.";
                return result;
            }

            if (!user.IsActive)
            {
                result.Error = "Account is deactivated.";
                return result;
            }

            // In a real implementation, verify the 2FA code
            // For now, we'll just check if it's not empty
            if (string.IsNullOrEmpty(request.TwoFactorCode))
            {
                result.Error = "Invalid 2FA code.";
                return result;
            }

            user.UpdateLastLogin();
            await _userRepo.UpdateAsync(user, cancellationToken);

            var token = GenerateJwtToken(user);

            result.Success = true;
            result.Token = token;
            result.Username = user.FullName; // Using FullName since Username no longer exists
            result.Role = user.Role;
            result.Permissions = GetUserPermissions(user.Role);

            return result;
        }
    }
} 