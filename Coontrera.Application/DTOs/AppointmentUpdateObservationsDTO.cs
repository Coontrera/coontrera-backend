using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class AppointmentUpdateObservationsDTO
    {
        [Required(ErrorMessage = "As observações são obrigatórias.")]
        public string Observations { get; set; } = string.Empty;
    }
}
