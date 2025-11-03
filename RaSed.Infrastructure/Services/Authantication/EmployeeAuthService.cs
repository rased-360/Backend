using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class EmployeeAuthService : IEmployeeAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<EmployeeAuthService> _logger;
        private const int RefreshTokenExpiryDays = 3;
        private const int MaxActiveTokensPerUser = 5;
        public EmployeeAuthService(IUnitOfWork unitOfWork, ITokenService tokenService, UserManager<ApplicationUser> userManager, ILogger<EmployeeAuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }
        public async Task<EmployeeAuthResult> LoginAsync(LoginDto dto, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", dto.Email);
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed - user not found: {Email}", dto.Email);
                    return EmployeeAuthResult.Failure("Login failed - user not found");
                }
                // 2. Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed - account deactivated: {Email}", dto.Email);
                    return EmployeeAuthResult.Failure("Account is deactivated. Contact administrator.");
                }

                // 3. Verify password
                if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    _logger.LogWarning("Login failed - invalid password for: {Email}", dto.Email);
                    return EmployeeAuthResult.Failure("Invalid email or password.");
                }

                // 4. Check if user is Employee (not  Admin)
                var employee = user as Employee;
                if (employee == null)
                {
                    _logger.LogWarning("Login failed - user is not employee: {Email}", dto.Email);
                    return EmployeeAuthResult.Failure("Access denied. Employee login only.");
                }


                // 6. Check if must change password
                bool mustChangePassword = employee.MustChangePassword;

                // 7. Generate claims for JWT
                var authClaims = await GenerateAuthClaims(user);
                // 8. Generate Access Token
                var accessToken = _tokenService.GenerateAccessToken(authClaims);

                // 9. Generate Refresh Token
                var refreshTokenString = _tokenService.GenerateRefreshToken();

                // ✅ 10. Clean up expired/revoked tokens first
                await _unitOfWork._refreshTokenRepository.RemoveExpiredTokensByUserIdAsync(user.Id);

                // ✅ 11. Check active tokens limit and enforce it
                await EnforceActiveTokenLimitAsync(user.Id);

                // ✅ 12. Create NEW refresh token (always create, never update)
                await CreateNewRefreshTokenAsync(user.Id, refreshTokenString, ipAddress);


                // 12. Update last login
                user.LastLogin = DateTime.UtcNow;


                // 13. Save changes -> make sure it be saved
                await _unitOfWork.SaveChangesAsync();

                // 14. Create response DTO
                var employeeResponse = new EmployeeResponseDto
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber ?? string.Empty,
                    Gender = employee.Gender,
                    NationalId = employee.NationalId,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt,
                };

                _logger.LogInformation("Login successful for: {Email}", dto.Email);

                // 15. Return success result
                return EmployeeAuthResult.Success(
                    accessToken: accessToken,
                    refreshToken: refreshTokenString,
                    employee: employeeResponse,
                    mustChangePassword: mustChangePassword,
                    message: mustChangePassword
                        ? "Login successful. Please change your password."
                        : "Login successful."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", dto.Email);
                return EmployeeAuthResult.Failure("An error occurred during login. Please try again.");
            }
        }

        public async Task<EmployeeAuthResult> LogoutAsync(string refreshToken, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Logout attempt from IP: {IP}", ipAddress);

                // 1. Validate refresh token
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("Logout failed - refresh token is empty");
                    return EmployeeAuthResult.Failure("Refresh token is required.");
                }

                // 2. Find refresh token in database
                var storedToken = await _unitOfWork._refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Logout failed - refresh token not found");
                    return EmployeeAuthResult.Failure("Invalid refresh token.");
                }
                // Verify token belongs to authenticated user
                if (storedToken.UserId.ToString() != userId)
                {
                    _logger.LogWarning("Logout attempt with token belonging to different user. Token UserId: {TokenUserId}, Requesting UserId: {RequestingUserId}",
                        storedToken.UserId, userId);
                    return EmployeeAuthResult.Failure("Invalid refresh token.");
                }
                // 3. Check if token already revoked
                if (storedToken.Revoked != null)
                {
                    _logger.LogWarning("Logout failed - token already revoked");
                    return EmployeeAuthResult.Failure("Token already revoked.");
                }

                // 4. Revoke the token
                storedToken.Revoked = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;
                storedToken.ReasonRevoked = "Logged out by user";

                // 5. Save changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Logout successful for user: {UserId}", storedToken.UserId);

                // 6. Return success
                return EmployeeAuthResult.Success(
                    accessToken: null,
                    refreshToken: null,
                    employee: null,
                    mustChangePassword: false,
                    message: "Logged out successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return EmployeeAuthResult.Failure("An error occurred during logout. Please try again.");
            }
        }

        public async Task<EmployeeAuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Refresh token attempt from IP: {IP}", ipAddress);

                // 1. Find refresh token in database
                var storedToken = await _unitOfWork._refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Refresh token not found");
                    return EmployeeAuthResult.Failure("Invalid refresh token.");
                }

                // 2. Check if token is active
                if (storedToken.Revoked != null)
                {
                    _logger.LogWarning("Refresh token already revoked");
                    return EmployeeAuthResult.Failure("Refresh token has been revoked.");
                }

                if (storedToken.IsExpired)
                {
                    _logger.LogWarning("Refresh token expired");
                    return EmployeeAuthResult.Failure("Refresh token has expired.");
                }

                // 3. Get user
                var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("User not found or inactive for refresh token");
                    return EmployeeAuthResult.Failure("User not found or inactive.");
                }

                var employee = user as Employee;
                if (employee == null)
                {
                    return EmployeeAuthResult.Failure("Invalid user type.");
                }

                // 4. Revoke old refresh token
                storedToken.Revoked = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;
                // Note: Repository should NOT call SaveChanges

                // 5. Get claims
                bool mustChangePassword = employee.MustChangePassword;
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
                var employeeResponse = new EmployeeResponseDto
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber ?? string.Empty,
                    Gender = employee.Gender,
                    NationalId = employee.NationalId,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt
                };

                _logger.LogInformation("Refresh token successful for user: {UserId}", user.Id);

                return EmployeeAuthResult.Success(
                    accessToken: newAccessToken,
                    refreshToken: newRefreshTokenString,
                    employee: employeeResponse,
                    mustChangePassword: mustChangePassword,
                    message: "Token refreshed successfully."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return EmployeeAuthResult.Failure("An error occurred during token refresh.");
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken, string userId, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Revoke token attempt from IP: {IP} for user: {UserId}", ipAddress, userId);

                // 1. Validate input
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("Revoke failed - refresh token is empty");
                    return false;
                }

                // 2. Find token in database
                var storedToken = await _unitOfWork._refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (storedToken == null)
                {
                    _logger.LogWarning("Revoke failed - token not found");
                    return false;
                }

                // 3. ✅ CRITICAL FIX: Verify token belongs to authenticated user
                if (storedToken.UserId.ToString() != userId)
                {
                    _logger.LogWarning(
                        "Revoke attempt with token belonging to different user. Token UserId: {TokenUserId}, Requesting UserId: {RequestingUserId}",
                        storedToken.UserId, userId);
                    return false;
                }

                // 4. Check if already revoked
                if (storedToken.Revoked != null)
                {
                    _logger.LogWarning("Revoke failed - token already revoked for user: {UserId}", userId);
                    return false;
                }

                // 5. Revoke the token
                storedToken.Revoked = DateTime.UtcNow;
                storedToken.RevokedByIp = ipAddress;
                storedToken.ReasonRevoked = "Revoked by user";

                // 6. Save changes
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Token revoked successfully for user: {UserId}", userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
                return false;
            }
        }

        // Always create new token(never update existing)
        private async Task CreateNewRefreshTokenAsync(int userId, string refreshToken, string ipAddress)
        {
            var newToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _unitOfWork._refreshTokenRepository.AddAsync(newToken);

            _logger.LogInformation("New refresh token created for user: {UserId} from IP: {IP}",
                userId, ipAddress);
        }

        // Enforce active tokens limit per user
        private async Task EnforceActiveTokenLimitAsync(int userId)
        {
            var activeCount = await _unitOfWork._refreshTokenRepository
                .GetActiveTokensCountAsync(userId);

            _logger.LogInformation("User {UserId} has {Count} active tokens", userId, activeCount);

            // If limit reached, remove oldest active token
            if (activeCount >= MaxActiveTokensPerUser)
            {
                var oldestToken = await _unitOfWork._refreshTokenRepository
                    .GetOldestActiveTokenAsync(userId);

                if (oldestToken != null)
                {
                    oldestToken.Revoked = DateTime.UtcNow;
                    oldestToken.ReasonRevoked = $"Exceeded max active tokens limit ({MaxActiveTokensPerUser})";

                    _logger.LogInformation(
                        "Revoked oldest token for user {UserId} due to limit. Token created: {Created}",
                        userId, oldestToken.Created);
                }
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
