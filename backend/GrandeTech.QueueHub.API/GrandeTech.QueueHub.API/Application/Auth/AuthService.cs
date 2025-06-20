using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grande.Fila.API.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Grande.Fila.API.Application.Auth
{
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var result = new LoginResult { Success = false };

            var user = await _userRepo.GetByUsernameAsync(request.Username, cancellationToken);
            if (user == null)
            {
                result.Error = "Invalid username or password.";
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
            if (user.Role == UserRoles.PlatformAdmin || user.Role == UserRoles.Admin)
            {
                // Check if admin account is locked
                if (user.IsLocked)
                {
                    result.Error = "Admin account is locked. Please contact support.";
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
            result.Username = user.Username;
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
            if (string.IsNullOrWhiteSpace(request.Username))
                result.FieldErrors["Username"] = "Username is required.";
            if (string.IsNullOrWhiteSpace(request.Email))
                result.FieldErrors["Email"] = "Email is required.";
            if (string.IsNullOrWhiteSpace(request.Password))
                result.FieldErrors["Password"] = "Password is required.";
            if (request.Password != request.ConfirmPassword)
                result.FieldErrors["ConfirmPassword"] = "Passwords do not match.";

            if (result.FieldErrors.Count > 0)
                return result;

            // Check uniqueness
            var usernameExists = await _userRepo.ExistsByUsernameAsync(request.Username, cancellationToken);
            if (usernameExists)
            {
                result.FieldErrors["Username"] = "Username is already taken.";
                return result;
            }

            var emailExists = await _userRepo.ExistsByEmailAsync(request.Email, cancellationToken);
            if (emailExists)
            {
                result.FieldErrors["Email"] = "Email is already registered.";
                return result;
            }

            // Create user
            var passwordHash = HashPassword(request.Password);
            var user = new User(request.Username, request.Email, passwordHash, "User"); // Default role is "User"

            try
            {
                await _userRepo.AddAsync(user, cancellationToken);
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
                new Claim(TenantClaims.Username, user.Username),
                new Claim(TenantClaims.Email, user.Email),
                new Claim(TenantClaims.Role, mappedRole),
                new Claim(TenantClaims.Permissions, JsonSerializer.Serialize(GetUserPermissions(mappedRole)))
            };

            // Add tenant context for non-platform admin roles
            if (mappedRole != UserRoles.PlatformAdmin)
            {
                var organizationId = Guid.NewGuid().ToString();
                claims.Add(new Claim(TenantClaims.OrganizationId, organizationId));
                claims.Add(new Claim(TenantClaims.TenantSlug, "test-tenant"));

                // Add location context for barber role
                if (mappedRole == UserRoles.Barber)
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
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string MapToNewRole(string oldRole)
        {
            return oldRole.ToLower() switch
            {
                "admin" => UserRoles.Admin,
                "owner" => UserRoles.Admin, // Owner becomes Admin in new model
                "barber" => UserRoles.Barber,
                "client" => UserRoles.Client,
                "user" => UserRoles.Client, // Default user becomes Client
                "system" => UserRoles.ServiceAccount,
                _ => UserRoles.Client // Default fallback
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
                UserRoles.Admin => new[]
                {
                    "manage:locations",
                    "manage:staff",
                    "manage:services",
                    "view:metrics",
                    "manage:branding"
                },
                UserRoles.Barber => new[]
                {
                    "manage:queue",
                    "view:queue",
                    "manage:appointments"
                },
                UserRoles.Client => new[]
                {
                    "view:queue",
                    "join:queue"
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

            var user = await _userRepo.GetByUsernameAsync(request.Username, cancellationToken);
            if (user == null)
            {
                result.Error = "Invalid username.";
                return result;
            }

            if (!user.IsActive)
            {
                result.Error = "Account is deactivated.";
                return result;
            }

            if (user.Role != UserRoles.PlatformAdmin && user.Role != UserRoles.Admin)
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

            var user = await _userRepo.GetByUsernameAsync(request.Username, cancellationToken);
            if (user == null)
            {
                result.Error = "Invalid username.";
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
            result.Username = user.Username;
            result.Role = user.Role;
            result.Permissions = GetUserPermissions(user.Role);

            return result;
        }
    }
} 