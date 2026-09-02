using Coontrera.Domain.Models.Enum;

namespace Coontrera.Domain.Models
{
    public class AuditLog
    {
        public string Id { get; private set; } = string.Empty;
        public string EntityName { get; private set; } = string.Empty;
        public string EntityId { get; private set; } = string.Empty;
        public AuditAction Action { get; private set; }
        public string? UserId { get; private set; }
        public string? UserEmail { get; private set; }
        public string? Details { get; private set; }
        public string? IpAddress { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        protected AuditLog() { }

        public AuditLog(
            string entityName,
            string entityId,
            AuditAction action,
            string? userId = null,
            string? userEmail = null,
            string? details = null,
            string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentException("EntityName cannot be empty.", nameof(entityName));
            if (string.IsNullOrWhiteSpace(entityId))
                throw new ArgumentException("EntityId cannot be empty.", nameof(entityId));

            Id = Guid.NewGuid().ToString();
            EntityName = entityName;
            EntityId = entityId;
            Action = action;
            UserId = userId;
            UserEmail = userEmail;
            Details = details;
            IpAddress = ipAddress;
            Timestamp = DateTime.UtcNow;
        }

        public void SetId(string id)
        {
            Id = id;
        }
    }
}
