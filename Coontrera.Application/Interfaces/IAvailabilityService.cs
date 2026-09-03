using Coontrera.Application.DTOs;

namespace Coontrera.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task<List<TimeSlotDTO>> GetAvailableSlotsAsync(DateOnly date);
        Task<List<DayAvailabilityDTO>> GetWeeklyAvailabilityAsync();
        Task UpdateWeeklyAvailabilityAsync(DayAvailabilityDTO dayDto, string adminUserId);
        Task<List<BlockedPeriodDTO>> GetBlockedPeriodsAsync(DateOnly? from = null, DateOnly? to = null);
        Task<BlockedPeriodDTO> BlockDayAsync(BlockDayDTO dto, string adminUserId);
        Task<BlockedPeriodDTO> BlockSlotAsync(BlockSlotDTO dto, string adminUserId);
        Task UnblockAsync(string blockedPeriodId, string adminUserId);
        Task<bool> IsDateFullyBlockedAsync(DateOnly date);
    }
}
