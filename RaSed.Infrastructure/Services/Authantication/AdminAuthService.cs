using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AdminAuthService> _logger;
        private const int RefreshTokenExpiryDays = 7;
        public AdminAuthService(IUnitOfWork unitOfWork, ITokenService tokenService, UserManager<ApplicationUser> userManager, ILogger<AdminAuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AdminAuthResult> LoginAsync(LoginDto dto, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", dto.Email);
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed - user not found: {Email}", dto.Email);
                    return AdminAuthResult.Failure("Login failed - user not found");
                }
                // 2. Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed - account deactivated: {Email}", dto.Email);
                    return AdminAuthResult.Failure("Account is deactivated. Contact administrator.");
                }

                // 3. Verify password
                if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    _logger.LogWarning("Login failed - invalid password for: {Email}", dto.Email);
                    return AdminAuthResult.Failure("Invalid email or password.");
                }

                // 4. Check if user is Admin (not Employee)
                var admin = user as Admin;
                if (admin == null)
                {
                    _logger.LogWarning("Login failed - user is not admin: {Email}", dto.Email);
                    return AdminAuthResult.Failure("Access denied. Admin login only.");
                }

                // 5. Check if SuperAdmin
                bool isSuperAdmin = admin.IsSuperAdmin;

                // 6. Check if must change password
                bool mustChangePassword = admin.MustChangePassword;

                // 7. Generate claims for JWT
                var authClaims = await GenerateAuthClaims(user);
                // 8. Generate Access Token
                var accessToken = _tokenService.GenerateAccessToken(authClaims);

                // 9. Generate Refresh Token
                var refreshTokenString = _tokenService.GenerateRefreshToken();

                //10 Remove old/expired refresh tokens for this user 
                await _unitOfWork._refreshTokenRepository.RemoveExpiredTokensByUserIdAsync(user.Id);

                // 11. Create RefreshToken entity
                await SaveOrUpdateRefreshToken(user.Id, refreshTokenString,ipAddress);
           

                // 12. Update last login
                user.LastLogin = DateTime.UtcNow;


                // 13. Save changes -> make sure it be saved
                await _unitOfWork.SaveChangesAsync();

                // 14. Create response DTO
                var adminResponse = new AdminResponseDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber ?? string.Empty,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt
                };

                _logger.LogInformation("Login successful for: {Email}", dto.Email);

                // 15. Return success result
                return AdminAuthResult.Success(
                    accessToken: accessToken,
                    refreshToken: refreshTokenString,
                    admin: adminResponse,
                    isSuperAdmin: isSuperAdmin,
                    mustChangePassword: mustChangePassword,
                    message: mustChangePassword
                        ? "Login successful. Please change your password."
                        : "Login successful."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", dto.Email);
                return AdminAuthResult.Failure("An error occurred during login. Please try again.");
            }

        }
        public async Task<AdminAuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Refresh token attempt from IP: {IP}", ipAddress);

                // 1. Find refresh token in database
                var storedToken = await _unitOfWork._refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Refresh token not found");
                    return AdminAuthResult.Failure("Invalid refresh token.");
                }

                // 2. Check if token is active
                if (storedToken.Revoked != null)
                {
                    _logger.LogWarning("Refresh token already revoked");
                    return AdminAuthResult.Failure("Refresh token has been revoked.");
                }

                if (storedToken.IsExpired)
                {
                    _logger.LogWarning("Refresh token expired");
                    return AdminAuthResult.Failure("Refresh token has expired.");
                }

                // 3. Get user
                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive for refresh token");
                    return AdminAuthResult.Failure("User not found or inactive.");
                }

                var admin = user as Admin;
                if (admin == null)
                {
                    return AdminAuthResult.Failure("Invalid user type.");
                }

                // 4. Revoke old refresh token
                storedToken.Revoked = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;
                // Note: Repository should NOT call SaveChanges

                // 5. Get claims
                bool isSuperAdmin = admin.IsSuperAdmin;
                bool mustChangePassword = admin.MustChangePassword;
                var authClaims = await GenerateAuthClaims(user);

                // 6. Generate new tokens
                var newAccessToken = _tokenService.GenerateAccessToken(authClaims);
                var newRefreshTokenString = _tokenService.GenerateRefreshToken();

                // 7. Link old token to new one
                storedToken.ReplacedByToken = newRefreshTokenString;

                // 8. Create new refresh token
                var newRefreshToken = new RefreshToken
                {
                    Token = newRefreshTokenString,
                    UserId = user.Id,
                    Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };

                await _unitOfWork._refreshTokenRepository.AddAsync(newRefreshToken);

                // 9. Save ALL changes in one transaction
                await _unitOfWork.SaveChangesAsync();

                // 10. Create response
                var adminResponse = new AdminResponseDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber ?? string.Empty,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt
                };

                _logger.LogInformation("Refresh token successful for user: {UserId}", user.Id);

                return AdminAuthResult.Success(
                    accessToken: newAccessToken,
                    refreshToken: newRefreshTokenString,
                    admin: adminResponse,
                    isSuperAdmin: isSuperAdmin,
                    mustChangePassword: mustChangePassword,
                    message: "Token refreshed successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return AdminAuthResult.Failure("An error occurred during token refresh.");
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var storedToken = await _unitOfWork._refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (storedToken == null || storedToken.Revoked != null)
                    return false;

                storedToken.Revoked = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;
                storedToken.ReasonRevoked = "Revoked by user";

                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return false;
            }
        }

        public async Task<AdminAuthResult> LogoutAsync(string refreshToken, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Logout attempt from IP: {IP}", ipAddress);

                // 1. Validate refresh token
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("Logout failed - refresh token is empty");
                    return AdminAuthResult.Failure("Refresh token is required.");
                }

                // 2. Find refresh token in database
                var storedToken = await _unitOfWork._refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Logout failed - refresh token not found");
                    return AdminAuthResult.Failure("Invalid refresh token.");
                }
                // Verify token belongs to authenticated user
                if (storedToken.UserId.ToString() != userId)
                {
                    _logger.LogWarning("Logout attempt with token belonging to different user. Token UserId: {TokenUserId}, Requesting UserId: {RequestingUserId}",
                        storedToken.UserId, userId);
                    return AdminAuthResult.Failure("Invalid refresh token.");
                }

                // 3. Check if token already revoked
                if (storedToken.Revoked != null)
                {
                    _logger.LogWarning("Logout failed - token already revoked");
                    return AdminAuthResult.Failure("Token already revoked.");
                }

                // 4. Revoke the token
                storedToken.Revoked = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;
                storedToken.ReasonRevoked = "Logged out by user";

                // 5. Save changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Logout successful for user: {UserId}", storedToken.UserId);

                // 6. Return success
                return AdminAuthResult.Success(
                    accessToken: null,
                    refreshToken: null,
                    admin: null,
                    isSuperAdmin: false,
                    mustChangePassword: false,
                    message: "Logged out successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return AdminAuthResult.Failure("An error occurred during logout. Please try again.");
            }
        }


        #region

        private async Task<List<Claim>> GenerateAuthClaims(ApplicationUser user)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            return authClaims;
        }

        private async Task SaveOrUpdateRefreshToken(int userId, string refreshToken, string ipAddress)
        {
            var existingToken = await _unitOfWork._refreshTokenRepository.GetByUserIdAsync(userId);

            if (existingToken == null)
            {
                // Create new token
                var newToken = new RefreshToken
                {
                    UserId = userId,
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
                await _unitOfWork._refreshTokenRepository.AddAsync(newToken);
            }
            else
            {
                // Update existing token
                existingToken.Token = refreshToken;
                existingToken.Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
                existingToken.Created = DateTime.UtcNow;
                existingToken.CreatedByIp = ipAddress;
                existingToken.Revoked = null; // Reset revoked status
                existingToken.RevokedByIp = null;
                existingToken.ReplacedByToken = null;
                existingToken.ReasonRevoked = null;
            }
        }  
        #endregion
    }
}
