using System.ComponentModel.DataAnnotations;

namespace Coontrera.Application.DTOs
{
    public class AppointmentCreateDTO
    {
        private string? _serviceId;
        private string? _notes;
        private DateOnly? _date;
        private TimeOnly? _time;

        /// <summary>
        /// ID do serviço clínico (aceita ServiceId ou ClinicServiceId).
        /// </summary>
        public string ClinicServiceId
        {
            get => _serviceId ?? string.Empty;
            set => _serviceId = value;
        }

        public string? ServiceId
        {
            get => _serviceId;
            set => _serviceId = value;
        }

        /// <summary>
        /// ID do cliente (opcional na request, pois é obtido do token JWT).
        /// </summary>
        public string? ClientId { get; set; }
        public string? UserId { get; set; }

        /// <summary>
        /// Data da consulta (aceita AppointmentDate ou Date).
        /// </summary>
        public DateOnly AppointmentDate
        {
            get => _date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            set => _date = value;
        }

        public DateOnly? Date
        {
            get => _date;
            set => _date = value;
        }

        /// <summary>
        /// Horário da consulta (aceita AppointmentHour ou TimeSlot "HH:mm").
        /// </summary>
        public TimeOnly AppointmentHour
        {
            get => _time ?? new TimeOnly(8, 0);
            set => _time = value;
        }

        public string? TimeSlot
        {
            get => _time?.ToString("HH:mm");
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && TimeOnly.TryParse(value, out var parsed))
                {
                    _time = parsed;
                }
            }
        }

        /// <summary>
        /// Observações da consulta (aceita Observations ou Notes).
        /// </summary>
        public string? Observations
        {
            get => _notes;
            set => _notes = value;
        }

        public string? Notes
        {
            get => _notes;
            set => _notes = value;
        }
    }
}
