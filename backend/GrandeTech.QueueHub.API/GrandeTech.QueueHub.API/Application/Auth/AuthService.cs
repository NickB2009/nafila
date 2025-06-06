using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrandeTech.QueueHub.API.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GrandeTech.QueueHub.API.Application.Auth
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

            user.UpdateLastLogin();
            await _userRepo.UpdateAsync(user, cancellationToken);

            var token = GenerateJwtToken(user);

            result.Success = true;
            result.Token = token;
            result.Username = user.Username;
            result.Role = user.Role;

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

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("sub", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            // In production, use a proper password hasher like BCrypt
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string hash)
        {
            var computedHash = HashPassword(password);
            return computedHash == hash;
        }
    }
} 