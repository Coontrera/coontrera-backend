using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        [MinLength(2, ErrorMessage = "O nome deve ter no mínimo 2 caracteres.")]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string Password { get; set; } = null!;

        [Required]
        [Phone]
        public string Phone { get; set; } = null!;
    }
}