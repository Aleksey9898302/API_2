using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace  Gamification.API.Services
{
    /// <summary>
    /// Сервис выпуска JWT-токенов.
    /// Токен подписывается секретным ключом (HMAC-SHA256) и содержит claims —
    /// заявления о пользователе (Id, Login, роль).
    /// </summary>
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string token, DateTime expiresAt) CreateToken(Guid userId, string nickname, string email, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims — «паспортные данные», зашитые в токен
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, nickname),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresMinutes"] ?? "60"));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}