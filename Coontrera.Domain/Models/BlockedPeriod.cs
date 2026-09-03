namespace Coontrera.Domain.Models
{
    public class BlockedPeriod
    {
        public string Id { get; private set; } = string.Empty;
        public DateOnly Date { get; private set; }
        public string? StartTime { get; private set; }
        public string? EndTime { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public DateTime DateRegistered { get; private set; } = DateTime.UtcNow;

        public bool IsFullDay => string.IsNullOrWhiteSpace(StartTime) && string.IsNullOrWhiteSpace(EndTime);

        protected BlockedPeriod() { }

        public BlockedPeriod(DateOnly date, string reason, string? startTime = null, string? endTime = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("O motivo do bloqueio é obrigatório.", nameof(reason));

            Id = Guid.NewGuid().ToString();
            Date = date;
            Reason = reason;
            StartTime = string.IsNullOrWhiteSpace(startTime) ? null : startTime;
            EndTime = string.IsNullOrWhiteSpace(endTime) ? null : endTime;
            DateRegistered = DateTime.UtcNow;
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public void SetDateRegistered(DateTime dateRegistered)
        {
            DateRegistered = dateRegistered;
        }
    }
}
