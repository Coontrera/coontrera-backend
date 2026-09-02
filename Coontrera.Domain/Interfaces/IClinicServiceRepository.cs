using Coontrera.Domain.Models;

namespace Coontrera.Domain.Interfaces
{
    public interface IClinicServiceRepository
    {
        Task<ClinicService> AddClinicServiceAsync(ClinicService service);
        Task<ClinicService?> GetClinicServiceByIdAsync(string id);
        Task<List<ClinicService>> GetAllClinicServicesAsync();
        Task UpdateClinicServiceAsync(ClinicService service);
        Task DeleteClinicServiceAsync(string id);
    }
}
