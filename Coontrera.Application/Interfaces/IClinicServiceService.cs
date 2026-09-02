using Coontrera.Application.DTOs;

namespace Coontrera.Application.Interfaces
{
    public interface IClinicServiceService
    {
        Task<ClinicServiceResponseDTO> CreateClinicServiceAsync(ClinicServiceCreateDTO request);
        Task<ClinicServiceResponseDTO?> GetClinicServiceByIdAsync(string id);
        Task<List<ClinicServiceResponseDTO>> GetAllClinicServicesAsync();
        Task UpdateClinicServiceAsync(string id, ClinicServiceCreateDTO request);
        Task DeleteClinicServiceAsync(string id);
        Task ActivateClinicServiceAsync(string id);
        Task DeactivateClinicServiceAsync(string id);
    }
}
