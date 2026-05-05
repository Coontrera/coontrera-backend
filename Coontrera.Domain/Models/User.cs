using Coontrera.Domain.Models.Enum;

namespace Coontrera.Domain.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public DateTime DateRegistered { get; private set; } = DateTime.UtcNow;
        public bool IsActive { get; private set; } = true;
        public UserRole Role { get; private set; } = UserRole.User;

        protected User(){}

        public User(string name, string email, string password, string phone, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            if(string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone cannot be empty.", nameof(phone));

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Password = password;
            Phone = phone;
            Role = role;
        }

        public void Update(string name, string email, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));
            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone cannot be empty.", nameof(phone));

            Name = name;
            Email = email;
            Phone = phone;
        }

        public void Desactivate()
        {
            if (Role == UserRole.Admin)
                throw new InvalidOperationException("Admin users cannot be deactivated.");
            IsActive = false;
        }

        public void Reactivate()
        {
            IsActive = true;
        }

        public void updatePhone(string newPhone)
        {
            if (string.IsNullOrWhiteSpace(newPhone))
                throw new ArgumentException("Phone cannot be empty.", nameof(newPhone));
            Phone = newPhone;
        }

        public void updatePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(newPassword));
            Password = newPassword;
        }
    
        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}