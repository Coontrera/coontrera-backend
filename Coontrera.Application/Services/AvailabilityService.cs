using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAuditService _auditService;

        public AvailabilityService(
            IAvailabilityRepository availabilityRepository,
            IAppointmentRepository appointmentRepository,
            IAuditService auditService)
        {
            _availabilityRepository = availabilityRepository;
            _appointmentRepository = appointmentRepository;
            _auditService = auditService;
        }

        public async Task<List<TimeSlotDTO>> GetAvailableSlotsAsync(DateOnly date)
        {
            var weekday = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            var dayRule = await _availabilityRepository.GetDayAvailabilityAsync(weekday);

            if (dayRule == null || !dayRule.IsOpen)
                return new List<TimeSlotDTO>();

            var blocked = await _availabilityRepository.GetBlockedPeriodsForDateAsync(date);
            if (blocked.Any(b => b.IsFullDay))
                return new List<TimeSlotDTO>();

            var appointments = await _appointmentRepository.GetAppointmentsByDateAsync(date);
            var bookedTimes = appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.AppointmentHour.ToString("HH:mm"))
                .ToHashSet();

            var brazilNow = DateTime.UtcNow.AddHours(-3);
            var slots = new List<TimeSlotDTO>();

            var cursor = ToMinutes(dayRule.StartTime);
            var end = ToMinutes(dayRule.EndTime);
            var slotMinutes = dayRule.SlotMinutes <= 0 ? 60 : dayRule.SlotMinutes;

            while (cursor + slotMinutes <= end)
            {
                var timeStr = FromMinutes(cursor);
                var hour = cursor / 60;
                var minute = cursor % 60;
                var slotDateTime = date.ToDateTime(new TimeOnly(hour, minute));

                var isPast = slotDateTime < brazilNow;
                var isBooked = bookedTimes.Contains(timeStr);
                var isBlocked = blocked.Any(b =>
                {
                    if (b.IsFullDay) return true;
                    if (string.IsNullOrEmpty(b.StartTime) || string.IsNullOrEmpty(b.EndTime)) return false;
                    var blockStart = ToMinutes(b.StartTime);
                    var blockEnd = ToMinutes(b.EndTime);
                    return cursor >= blockStart && cursor < blockEnd;
                });

                slots.Add(new TimeSlotDTO
                {
                    Date = date,
                    Time = timeStr,
                    IsAvailable = !isPast && !isBooked && !isBlocked
                });

                cursor += slotMinutes;
            }

            return slots;
        }

        public async Task<List<DayAvailabilityDTO>> GetWeeklyAvailabilityAsync()
        {
            var days = await _availabilityRepository.GetWeeklyAvailabilityAsync();
            return days.Select(d => new DayAvailabilityDTO
            {
                Weekday = d.Weekday,
                IsOpen = d.IsOpen,
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                SlotMinutes = d.SlotMinutes
            }).ToList();
        }

        public async Task UpdateWeeklyAvailabilityAsync(DayAvailabilityDTO dayDto, string adminUserId)
        {
            var day = new DayAvailability(
                dayDto.Weekday,
                dayDto.IsOpen,
                dayDto.StartTime,
                dayDto.EndTime,
                dayDto.SlotMinutes
            );

            await _availabilityRepository.UpdateDayAvailabilityAsync(day);

            await _auditService.LogAsync(
                entityName: nameof(DayAvailability),
                entityId: day.Weekday.ToString(),
                action: AuditAction.Update,
                userId: adminUserId,
                details: $"Disponibilidade semanal alterada para dia {day.Weekday}: Aberto={day.IsOpen}, Horário={day.StartTime}-{day.EndTime}."
            );
        }

        public async Task<List<BlockedPeriodDTO>> GetBlockedPeriodsAsync(DateOnly? from = null, DateOnly? to = null)
        {
            var blocked = await _availabilityRepository.GetBlockedPeriodsAsync(from, to);
            return blocked.Select(b => new BlockedPeriodDTO
            {
                Id = b.Id,
                Date = b.Date,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Reason = b.Reason,
                IsFullDay = b.IsFullDay,
                DateRegistered = b.DateRegistered
            }).ToList();
        }

        public async Task<BlockedPeriodDTO> BlockDayAsync(BlockDayDTO dto, string adminUserId)
        {
            var period = new BlockedPeriod(dto.Date, dto.Reason);
            await _availabilityRepository.AddBlockedPeriodAsync(period);

            await _auditService.LogAsync(
                entityName: nameof(BlockedPeriod),
                entityId: period.Id,
                action: AuditAction.Create,
                userId: adminUserId,
                details: $"Dia bloqueado: {period.Date:yyyy-MM-dd}. Motivo: '{period.Reason}'."
            );

            return new BlockedPeriodDTO
            {
                Id = period.Id,
                Date = period.Date,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                Reason = period.Reason,
                IsFullDay = period.IsFullDay,
                DateRegistered = period.DateRegistered
            };
        }

        public async Task<BlockedPeriodDTO> BlockSlotAsync(BlockSlotDTO dto, string adminUserId)
        {
            var period = new BlockedPeriod(dto.Date, dto.Reason, dto.StartTime, dto.EndTime);
            await _availabilityRepository.AddBlockedPeriodAsync(period);

            await _auditService.LogAsync(
                entityName: nameof(BlockedPeriod),
                entityId: period.Id,
                action: AuditAction.Create,
                userId: adminUserId,
                details: $"Horário bloqueado em {period.Date:yyyy-MM-dd} de {period.StartTime} às {period.EndTime}. Motivo: '{period.Reason}'."
            );

            return new BlockedPeriodDTO
            {
                Id = period.Id,
                Date = period.Date,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                Reason = period.Reason,
                IsFullDay = period.IsFullDay,
                DateRegistered = period.DateRegistered
            };
        }

        public async Task UnblockAsync(string blockedPeriodId, string adminUserId)
        {
            var period = await _availabilityRepository.GetBlockedPeriodByIdAsync(blockedPeriodId);
            if (period == null)
                throw new KeyNotFoundException("Período bloqueado não encontrado.");

            await _availabilityRepository.DeleteBlockedPeriodAsync(blockedPeriodId);

            await _auditService.LogAsync(
                entityName: nameof(BlockedPeriod),
                entityId: blockedPeriodId,
                action: AuditAction.Delete,
                userId: adminUserId,
                details: $"Desbloqueio efetuado para a data {period.Date:yyyy-MM-dd}."
            );
        }

        public async Task<bool> IsDateFullyBlockedAsync(DateOnly date)
        {
            var weekday = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
            var dayRule = await _availabilityRepository.GetDayAvailabilityAsync(weekday);

            if (dayRule == null || !dayRule.IsOpen)
                return true;

            var blocked = await _availabilityRepository.GetBlockedPeriodsForDateAsync(date);
            return blocked.Any(b => b.IsFullDay);
        }

        private static int ToMinutes(string hhmm)
        {
            var parts = hhmm.Split(':');
            return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
        }

        private static string FromMinutes(int minutes)
        {
            var h = (minutes / 60).ToString("D2");
            var m = (minutes % 60).ToString("D2");
            return $"{h}:{m}";
        }
    }
}
