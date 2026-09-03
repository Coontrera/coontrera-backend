using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class BlockSlotDTO
    {
        [Required(ErrorMessage = "A data é obrigatória.")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "O horário inicial é obrigatório.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "O horário final é obrigatório.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "O motivo do bloqueio é obrigatório.")]
        public string Reason { get; set; } = string.Empty;
    }
}
