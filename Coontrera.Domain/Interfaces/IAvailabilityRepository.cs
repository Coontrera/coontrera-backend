using Coontrera.Domain.Models;

namespace Coontrera.Domain.Interfaces
{
    public interface IAvailabilityRepository
    {
        Task<List<DayAvailability>> GetWeeklyAvailabilityAsync();
        Task<DayAvailability?> GetDayAvailabilityAsync(int weekday);
        Task UpdateDayAvailabilityAsync(DayAvailability day);
        Task<BlockedPeriod> AddBlockedPeriodAsync(BlockedPeriod period);
        Task<List<BlockedPeriod>> GetBlockedPeriodsAsync(DateOnly? from = null, DateOnly? to = null);
        Task<List<BlockedPeriod>> GetBlockedPeriodsForDateAsync(DateOnly date);
        Task<BlockedPeriod?> GetBlockedPeriodByIdAsync(string id);
        Task DeleteBlockedPeriodAsync(string id);
    }
}
