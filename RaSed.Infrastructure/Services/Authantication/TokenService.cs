using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaSed.Application.Interfaces.Authantication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace RaSed.Infrastructure.Services.Authantication
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration configuration) {
            _config = configuration;
        }
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:issuer"],
                audience: _config["JWT:audience"],
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.UtcNow.AddMinutes(15)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            // Create a 32-byte array to hold cryptographically secure random bytes
            var randomNumber = new byte[32];

            // Use a cryptographically secure random number generator 
            // to fill the byte array with random values
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);

            // Convert the random bytes to a base64 encoded string 
            return Convert.ToBase64String(randomNumber);
        }

   



    }
}
