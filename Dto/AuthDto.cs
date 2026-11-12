using System.ComponentModel.DataAnnotations;

namespace  Gamification.API.Dto
{
    /// <summary>
    /// DTO для входа в систему. Из этих данных выпускается JWT-токен.
    /// </summary>
    public class LoginDto
    {
        [Required(ErrorMessage = "Логин не может быть пустым.")]
        [StringLength(50, ErrorMessage = "Логин не длиннее 50 символов.")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль не может быть пустым.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть минимум 6 символов.")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO ответа с токеном.
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
    
    
    public class RegisterDto
    {
        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Nickname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Пароль должен содержать хотя бы одну букву и одну цифру.")]
        public string Password { get; set; } = string.Empty;
    }

}