using Coontrera.Domain.Models;

namespace Coontrera.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment> AddAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(string id);
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<List<Appointment>> GetAppointmentsByUserIdAsync(string userId);
        Task<List<Appointment>> GetAppointmentsByDateAsync(DateOnly date);
        Task<List<Appointment>> GetAppointmentsByServiceIdAsync(string serviceId);
        Task UpdateAppointmentAsync(Appointment appointment);
        Task DeleteAppointmentAsync(string id);
    }
}
