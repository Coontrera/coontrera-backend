using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.DTOs
{
    public class AppointmentResponseDTO
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public string ClientId => UserId;

        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        public string ClinicServiceId { get; set; } = string.Empty;
        public string ServiceId => ClinicServiceId;

        public string ClinicServiceTitle { get; set; } = string.Empty;

        public DateOnly AppointmentDate { get; set; }
        public DateTime Date => AppointmentDate.ToDateTime(TimeOnly.MinValue);

        public TimeOnly AppointmentHour { get; set; }
        public string TimeSlot => AppointmentHour.ToString("HH:mm");

        public AppointmentStatus Status { get; set; }
        public string StatusDescription { get; set; } = string.Empty;

        public string Observations { get; set; } = string.Empty;
        public string Notes => Observations;

        public string CancelledReason { get; set; } = string.Empty;
        public string CancelReason => CancelledReason;

        public UserRole? CancelledBy { get; set; }

        public DateTime DateRegistered { get; set; }
        public DateTime CreatedAt => DateRegistered;

        public bool IsUpcoming => Status == AppointmentStatus.Pending || Status == AppointmentStatus.Confirmed;
    }
}
