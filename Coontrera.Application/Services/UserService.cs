// Application
using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;

//Domain
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models;
using Coontrera.Domain.Models.Enum;

//Others
using FirebaseAdmin.Auth;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace Coontrera.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseApiKey;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, HttpClient httpClient, IPasswordHasher passwordHasher, IConfiguration configuration, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _firebaseApiKey = configuration["Firebase:ApiKey"];
            _httpClient = httpClient;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
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
                Phone = newUser.Phone
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

        public async Task<UserResponseDTO> GetOrCreateProfileAsync(string firebaseUid, string? email)
        {
            var user = await _userRepository.GetUserByIdAsync(firebaseUid);
            if (user == null)
            {
                user = new User(
                    name: "New User",
                    email: email?? "",
                    password:"",
                    phone:"0000000000",
                    role: UserRole.User
                );
                user.SetId(firebaseUid);

                await _userRepository.AddUserAsync(user);
            }

            return new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };
            
        }
    
        public async Task<AdminDashboardDto> GetAdminDataAsync(string firebaseUid)
        {
            var user = await _userRepository.GetUserByIdAsync(firebaseUid);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (user.Role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied. Admins only.");

            var adminData = new AdminDashboardDto
            {
                Message = "Acesso autorizado, bem-vindo ao painel de administração!",
                SystemStatus = "Online",
                Role = (int)user.Role
            };

            return adminData;
        }
    
        public async Task<UserResponseDTO> RegisterNewUserAsync(UserRegisterDto request)
        {
            var userArgs = new UserRecordArgs
            {
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.Name
            };

            UserRecord userRecord;
            try
            {
                userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
            }
            catch (FirebaseAuthException ex)
            {
                throw new Exception($"Firebase error: {ex.Message}");
            }

            var newUser = new User(
                name: request.Name,
                email: request.Email,
                password: "", 
                phone: request.Phone,
                role: UserRole.User
            );

            newUser.SetId(userRecord.Uid);

            await _userRepository.AddUserAsync(newUser);

            return new UserResponseDTO
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Phone = newUser.Phone
            };
        }

        public async Task<string> LoginUserAsync(string email, string password)
        {
            var authUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}";
            
            var requestBody = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(authUrl, content);

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

            // Lemos a resposta do Firebase que contém o Token
            var responseData = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(responseData);
            
            // Extrai o Token oficial do Firebase
            string firebaseToken = jsonDocument.RootElement.GetProperty("localId").GetString();

            // Opcional: Validar se o usuário existe no seu Firestore antes de retornar o token
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException("Usuário não cadastrado.");

            return _tokenService.GenerateToken(email, firebaseToken);
        }
    }
}