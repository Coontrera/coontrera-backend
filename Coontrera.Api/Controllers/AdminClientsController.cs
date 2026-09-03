using System.Security.Claims;
using Coontrera.Application.DTOs;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coontrera.Api.Controllers
{
    [ApiController]
    [Route("api/admin/clients")]
    [Authorize]
    public class AdminClientsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public AdminClientsController(
            IUserRepository userRepository,
            IAppointmentRepository appointmentRepository)
        {
            _userRepository = userRepository;
            _appointmentRepository = appointmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var currentUser = await _userRepository.GetUserByIdAsync(currentUserId);
            if (currentUser == null || (currentUser.Role != UserRole.Admin && currentUser.Role != UserRole.Funcionario))
            {
                return Forbid();
            }

            try
            {
                var allUsers = await _userRepository.GetAllUsersAsync();
                var clients = allUsers.Where(u => u.Role == UserRole.User).ToList();

                var allAppointments = await _appointmentRepository.GetAllAppointmentsAsync();
                var countsByClientId = new Dictionary<string, int>();
                foreach (var a in allAppointments)
                {
                    countsByClientId[a.UserId] = countsByClientId.TryGetValue(a.UserId, out var count) ? count + 1 : 1;
                }

                var response = clients.Select(c => new ClientResponseDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    AppointmentsCount = countsByClientId.TryGetValue(c.Id, out var count) ? count : 0,
                    CreatedAt = c.DateRegistered
                })
                .OrderBy(c => c.Name)
                .ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar lista de clientes.", details = ex.Message });
            }
        }
    }
}
