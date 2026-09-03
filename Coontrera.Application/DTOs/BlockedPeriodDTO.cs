namespace Coontrera.Application.DTOs
{
    public class BlockedPeriodDTO
    {
        public string Id { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsFullDay { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}
