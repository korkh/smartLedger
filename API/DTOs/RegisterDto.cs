using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class RegisterDto
    {
        [Required]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(
            "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).{8,}$",
            ErrorMessage = "Пароль должен содержать заглавную букву, цифру и быть не менее 8 символов."
        )]
        public string Password { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}
