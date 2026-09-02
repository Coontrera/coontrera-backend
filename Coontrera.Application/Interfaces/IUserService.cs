using Coontrera.Application.DTOs;

namespace Coontrera.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDTO> CreateUserAsync(UserCreateDTO request);
        Task<UserResponseDTO?> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(string userId, UserUpdateDTO request);
        Task DeleteUserAsync(string userId);
        Task ActivateUserAsync(string userId);
        Task DeactivateUserAsync(string userId);
        Task<UserResponseDTO> GetOrCreateProfileAsync(string firebaseUid, string? email);
        Task<AdminDashboardDto> GetAdminDataAsync(string firebaseUid);
        Task<UserResponseDTO> RegisterNewUserAsync(UserRegisterDto request);
        Task<string> LoginUserAsync(string email, string password);
    }
}