using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string entityName,
            string entityId,
            AuditAction action,
            string? userId = null,
            string? userEmail = null,
            string? details = null,
            string? ipAddress = null);

        Task<List<AuditLog>> GetAllAuditsAsync();
        Task<List<AuditLog>> GetAuditsByEntityAsync(string entityName, string entityId);
        Task<List<AuditLog>> GetAuditsByUserAsync(string userId);
    }
}
