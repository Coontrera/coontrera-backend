namespace Coontrera.Application.DTOs
{
    public class TimeSlotDTO
    {
        public DateOnly Date { get; set; }

        /// <summary>
        /// 24h format "HH:mm" e.g. "08:00", "09:00"
        /// </summary>
        public string Time { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}
