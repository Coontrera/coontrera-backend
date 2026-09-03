using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;
using Google.Cloud.Firestore;

namespace Coontrera.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "Appointments";

        public AppointmentRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<Appointment> AddAppointmentAsync(Appointment appointment)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(appointment.Id);

            var appointmentData = new Dictionary<string, object>
            {
                { "UserId", appointment.UserId },
                { "ClinicServiceId", appointment.ClinicServiceId },
                { "AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd") },
                { "AppointmentHour", appointment.AppointmentHour.ToString("HH:mm") },
                { "Status", (int)appointment.Status },
                { "Observations", appointment.Observations },
                { "CancelledReason", appointment.CancelledReason },
                { "CancelledBy", appointment.CancelledBy.HasValue ? (int)appointment.CancelledBy.Value : (object?)null! },
                { "DateRegistered", appointment.DateRegistered.ToUniversalTime() }
            };

            await docRef.SetAsync(appointmentData);
            return appointment;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string id)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            return MapSnapshotToAppointment(snapshot);
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            Query query = _db.Collection(CollectionName);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var appointments = new List<Appointment>();
            foreach (var document in querySnapshot.Documents)
            {
                appointments.Add(MapSnapshotToAppointment(document));
            }

            return appointments.OrderByDescending(a => a.AppointmentDate)
                               .ThenBy(a => a.AppointmentHour)
                               .ToList();
        }

        public async Task<List<Appointment>> GetAppointmentsByUserIdAsync(string userId)
        {
            Query query = _db.Collection(CollectionName).WhereEqualTo("UserId", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var appointments = new List<Appointment>();
            foreach (var document in querySnapshot.Documents)
            {
                appointments.Add(MapSnapshotToAppointment(document));
            }

            return appointments.OrderByDescending(a => a.AppointmentDate)
                               .ThenBy(a => a.AppointmentHour)
                               .ToList();
        }

        public async Task<List<Appointment>> GetAppointmentsByDateAsync(DateOnly date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            Query query = _db.Collection(CollectionName).WhereEqualTo("AppointmentDate", dateStr);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var appointments = new List<Appointment>();
            foreach (var document in querySnapshot.Documents)
            {
                appointments.Add(MapSnapshotToAppointment(document));
            }

            return appointments.OrderBy(a => a.AppointmentHour).ToList();
        }

        public async Task<List<Appointment>> GetAppointmentsByServiceIdAsync(string serviceId)
        {
            Query query = _db.Collection(CollectionName).WhereEqualTo("ClinicServiceId", serviceId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var appointments = new List<Appointment>();
            foreach (var document in querySnapshot.Documents)
            {
                appointments.Add(MapSnapshotToAppointment(document));
            }

            return appointments.OrderByDescending(a => a.AppointmentDate)
                               .ThenBy(a => a.AppointmentHour)
                               .ToList();
        }

        public async Task UpdateAppointmentAsync(Appointment appointment)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(appointment.Id);

            var updates = new Dictionary<string, object>
            {
                { "UserId", appointment.UserId },
                { "ClinicServiceId", appointment.ClinicServiceId },
                { "AppointmentDate", appointment.AppointmentDate.ToString("yyyy-MM-dd") },
                { "AppointmentHour", appointment.AppointmentHour.ToString("HH:mm") },
                { "Status", (int)appointment.Status },
                { "Observations", appointment.Observations },
                { "CancelledReason", appointment.CancelledReason },
                { "CancelledBy", appointment.CancelledBy.HasValue ? (int)appointment.CancelledBy.Value : (object?)null! }
            };

            await docRef.UpdateAsync(updates);
        }

        public async Task DeleteAppointmentAsync(string id)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
        }

        private static Appointment MapSnapshotToAppointment(DocumentSnapshot snapshot)
        {
            var dict = snapshot.ToDictionary();

            var userId = dict.TryGetValue("UserId", out var uId) ? uId?.ToString() ?? string.Empty : string.Empty;
            var serviceId = dict.TryGetValue("ClinicServiceId", out var sId) ? sId?.ToString() ?? string.Empty : string.Empty;

            DateOnly appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dict.TryGetValue("AppointmentDate", out var dateObj))
            {
                if (dateObj is string dateStr && DateOnly.TryParse(dateStr, out var parsedDate))
                {
                    appointmentDate = parsedDate;
                }
                else if (dateObj is Timestamp ts)
                {
                    appointmentDate = DateOnly.FromDateTime(ts.ToDateTime());
                }
                else if (dateObj is DateTime dt)
                {
                    appointmentDate = DateOnly.FromDateTime(dt);
                }
            }

            TimeOnly appointmentHour = new TimeOnly(8, 0);
            if (dict.TryGetValue("AppointmentHour", out var hourObj))
            {
                if (hourObj is string hourStr && TimeOnly.TryParse(hourStr, out var parsedHour))
                {
                    appointmentHour = parsedHour;
                }
            }

            var observations = dict.TryGetValue("Observations", out var obs) ? obs?.ToString() ?? string.Empty : string.Empty;

            var appointment = new Appointment(
                userId,
                serviceId,
                appointmentDate,
                appointmentHour,
                observations
            );

            if (dict.TryGetValue("Status", out var statusObj) && statusObj != null)
            {
                appointment.SetStatus((AppointmentStatus)Convert.ToInt32(statusObj));
            }

            var cancelledReason = dict.TryGetValue("CancelledReason", out var reasonObj) ? reasonObj?.ToString() ?? string.Empty : string.Empty;
            UserRole? cancelledBy = null;
            if (dict.TryGetValue("CancelledBy", out var cancelledByObj) && cancelledByObj != null)
            {
                if (int.TryParse(cancelledByObj.ToString(), out var roleVal))
                {
                    cancelledBy = (UserRole)roleVal;
                }
            }
            appointment.SetCancellationInfo(cancelledReason, cancelledBy);

            if (dict.TryGetValue("DateRegistered", out var dateRegObj))
            {
                if (dateRegObj is Timestamp ts)
                {
                    appointment.SetDateRegistered(ts.ToDateTime());
                }
                else if (dateRegObj is DateTime dt)
                {
                    appointment.SetDateRegistered(dt);
                }
                else if (dateRegObj is string dateStr && DateTime.TryParse(dateStr, out var parsedDate))
                {
                    appointment.SetDateRegistered(parsedDate);
                }
            }

            appointment.SetId(snapshot.Id);

            return appointment;
        }
    }
}
