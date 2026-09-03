using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class DayAvailabilityDTO
    {
        [Range(1, 7, ErrorMessage = "Weekday deve ser entre 1 (Segunda) e 7 (Domingo).")]
        public int Weekday { get; set; }
        public bool IsOpen { get; set; }
        public string StartTime { get; set; } = "08:00";
        public string EndTime { get; set; } = "18:00";
        public int SlotMinutes { get; set; } = 60;
    }
}
