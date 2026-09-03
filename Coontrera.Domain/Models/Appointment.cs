using Coontrera.Domain.Models.Enum;

namespace Coontrera.Domain.Models
{
    public class Appointment
    {
        public string Id { get; private set; } = string.Empty;
        public string UserId { get; private set; } = string.Empty;
        public string ClinicServiceId { get; private set; } = string.Empty;
        public DateOnly AppointmentDate { get; private set; }
        public TimeOnly AppointmentHour { get; private set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;
        public string Observations { get; private set; } = string.Empty;
        public string CancelledReason { get; private set; } = string.Empty;
        public UserRole? CancelledBy { get; private set; }
        public DateTime DateRegistered { get; private set; } = DateTime.UtcNow;

        protected Appointment() { }

        public Appointment(
            string userId,
            string clinicServiceId,
            DateOnly appointmentDate,
            TimeOnly appointmentHour,
            string observations = "")
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId não pode ser vazio.", nameof(userId));

            if (string.IsNullOrWhiteSpace(clinicServiceId))
                throw new ArgumentException("ClinicServiceId não pode ser vazio.", nameof(clinicServiceId));

            ValidateAppointmentHour(appointmentHour);

            Id = Guid.NewGuid().ToString();
            UserId = userId;
            ClinicServiceId = clinicServiceId;
            AppointmentDate = appointmentDate;
            AppointmentHour = appointmentHour;
            Observations = observations ?? string.Empty;
            Status = AppointmentStatus.Pending;
            DateRegistered = DateTime.UtcNow;
        }

        public void UpdateObservations(string observations)
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Não é possível alterar observações de uma consulta cancelada.");

            Observations = observations ?? string.Empty;
        }

        public void Reschedule(DateOnly newDate, TimeOnly newHour)
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Não é possível reagendar uma consulta cancelada.");

            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Não é possível reagendar uma consulta já concluída.");

            ValidateAppointmentHour(newHour);

            AppointmentDate = newDate;
            AppointmentHour = newHour;
        }

        public void Confirm()
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Não é possível confirmar uma consulta cancelada.");

            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("A consulta já foi concluída.");

            Status = AppointmentStatus.Confirmed;
        }

        public void Complete()
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Não é possível concluir uma consulta cancelada.");

            Status = AppointmentStatus.Completed;
        }

        public void Cancel(string reason, UserRole cancelledBy)
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Não é possível cancelar uma consulta que já foi concluída.");

            Status = AppointmentStatus.Cancelled;
            CancelledReason = string.IsNullOrWhiteSpace(reason) ? "Cancelado sem motivo especificado." : reason;
            CancelledBy = cancelledBy;
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public void SetDateRegistered(DateTime dateRegistered)
        {
            DateRegistered = dateRegistered;
        }

        public void SetStatus(AppointmentStatus status)
        {
            Status = status;
        }

        public void SetCancellationInfo(string reason, UserRole? cancelledBy)
        {
            CancelledReason = reason ?? string.Empty;
            CancelledBy = cancelledBy;
        }

        private static void ValidateAppointmentHour(TimeOnly hour)
        {
            var minHour = new TimeOnly(8, 0);
            var maxHour = new TimeOnly(17, 0);

            if (hour < minHour || hour > maxHour)
            {
                throw new ArgumentException("O horário de agendamento deve ser entre 08:00 e 17:00.");
            }
        }
    }
}