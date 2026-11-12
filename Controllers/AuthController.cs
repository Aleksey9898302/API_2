using System.Collections.Concurrent;
using Gamification.API.Dto;
using Gamification.API.Services;
using Gamification.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gamification.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtTokenService _jwtService;
    private readonly GamificationEngine _engine;
    private readonly PasswordHasher<string> _passwordHasher = new();

    // Временное хранилище учётных записей.
    // После перезапуска приложения данные пропадут.
    private static readonly ConcurrentDictionary<string, (Guid PlayerId, string Nickname, string PasswordHash)> Accounts = new();

    private static readonly object AccountsLock = new();

    public AuthController(JwtTokenService jwtService, GamificationEngine engine)
    {
        _jwtService = jwtService;
        _engine = engine;
    }

    // Регистрация нового игрока.
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        lock (AccountsLock)
        {
            // Проверяем, зарегистрирован ли такой Email.
            if (Accounts.ContainsKey(email))
            {
                return Conflict(new
                {
                    message = "Пользователь с таким Email уже существует."
                });
            }

            // Создаём игровой профиль в PostgreSQL.
            var player = _engine.RegisterPlayer(dto.Nickname);

            // Создаём хеш пароля.
            var passwordHash = _passwordHasher.HashPassword(email, dto.Password);

            // Сохраняем учётную запись в оперативной памяти.
            Accounts[email] = (player.Id, player.Nickname, passwordHash);

            // Создаём JWT-токен.
            var (token, expiresAt) = _jwtService.CreateToken(player.Id, player.Nickname, email, "player");

            return Ok(new
            {
                playerId = player.Id,
                token,
                expiresAt
            });
        }
    }

    // Вход существующего игрока.
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var email = dto.Login.Trim().ToLowerInvariant();

        // Ищем учётную запись по Email.
        if (!Accounts.TryGetValue(email, out var account))
        {
            return Unauthorized(new
            {
                message = "Неверный Email или пароль."
            });
        }

        // Проверяем введённый пароль с сохранённым хешем.
        var result = _passwordHasher.VerifyHashedPassword(email, account.PasswordHash, dto.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Неверный Email или пароль."
            });
        }

        // Выдаём JWT-токен.
        var (token, expiresAt) = _jwtService.CreateToken(account.PlayerId, account.Nickname, email, "player");

        return Ok(new
        {
            playerId = account.PlayerId,
            token,
            expiresAt
        });
    }
}
