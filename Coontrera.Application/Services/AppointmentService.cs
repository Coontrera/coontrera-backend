using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClinicServiceRepository _clinicServiceRepository;
        private readonly IAuditService _auditService;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IUserRepository userRepository,
            IClinicServiceRepository clinicServiceRepository,
            IAuditService auditService)
        {
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
            _clinicServiceRepository = clinicServiceRepository;
            _auditService = auditService;
        }

        public async Task<AppointmentResponseDTO> CreateAppointmentAsync(string userId, AppointmentCreateDTO request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            if (!user.IsActive)
                throw new InvalidOperationException("Usuário inativo não pode realizar agendamentos.");

            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(request.ClinicServiceId);
            if (service == null)
                throw new KeyNotFoundException("Serviço clínico não encontrado.");

            if (!service.IsActive)
                throw new InvalidOperationException("O serviço clínico selecionado não está ativo.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (request.AppointmentDate < today)
                throw new ArgumentException("A data do agendamento não pode ser anterior à data de hoje.");

            var appointment = new Appointment(
                userId,
                request.ClinicServiceId,
                request.AppointmentDate,
                request.AppointmentHour,
                request.Observations ?? string.Empty
            );

            await _appointmentRepository.AddAppointmentAsync(appointment);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: appointment.Id,
                action: AuditAction.Create,
                userId: user.Id,
                userEmail: user.Email,
                details: $"Agendamento criado para '{service.Title}' em {appointment.AppointmentDate:yyyy-MM-dd} às {appointment.AppointmentHour:HH:mm} com status Pendente."
            );

            return MapToResponseDTO(appointment, user.Name, user.Email, service.Title);
        }

        public async Task<AppointmentResponseDTO?> GetAppointmentByIdAsync(string id, string requesterUserId, UserRole requesterRole)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return null;

            if (requesterRole != UserRole.Admin && requesterRole != UserRole.Funcionario && appointment.UserId != requesterUserId)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para visualizar este agendamento.");
            }

            var user = await _userRepository.GetUserByIdAsync(appointment.UserId);
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(appointment.ClinicServiceId);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: id,
                action: AuditAction.Read,
                userId: requesterUserId,
                details: $"Consulta de agendamento por ID."
            );

            return MapToResponseDTO(appointment, user?.Name, user?.Email, service?.Title);
        }

        public async Task<List<AppointmentResponseDTO>> GetAllAppointmentsAsync(
            string requesterUserId,
            UserRole requesterRole,
            DateOnly? from = null,
            DateOnly? to = null,
            DateOnly? date = null,
            string? clientId = null,
            string? serviceId = null,
            AppointmentStatus? status = null)
        {
            List<Appointment> appointments;

            if (requesterRole == UserRole.Admin || requesterRole == UserRole.Funcionario)
            {
                appointments = await _appointmentRepository.GetAllAppointmentsAsync();
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    appointments = appointments.Where(a => a.UserId == clientId).ToList();
                }
            }
            else
            {
                appointments = await _appointmentRepository.GetAppointmentsByUserIdAsync(requesterUserId);
            }

            if (date.HasValue)
            {
                appointments = appointments.Where(a => a.AppointmentDate == date.Value).ToList();
            }

            if (from.HasValue)
            {
                appointments = appointments.Where(a => a.AppointmentDate >= from.Value).ToList();
            }

            if (to.HasValue)
            {
                appointments = appointments.Where(a => a.AppointmentDate <= to.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(serviceId))
            {
                appointments = appointments.Where(a => a.ClinicServiceId == serviceId).ToList();
            }

            if (status.HasValue)
            {
                appointments = appointments.Where(a => a.Status == status.Value).ToList();
            }

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: "All",
                action: AuditAction.Read,
                userId: requesterUserId,
                details: $"Listagem de agendamentos com filtros (Total: {appointments.Count})."
            );

            return await MapListToResponseDTOAsync(appointments);
        }

        public async Task<List<AppointmentResponseDTO>> GetMyAppointmentsAsync(string userId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByUserIdAsync(userId);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: "My",
                action: AuditAction.Read,
                userId: userId,
                details: $"Listagem dos próprios agendamentos (Total: {appointments.Count})."
            );

            return await MapListToResponseDTOAsync(appointments);
        }

        public async Task<AppointmentResponseDTO> UpdateObservationsAsync(string id, string observations, string requesterUserId, UserRole requesterRole)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException("Agendamento não encontrado.");

            if (requesterRole != UserRole.Admin && requesterRole != UserRole.Funcionario && appointment.UserId != requesterUserId)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para alterar observações deste agendamento.");
            }

            appointment.UpdateObservations(observations);
            await _appointmentRepository.UpdateAppointmentAsync(appointment);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: id,
                action: AuditAction.Update,
                userId: requesterUserId,
                details: "Observações do agendamento atualizadas."
            );

            var user = await _userRepository.GetUserByIdAsync(appointment.UserId);
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(appointment.ClinicServiceId);

            return MapToResponseDTO(appointment, user?.Name, user?.Email, service?.Title);
        }

        public async Task<AppointmentResponseDTO> ConfirmAppointmentAsync(string id, string adminUserId, UserRole adminRole)
        {
            if (adminRole != UserRole.Admin && adminRole != UserRole.Funcionario)
            {
                throw new UnauthorizedAccessException("Apenas administradores ou funcionários podem confirmar consultas.");
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException("Agendamento não encontrado.");

            appointment.Confirm();
            await _appointmentRepository.UpdateAppointmentAsync(appointment);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: id,
                action: AuditAction.Update,
                userId: adminUserId,
                details: "Consulta confirmada pelo administrador."
            );

            var user = await _userRepository.GetUserByIdAsync(appointment.UserId);
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(appointment.ClinicServiceId);

            return MapToResponseDTO(appointment, user?.Name, user?.Email, service?.Title);
        }

        public async Task<AppointmentResponseDTO> CompleteAppointmentAsync(string id, string adminUserId, UserRole adminRole)
        {
            if (adminRole != UserRole.Admin && adminRole != UserRole.Funcionario)
            {
                throw new UnauthorizedAccessException("Apenas administradores ou funcionários podem concluir consultas.");
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException("Agendamento não encontrado.");

            appointment.Complete();
            await _appointmentRepository.UpdateAppointmentAsync(appointment);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: id,
                action: AuditAction.Update,
                userId: adminUserId,
                details: "Consulta marcada como concluída pelo administrador."
            );

            var user = await _userRepository.GetUserByIdAsync(appointment.UserId);
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(appointment.ClinicServiceId);

            return MapToResponseDTO(appointment, user?.Name, user?.Email, service?.Title);
        }

        public async Task<AppointmentResponseDTO> CancelAppointmentAsync(string id, string reason, string requesterUserId, UserRole requesterRole)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException("Agendamento não encontrado.");

            var isStaff = requesterRole == UserRole.Admin || requesterRole == UserRole.Funcionario;
            if (!isStaff && appointment.UserId != requesterUserId)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para cancelar este agendamento.");
            }

            var cancelledByRole = isStaff ? UserRole.Admin : UserRole.User;
            appointment.Cancel(reason, cancelledByRole);
            await _appointmentRepository.UpdateAppointmentAsync(appointment);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: id,
                action: AuditAction.Update,
                userId: requesterUserId,
                details: $"Agendamento cancelado por {cancelledByRole}. Motivo: '{appointment.CancelledReason}'."
            );

            var user = await _userRepository.GetUserByIdAsync(appointment.UserId);
            var service = await _clinicServiceRepository.GetClinicServiceByIdAsync(appointment.ClinicServiceId);

            return MapToResponseDTO(appointment, user?.Name, user?.Email, service?.Title);
        }

        public async Task DeleteAppointmentAsync(string id, string adminUserId, UserRole adminRole)
        {
            if (adminRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Apenas administradores podem excluir agendamentos.");
            }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
                throw new KeyNotFoundException("Agendamento não encontrado.");

            await _appointmentRepository.DeleteAppointmentAsync(id);

            await _auditService.LogAsync(
                entityName: nameof(Appointment),
                entityId: id,
                action: AuditAction.Delete,
                userId: adminUserId,
                details: $"Agendamento excluído pelo administrador."
            );
        }

        private static AppointmentResponseDTO MapToResponseDTO(
            Appointment appointment,
            string? userName = null,
            string? userEmail = null,
            string? clinicServiceTitle = null)
        {
            return new AppointmentResponseDTO
            {
                Id = appointment.Id,
                UserId = appointment.UserId,
                UserName = userName ?? string.Empty,
                UserEmail = userEmail ?? string.Empty,
                ClinicServiceId = appointment.ClinicServiceId,
                ClinicServiceTitle = clinicServiceTitle ?? string.Empty,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentHour = appointment.AppointmentHour,
                Status = appointment.Status,
                StatusDescription = GetStatusDescription(appointment.Status),
                Observations = appointment.Observations,
                CancelledReason = appointment.CancelledReason,
                CancelledBy = appointment.CancelledBy,
                DateRegistered = appointment.DateRegistered
            };
        }

        private async Task<List<AppointmentResponseDTO>> MapListToResponseDTOAsync(List<Appointment> appointments)
        {
            var userIds = appointments.Select(a => a.UserId).Distinct().ToList();
            var serviceIds = appointments.Select(a => a.ClinicServiceId).Distinct().ToList();

            var userDict = new Dictionary<string, User?>();
            foreach (var uid in userIds)
            {
                userDict[uid] = await _userRepository.GetUserByIdAsync(uid);
            }

            var serviceDict = new Dictionary<string, ClinicService?>();
            foreach (var sid in serviceIds)
            {
                serviceDict[sid] = await _clinicServiceRepository.GetClinicServiceByIdAsync(sid);
            }

            return appointments.Select(a =>
            {
                userDict.TryGetValue(a.UserId, out var user);
                serviceDict.TryGetValue(a.ClinicServiceId, out var service);
                return MapToResponseDTO(a, user?.Name, user?.Email, service?.Title);
            }).ToList();
        }

        private static string GetStatusDescription(AppointmentStatus status)
        {
            return status switch
            {
                AppointmentStatus.Pending => "Pendente",
                AppointmentStatus.Confirmed => "Confirmado",
                AppointmentStatus.Cancelled => "Cancelado",
                AppointmentStatus.Completed => "Concluído",
                _ => status.ToString()
            };
        }
    }
}
