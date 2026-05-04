using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class UserUpdateDTO
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [Phone]
        public string Phone { get; set; } = null!;

    }
}