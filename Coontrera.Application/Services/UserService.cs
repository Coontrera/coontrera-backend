using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

namespace Coontrera.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponseDTO> CreateUserAsync(UserCreateDTO request)
        {
            var userExisting = await _userRepository.GetUserByEmailAsync(request.Email);
            if (userExisting != null)
                throw new InvalidOperationException("A user with this email already exists.");
            
            string hashedPassword = _passwordHasher.Hash(request.Password);
            var newUser = new User(
                request.Name,
                request.Email,
                hashedPassword,
                request.Phone,
                UserRole.User
            );

            await _userRepository.AddUserAsync(newUser);

            var response = new UserResponseDTO
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
            };
            
            return response;
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return null;

            var response = new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
            
            return response;
        }

        public async Task UpdateUserAsync(string userId, UserUpdateDTO request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");
            if (user.Email != request.Email)
            {
                var usingEmail = await _userRepository.GetUserByEmailAsync(request.Email);
                if (usingEmail != null)
                {
                    throw new Exception("Este novo e-mail já está sendo usado por outra pessoa.");
                }
            }   

            user.Update(request.Name, request.Email, request.Phone);
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var currentUser = await _userRepository.GetUserByIdAsync(userId);
            if (currentUser == null)
                throw new KeyNotFoundException("User not found.");

            if (currentUser.Role == UserRole.Admin)
                throw new InvalidOperationException("Admin users cannot be deleted.");

            await _userRepository.DeleteUserAsync(userId);
        }
    }
}