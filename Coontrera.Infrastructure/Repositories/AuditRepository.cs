using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;
using Google.Cloud.Firestore;

namespace Coontrera.Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "Audits";

        public AuditRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<AuditLog> AddAuditAsync(AuditLog audit)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(audit.Id);

            var auditData = new Dictionary<string, object>
            {
                { "EntityName", audit.EntityName },
                { "EntityId", audit.EntityId },
                { "Action", (int)audit.Action },
                { "UserId", audit.UserId ?? string.Empty },
                { "UserEmail", audit.UserEmail ?? string.Empty },
                { "Details", audit.Details ?? string.Empty },
                { "IpAddress", audit.IpAddress ?? string.Empty },
                { "Timestamp", audit.Timestamp.ToUniversalTime() }
            };

            await docRef.SetAsync(auditData);
            return audit;
        }

        public async Task<List<AuditLog>> GetAllAuditsAsync()
        {
            Query query = _db.Collection(CollectionName).OrderByDescending("Timestamp");
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var audits = new List<AuditLog>();
            foreach (var document in querySnapshot.Documents)
            {
                audits.Add(MapSnapshotToAuditLog(document));
            }

            return audits;
        }

        public async Task<List<AuditLog>> GetAuditsByEntityAsync(string entityName, string entityId)
        {
            Query query = _db.Collection(CollectionName)
                .WhereEqualTo("EntityName", entityName)
                .WhereEqualTo("EntityId", entityId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var audits = new List<AuditLog>();
            foreach (var document in querySnapshot.Documents)
            {
                audits.Add(MapSnapshotToAuditLog(document));
            }

            return audits.OrderByDescending(a => a.Timestamp).ToList();
        }

        public async Task<List<AuditLog>> GetAuditsByUserAsync(string userId)
        {
            Query query = _db.Collection(CollectionName).WhereEqualTo("UserId", userId);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            var audits = new List<AuditLog>();
            foreach (var document in querySnapshot.Documents)
            {
                audits.Add(MapSnapshotToAuditLog(document));
            }

            return audits.OrderByDescending(a => a.Timestamp).ToList();
        }

        private AuditLog MapSnapshotToAuditLog(DocumentSnapshot snapshot)
        {
            var dict = snapshot.ToDictionary();

            var entityName = dict.ContainsKey("EntityName") ? dict["EntityName"]?.ToString() ?? string.Empty : string.Empty;
            var entityId = dict.ContainsKey("EntityId") ? dict["EntityId"]?.ToString() ?? string.Empty : string.Empty;
            var action = dict.ContainsKey("Action") ? (AuditAction)Convert.ToInt32(dict["Action"]) : AuditAction.Read;
            var userId = dict.ContainsKey("UserId") ? dict["UserId"]?.ToString() : null;
            var userEmail = dict.ContainsKey("UserEmail") ? dict["UserEmail"]?.ToString() : null;
            var details = dict.ContainsKey("Details") ? dict["Details"]?.ToString() : null;
            var ipAddress = dict.ContainsKey("IpAddress") ? dict["IpAddress"]?.ToString() : null;

            var auditLog = new AuditLog(
                entityName,
                entityId,
                action,
                userId,
                userEmail,
                details,
                ipAddress
            );

            auditLog.SetId(snapshot.Id);

            return auditLog;
        }
    }
}
