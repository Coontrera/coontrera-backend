using Coontrera.Application.DTOs;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> CreateAppointmentAsync(string userId, AppointmentCreateDTO request);
        Task<AppointmentResponseDTO?> GetAppointmentByIdAsync(string id, string requesterUserId, UserRole requesterRole);
        Task<List<AppointmentResponseDTO>> GetAllAppointmentsAsync(
            string requesterUserId,
            UserRole requesterRole,
            DateOnly? from = null,
            DateOnly? to = null,
            DateOnly? date = null,
            string? clientId = null,
            string? serviceId = null,
            AppointmentStatus? status = null);
        Task<List<AppointmentResponseDTO>> GetMyAppointmentsAsync(string userId);
        Task<AppointmentResponseDTO> UpdateObservationsAsync(string id, string observations, string requesterUserId, UserRole requesterRole);
        Task<AppointmentResponseDTO> ConfirmAppointmentAsync(string id, string adminUserId, UserRole adminRole);
        Task<AppointmentResponseDTO> CompleteAppointmentAsync(string id, string adminUserId, UserRole adminRole);
        Task<AppointmentResponseDTO> CancelAppointmentAsync(string id, string reason, string requesterUserId, UserRole requesterRole);
        Task DeleteAppointmentAsync(string id, string adminUserId, UserRole adminRole);
    }
}
