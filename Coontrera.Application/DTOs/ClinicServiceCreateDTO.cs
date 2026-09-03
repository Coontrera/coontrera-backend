using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class ClinicServiceCreateDTO
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [MinLength(3, ErrorMessage = "O título deve ter pelo menos 3 caracteres.")]
        [MaxLength(100, ErrorMessage = "O título não pode ter mais de 100 caracteres.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [MinLength(10, ErrorMessage = "A descrição deve ter pelo menos 10 caracteres.")]
        [MaxLength(1000, ErrorMessage = "A descrição não pode ter mais de 1000 caracteres.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "A imagem é obrigatória.")]
        [Url(ErrorMessage = "O link da imagem deve ser uma URL válida.")]
        public string ImageUrl { get; set; } = string.Empty;

        public string ImageAlt { get; set; } = string.Empty;

        public string CtaText { get; set; } = string.Empty;

        public string IconAsset { get; set; } = string.Empty;

        public List<string> Benefits { get; set; } = new();
    }
}
