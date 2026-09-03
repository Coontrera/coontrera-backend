using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Google.Cloud.Firestore;

namespace Coontrera.Infrastructure.Repositories
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly FirestoreDb _db;
        private const string WeeklyCollectionName = "WeeklyAvailability";
        private const string BlockedCollectionName = "BlockedPeriods";

        public AvailabilityRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<List<DayAvailability>> GetWeeklyAvailabilityAsync()
        {
            var collection = _db.Collection(WeeklyCollectionName);
            var snapshot = await collection.GetSnapshotAsync();

            if (snapshot.Documents.Count == 0)
            {
                await SeedDefaultWeeklyAvailabilityAsync();
                snapshot = await collection.GetSnapshotAsync();
            }

            var days = new List<DayAvailability>();
            foreach (var doc in snapshot.Documents)
            {
                days.Add(MapSnapshotToDayAvailability(doc));
            }

            return days.OrderBy(d => d.Weekday).ToList();
        }

        public async Task<DayAvailability?> GetDayAvailabilityAsync(int weekday)
        {
            var docRef = _db.Collection(WeeklyCollectionName).Document(weekday.ToString());
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                await GetWeeklyAvailabilityAsync();
                snapshot = await docRef.GetSnapshotAsync();
                if (!snapshot.Exists) return null;
            }

            return MapSnapshotToDayAvailability(snapshot);
        }

        public async Task UpdateDayAvailabilityAsync(DayAvailability day)
        {
            var docRef = _db.Collection(WeeklyCollectionName).Document(day.Weekday.ToString());
            var data = new Dictionary<string, object>
            {
                { "Weekday", day.Weekday },
                { "IsOpen", day.IsOpen },
                { "StartTime", day.StartTime },
                { "EndTime", day.EndTime },
                { "SlotMinutes", day.SlotMinutes }
            };

            await docRef.SetAsync(data);
        }

        public async Task<BlockedPeriod> AddBlockedPeriodAsync(BlockedPeriod period)
        {
            var docRef = _db.Collection(BlockedCollectionName).Document(period.Id);
            var data = new Dictionary<string, object>
            {
                { "Date", period.Date.ToString("yyyy-MM-dd") },
                { "StartTime", period.StartTime ?? (object?)null! },
                { "EndTime", period.EndTime ?? (object?)null! },
                { "Reason", period.Reason },
                { "DateRegistered", period.DateRegistered.ToUniversalTime() }
            };

            await docRef.SetAsync(data);
            return period;
        }

        public async Task<List<BlockedPeriod>> GetBlockedPeriodsAsync(DateOnly? from = null, DateOnly? to = null)
        {
            Query query = _db.Collection(BlockedCollectionName);
            var snapshot = await query.GetSnapshotAsync();

            var list = new List<BlockedPeriod>();
            foreach (var doc in snapshot.Documents)
            {
                list.Add(MapSnapshotToBlockedPeriod(doc));
            }

            if (from.HasValue)
            {
                list = list.Where(b => b.Date >= from.Value).ToList();
            }

            if (to.HasValue)
            {
                list = list.Where(b => b.Date <= to.Value).ToList();
            }

            return list.OrderBy(b => b.Date).ToList();
        }

        public async Task<List<BlockedPeriod>> GetBlockedPeriodsForDateAsync(DateOnly date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var query = _db.Collection(BlockedCollectionName).WhereEqualTo("Date", dateStr);
            var snapshot = await query.GetSnapshotAsync();

            var list = new List<BlockedPeriod>();
            foreach (var doc in snapshot.Documents)
            {
                list.Add(MapSnapshotToBlockedPeriod(doc));
            }

            return list;
        }

        public async Task<BlockedPeriod?> GetBlockedPeriodByIdAsync(string id)
        {
            var docRef = _db.Collection(BlockedCollectionName).Document(id);
            var snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            return MapSnapshotToBlockedPeriod(snapshot);
        }

        public async Task DeleteBlockedPeriodAsync(string id)
        {
            var docRef = _db.Collection(BlockedCollectionName).Document(id);
            await docRef.DeleteAsync();
        }

        private async Task SeedDefaultWeeklyAvailabilityAsync()
        {
            for (var weekday = 1; weekday <= 7; weekday++)
            {
                var isWeekend = weekday == 6 || weekday == 7;
                var day = new DayAvailability(
                    weekday: weekday,
                    isOpen: !isWeekend,
                    startTime: "08:00",
                    endTime: "18:00",
                    slotMinutes: 60
                );
                await UpdateDayAvailabilityAsync(day);
            }
        }

        private static DayAvailability MapSnapshotToDayAvailability(DocumentSnapshot snapshot)
        {
            var dict = snapshot.ToDictionary();

            var weekday = dict.TryGetValue("Weekday", out var wObj) ? Convert.ToInt32(wObj) : 1;
            var isOpen = dict.TryGetValue("IsOpen", out var oObj) && Convert.ToBoolean(oObj);
            var startTime = dict.TryGetValue("StartTime", out var sObj) ? sObj?.ToString() ?? "08:00" : "08:00";
            var endTime = dict.TryGetValue("EndTime", out var eObj) ? eObj?.ToString() ?? "18:00" : "18:00";
            var slotMinutes = dict.TryGetValue("SlotMinutes", out var slotObj) ? Convert.ToInt32(slotObj) : 60;

            return new DayAvailability(weekday, isOpen, startTime, endTime, slotMinutes);
        }

        private static BlockedPeriod MapSnapshotToBlockedPeriod(DocumentSnapshot snapshot)
        {
            var dict = snapshot.ToDictionary();

            DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dict.TryGetValue("Date", out var dObj))
            {
                if (dObj is string dStr && DateOnly.TryParse(dStr, out var parsedDate))
                    date = parsedDate;
                else if (dObj is Timestamp ts)
                    date = DateOnly.FromDateTime(ts.ToDateTime());
                else if (dObj is DateTime dt)
                    date = DateOnly.FromDateTime(dt);
            }

            var reason = dict.TryGetValue("Reason", out var rObj) ? rObj?.ToString() ?? string.Empty : string.Empty;
            var startTime = dict.TryGetValue("StartTime", out var stObj) ? stObj?.ToString() : null;
            var endTime = dict.TryGetValue("EndTime", out var etObj) ? etObj?.ToString() : null;

            var period = new BlockedPeriod(date, reason, startTime, endTime);
            period.SetId(snapshot.Id);

            if (dict.TryGetValue("DateRegistered", out var drObj))
            {
                if (drObj is Timestamp ts)
                    period.SetDateRegistered(ts.ToDateTime());
                else if (drObj is DateTime dt)
                    period.SetDateRegistered(dt);
            }

            return period;
        }
    }
}
