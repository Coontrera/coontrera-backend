using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Google.Cloud.Firestore;

namespace Coontrera.Infrastructure.Repositories
{
    public class ClinicServiceRepository : IClinicServiceRepository
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "ClinicServices";

        public ClinicServiceRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<ClinicService> AddClinicServiceAsync(ClinicService service)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(service.Id);

            var serviceData = new Dictionary<string, object>
            {
                { "Title", service.Title },
                { "Description", service.Description },
                { "Benefits", service.Benefits },
                { "ImageUrl", service.ImageUrl },
                { "ImageAlt", service.ImageAlt },
                { "CtaText", service.CtaText },
                { "IconAsset", service.IconAsset },
                { "IsActive", service.IsActive },
                { "DateRegistered", service.DateRegistered.ToUniversalTime() }
            };

            await docRef.SetAsync(serviceData);
            return service;
        }

        public async Task<ClinicService?> GetClinicServiceByIdAsync(string id)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            return MapSnapshotToClinicService(snapshot);
        }

        public async Task<List<ClinicService>> GetAllClinicServicesAsync()
        {
            Query query = _db.Collection(CollectionName);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var services = new List<ClinicService>();
            foreach (var document in querySnapshot.Documents)
            {
                services.Add(MapSnapshotToClinicService(document));
            }

            return services;
        }

        public async Task UpdateClinicServiceAsync(ClinicService service)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(service.Id);
            var updates = new Dictionary<string, object>
            {
                { "Title", service.Title },
                { "Description", service.Description },
                { "Benefits", service.Benefits },
                { "ImageUrl", service.ImageUrl },
                { "ImageAlt", service.ImageAlt },
                { "CtaText", service.CtaText },
                { "IconAsset", service.IconAsset },
                { "IsActive", service.IsActive }
            };

            await docRef.UpdateAsync(updates);
        }

        public async Task DeleteClinicServiceAsync(string id)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(id);
            await docRef.DeleteAsync();
        }

        private ClinicService MapSnapshotToClinicService(DocumentSnapshot snapshot)
        {
            var dict = snapshot.ToDictionary();
            
            var benefits = new List<string>();
            if (dict.TryGetValue("Benefits", out var benefitsObj) && benefitsObj is IEnumerable<object> benefitsObjList)
            {
                foreach (var obj in benefitsObjList)
                {
                    if (obj != null) benefits.Add(obj.ToString()!);
                }
            }

            var service = new ClinicService(
                dict.TryGetValue("Title", out var title) ? title?.ToString() ?? string.Empty : string.Empty,
                dict.TryGetValue("Description", out var desc) ? desc?.ToString() ?? string.Empty : string.Empty,
                dict.TryGetValue("ImageUrl", out var imgUrl) ? imgUrl?.ToString() ?? string.Empty : string.Empty,
                dict.TryGetValue("ImageAlt", out var imgAlt) ? imgAlt?.ToString() ?? string.Empty : string.Empty,
                benefits,
                dict.TryGetValue("CtaText", out var cta) ? cta?.ToString() ?? string.Empty : string.Empty,
                dict.TryGetValue("IconAsset", out var icon) ? icon?.ToString() ?? string.Empty : string.Empty
            );

            if (dict.TryGetValue("IsActive", out var isActiveVal) && isActiveVal is bool isActive)
            {
                if (!isActive)
                {
                    service.Deactivate();
                }
            }

            if (dict.TryGetValue("DateRegistered", out var dateVal))
            {
                if (dateVal is Timestamp ts)
                {
                    service.SetDateRegistered(ts.ToDateTime());
                }
                else if (dateVal is DateTime dt)
                {
                    service.SetDateRegistered(dt);
                }
                else if (dateVal is string dateStr && DateTime.TryParse(dateStr, out var parsedDate))
                {
                    service.SetDateRegistered(parsedDate);
                }
            }

            service.SetId(snapshot.Id);

            return service;
        }
    }
}
