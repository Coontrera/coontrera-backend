using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class BlockDayDTO
    {
        [Required(ErrorMessage = "A data é obrigatória.")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "O motivo do bloqueio é obrigatório.")]
        public string Reason { get; set; } = string.Empty;
    }
}
