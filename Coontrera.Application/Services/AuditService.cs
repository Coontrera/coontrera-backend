using Coontrera.Application.Interfaces;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task LogAsync(
            string entityName,
            string entityId,
            AuditAction action,
            string? userId = null,
            string? userEmail = null,
            string? details = null,
            string? ipAddress = null)
        {
            try
            {
                var auditLog = new AuditLog(
                    entityName,
                    entityId,
                    action,
                    userId,
                    userEmail,
                    details,
                    ipAddress
                );

                await _auditRepository.AddAuditAsync(auditLog);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditService Error] Falha ao gravar log de auditoria para {entityName} (ID: {entityId}, Ação: {action}): {ex.Message}");
            }
        }

        public async Task<List<AuditLog>> GetAllAuditsAsync()
        {
            return await _auditRepository.GetAllAuditsAsync();
        }

        public async Task<List<AuditLog>> GetAuditsByEntityAsync(string entityName, string entityId)
        {
            return await _auditRepository.GetAuditsByEntityAsync(entityName, entityId);
        }

        public async Task<List<AuditLog>> GetAuditsByUserAsync(string userId)
        {
            return await _auditRepository.GetAuditsByUserAsync(userId);
        }
    }
}
