namespace Coontrera.Domain.Models
{
    public class DayAvailability
    {
        public int Weekday { get; private set; }
        public bool IsOpen { get; private set; }
        public string StartTime { get; private set; } = "08:00";
        public string EndTime { get; private set; } = "18:00";
        public int SlotMinutes { get; private set; } = 60;

        protected DayAvailability() { }

        public DayAvailability(int weekday, bool isOpen, string startTime = "08:00", string endTime = "18:00", int slotMinutes = 60)
        {
            if (weekday < 1 || weekday > 7)
                throw new ArgumentOutOfRangeException(nameof(weekday), "Weekday deve ser entre 1 (Segunda) e 7 (Domingo).");

            Weekday = weekday;
            IsOpen = isOpen;
            StartTime = string.IsNullOrWhiteSpace(startTime) ? "08:00" : startTime;
            EndTime = string.IsNullOrWhiteSpace(endTime) ? "18:00" : endTime;
            SlotMinutes = slotMinutes <= 0 ? 60 : slotMinutes;
        }

        public void Update(bool isOpen, string? startTime, string? endTime, int? slotMinutes = null)
        {
            IsOpen = isOpen;
            if (!string.IsNullOrWhiteSpace(startTime)) StartTime = startTime;
            if (!string.IsNullOrWhiteSpace(endTime)) EndTime = endTime;
            if (slotMinutes.HasValue && slotMinutes.Value > 0) SlotMinutes = slotMinutes.Value;
        }
    }
}
