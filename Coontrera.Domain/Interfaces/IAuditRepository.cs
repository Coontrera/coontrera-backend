using Coontrera.Domain.Models;

namespace Coontrera.Domain.Interfaces
{
    public interface IAuditRepository
    {
        Task<AuditLog> AddAuditAsync(AuditLog audit);
        Task<List<AuditLog>> GetAllAuditsAsync();
        Task<List<AuditLog>> GetAuditsByEntityAsync(string entityName, string entityId);
        Task<List<AuditLog>> GetAuditsByUserAsync(string userId);
    }
}
