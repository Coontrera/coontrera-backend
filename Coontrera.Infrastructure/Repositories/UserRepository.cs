using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;
using Google.Cloud.Firestore;



namespace Coontrera.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "Users";

        public UserRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<User> AddUserAsync(User user)
        {            
           var userData = new Dictionary<string, object>
            {
                { "Name", user.Name },
                { "Email", user.Email },
                { "Password", user.Password },
                { "Phone", user.Phone },
                {"DateRegistered", user.DateRegistered },
                {"IsActive", user.IsActive },
                { "Role", (int)user.Role }
            };

            DocumentReference docRef = _db.Collection(CollectionName).Document(user.Id.ToString());
            await docRef.SetAsync(userData);

            return user;
        }

        public async Task DeleteUserAsync(string userId)
        {
           DocumentReference docRef = _db.Collection(CollectionName).Document(userId);
            await docRef.DeleteAsync();
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;

            return MapSnapshotToUser(snapshot);

        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            Query query = _db.Collection(CollectionName).WhereEqualTo("Email", email);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            if (querySnapshot.Documents.Count == 0) return null;

            return MapSnapshotToUser(querySnapshot.Documents[0]);
        }

        public async Task UpdateUserAsync(User user)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(user.Id.ToString());
            var updates = new Dictionary<string, object>
            {
                {"Name", user.Name },
                {"Email", user.Email },
                {"Phone", user.Phone},
                {"IsActive", user.IsActive },
                {"Role", (int)user.Role }
            };

            await docRef.UpdateAsync(updates);
        }
    
        private User MapSnapshotToUser(DocumentSnapshot snapshot)
        {
            var dict = snapshot.ToDictionary();
            var user = new User(
                dict.ContainsKey("Name") ? dict["Name"].ToString()! : "",
                dict.ContainsKey("Email") ? dict["Email"].ToString()! : "",
                dict.ContainsKey("PasswordHash") ? dict["PasswordHash"].ToString()! : "",
                dict.ContainsKey("Phone") ? dict["Phone"]?.ToString()! : "",
                dict.ContainsKey("Role") ? (UserRole)Convert.ToInt32(dict["Role"]) : UserRole.User
            );

            user.SetId(Guid.Parse(snapshot.Id));

            return user;
            
        }
    }   
}
