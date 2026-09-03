using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Services
{
    public class ClinicServiceService : IClinicServiceService
    {
        private readonly IClinicServiceRepository _clinicServiceRepository;
        private readonly IAuditService _auditService;

        public ClinicServiceService(
            IClinicServiceRepository clinicServiceRepository,
            IAuditService auditService)
        {
            _clinicServiceRepository = clinicServiceRepository;
            _auditService = auditService;
        }

        public async Task<ClinicServiceResponseDTO> CreateClinicServiceAsync(ClinicServiceCreateDTO request)
        {
            var service = new ClinicService(
                request.Title,
                request.Description,
                request.ImageUrl,
                request.ImageAlt,
                request.Benefits,
                request.CtaText,
                request.IconAsset
            );

            await _clinicServiceRepository.AddClinicServiceAsync(service);

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: service.Id,
                action: AuditAction.Create,
                details: $"Serviço criado: '{service.Title}'."
            );

            return MapToResponseDTO(service);
        }

        public async Task<ClinicServiceResponseDTO?> GetClinicServiceByIdAsync(string id)
        {
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(id);
            if (service == null) return null;

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: id,
                action: AuditAction.Read,
                details: $"Serviço consultado por ID: '{service.Title}'."
            );

            return MapToResponseDTO(service);
        }

        public async Task<List<ClinicServiceResponseDTO>> GetAllClinicServicesAsync()
        {
            var services = await _clinicServiceRepository.GetAllClinicServicesAsync();

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: "All",
                action: AuditAction.Read,
                details: $"Listagem de todos os serviços clínicos (Total: {services.Count})."
            );

            return services.Select(MapToResponseDTO).ToList();
        }

        public async Task UpdateClinicServiceAsync(string id, ClinicServiceCreateDTO request)
        {
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(id);
            if (service == null)
                throw new KeyNotFoundException("Clinic service not found.");

            service.Update(
                request.Title,
                request.Description,
                request.ImageUrl,
                request.ImageAlt,
                request.Benefits,
                request.CtaText,
                request.IconAsset
            );

            await _clinicServiceRepository.UpdateClinicServiceAsync(service);

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: id,
                action: AuditAction.Update,
                details: $"Serviço atualizado: '{service.Title}'."
            );
        }

        public async Task DeleteClinicServiceAsync(string id)
        {
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(id);
            if (service == null)
                throw new KeyNotFoundException("Clinic service not found.");

            await _clinicServiceRepository.DeleteClinicServiceAsync(id);

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: id,
                action: AuditAction.Delete,
                details: $"Serviço excluído: '{service.Title}'."
            );
        }

        public async Task ActivateClinicServiceAsync(string id)
        {
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(id);
            if (service == null)
                throw new KeyNotFoundException("Clinic service not found.");

            service.Reactivate();
            await _clinicServiceRepository.UpdateClinicServiceAsync(service);

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: id,
                action: AuditAction.Activate,
                details: $"Serviço ativado: '{service.Title}'."
            );
        }

        public async Task DeactivateClinicServiceAsync(string id)
        {
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(id);
            if (service == null)
                throw new KeyNotFoundException("Clinic service not found.");

            service.Deactivate();
            await _clinicServiceRepository.UpdateClinicServiceAsync(service);

            await _auditService.LogAsync(
                entityName: nameof(ClinicService),
                entityId: id,
                action: AuditAction.Deactivate,
                details: $"Serviço inativado: '{service.Title}'."
            );
        }

        private ClinicServiceResponseDTO MapToResponseDTO(ClinicService service)
        {
            return new ClinicServiceResponseDTO
            {
                Id = service.Id,
                Title = service.Title,
                Description = service.Description,
                Benefits = service.Benefits,
                ImageUrl = service.ImageUrl,
                ImageAlt = service.ImageAlt,
                CtaText = service.CtaText,
                IconAsset = service.IconAsset,
                IsActive = service.IsActive,
                DateRegistered = service.DateRegistered
            };
        }
    }
}
