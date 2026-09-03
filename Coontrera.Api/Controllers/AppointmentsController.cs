using System.Security.Claims;
using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;
using Coontrera.Domain.Interfaces;
using Coontrera.Domain.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Coontrera.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserRepository _userRepository;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IUserRepository userRepository)
        {
            _appointmentService = appointmentService;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var response = await _appointmentService.CreateAppointmentAsync(userId, dto);
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao criar agendamento.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? from = null,
            [FromQuery] string? to = null,
            [FromQuery] string? date = null,
            [FromQuery] string? clientId = null,
            [FromQuery] string? serviceId = null,
            [FromQuery] AppointmentStatus? status = null)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            DateOnly? fromDate = null;
            if (!string.IsNullOrWhiteSpace(from) && DateOnly.TryParse(from, out var f))
                fromDate = f;

            DateOnly? toDate = null;
            if (!string.IsNullOrWhiteSpace(to) && DateOnly.TryParse(to, out var t))
                toDate = t;

            DateOnly? filterDate = null;
            if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var d))
                filterDate = d;

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                var response = await _appointmentService.GetAllAppointmentsAsync(
                    userId,
                    userRole,
                    fromDate,
                    toDate,
                    filterDate,
                    clientId,
                    serviceId,
                    status
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar agendamentos.", details = ex.Message });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var response = await _appointmentService.GetMyAppointmentsAsync(userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao listar seus agendamentos.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                var response = await _appointmentService.GetAppointmentByIdAsync(id, userId, userRole);
                if (response == null)
                    return NotFound(new { message = "Agendamento não encontrado." });

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar agendamento.", details = ex.Message });
            }
        }

        [HttpPatch("{id}/observations")]
        public async Task<IActionResult> UpdateObservations(string id, [FromBody] AppointmentUpdateObservationsDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                var response = await _appointmentService.UpdateObservationsAsync(id, dto.Observations, userId, userRole);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar observações.", details = ex.Message });
            }
        }

        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> Confirm(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                var response = await _appointmentService.ConfirmAppointmentAsync(id, userId, userRole);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao confirmar consulta.", details = ex.Message });
            }
        }

        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> Complete(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                var response = await _appointmentService.CompleteAppointmentAsync(id, userId, userRole);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao concluir consulta.", details = ex.Message });
            }
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(string id, [FromBody] AppointmentCancelDTO? dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                var reason = string.IsNullOrWhiteSpace(dto?.Reason) ? "Cancelamento solicitado." : dto.Reason;
                var response = await _appointmentService.CancelAppointmentAsync(id, reason, userId, userRole);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao cancelar consulta.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            try
            {
                var userRole = await GetCurrentUserRoleAsync(userId);
                await _appointmentService.DeleteAppointmentAsync(id, userId, userRole);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao excluir agendamento.", details = ex.Message });
            }
        }

        private string? GetCurrentUserId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        private async Task<UserRole> GetCurrentUserRoleAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return user?.Role ?? UserRole.User;
        }
    }
}
