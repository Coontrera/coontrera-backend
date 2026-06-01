using Coontrera.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Coontrera.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;

        public TokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:SecretKey"];
        }

        public string GenerateToken(string email, string firebaseUid)
        {
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, firebaseUid)
                }),
                Expires = DateTime.UtcNow.AddHours(24), 
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
}