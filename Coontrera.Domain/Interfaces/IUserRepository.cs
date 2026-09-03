using Coontrera.Domain.Models;

namespace Coontrera.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(string id); 
        Task<User?> GetUserByEmailAsync(string email); 
        Task<List<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string id);
    }
}