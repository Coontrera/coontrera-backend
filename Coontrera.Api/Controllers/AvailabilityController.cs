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
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly IUserRepository _userRepository;

        public AvailabilityController(
            IAvailabilityService availabilityService,
            IUserRepository userRepository)
        {
            _availabilityService = availabilityService;
            _userRepository = userRepository;
        }

        [HttpGet("slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] string? date)
        {
            if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParse(date, out var parsedDate))
            {
                return BadRequest(new { message = "Data inválida. Use o formato yyyy-MM-dd." });
            }

            try
            {
                var slots = await _availabilityService.GetAvailableSlotsAsync(parsedDate);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar horários disponíveis.", details = ex.Message });
            }
        }

        [HttpGet("weekly")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWeeklyAvailability()
        {
            try
            {
                var weekly = await _availabilityService.GetWeeklyAvailabilityAsync();
                return Ok(weekly);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar disponibilidade semanal.", details = ex.Message });
            }
        }

        [HttpPut("weekly")]
        [Authorize]
        public async Task<IActionResult> UpdateWeeklyAvailability([FromBody] DayAvailabilityDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var role = await GetCurrentUserRoleAsync(userId);
            if (role != UserRole.Admin && role != UserRole.Funcionario)
                return Forbid();

            try
            {
                await _availabilityService.UpdateWeeklyAvailabilityAsync(dto, userId);
                return NoContent();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar disponibilidade semanal.", details = ex.Message });
            }
        }

        [HttpGet("blocked")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBlockedPeriods([FromQuery] string? from, [FromQuery] string? to)
        {
            DateOnly? fromDate = null;
            if (!string.IsNullOrWhiteSpace(from) && DateOnly.TryParse(from, out var f))
                fromDate = f;

            DateOnly? toDate = null;
            if (!string.IsNullOrWhiteSpace(to) && DateOnly.TryParse(to, out var t))
                toDate = t;

            try
            {
                var list = await _availabilityService.GetBlockedPeriodsAsync(fromDate, toDate);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar períodos bloqueados.", details = ex.Message });
            }
        }

        [HttpPost("blocked/day")]
        [Authorize]
        public async Task<IActionResult> BlockDay([FromBody] BlockDayDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var role = await GetCurrentUserRoleAsync(userId);
            if (role != UserRole.Admin && role != UserRole.Funcionario)
                return Forbid();

            try
            {
                var response = await _availabilityService.BlockDayAsync(dto, userId);
                return CreatedAtAction(nameof(GetBlockedPeriods), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao bloquear dia.", details = ex.Message });
            }
        }

        [HttpPost("blocked/slot")]
        [Authorize]
        public async Task<IActionResult> BlockSlot([FromBody] BlockSlotDTO dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var role = await GetCurrentUserRoleAsync(userId);
            if (role != UserRole.Admin && role != UserRole.Funcionario)
                return Forbid();

            try
            {
                var response = await _availabilityService.BlockSlotAsync(dto, userId);
                return CreatedAtAction(nameof(GetBlockedPeriods), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao bloquear horário.", details = ex.Message });
            }
        }

        [HttpDelete("blocked/{id}")]
        [Authorize]
        public async Task<IActionResult> Unblock(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var role = await GetCurrentUserRoleAsync(userId);
            if (role != UserRole.Admin && role != UserRole.Funcionario)
                return Forbid();

            try
            {
                await _availabilityService.UnblockAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao remover bloqueio.", details = ex.Message });
            }
        }

        [HttpGet("is-blocked")]
        [AllowAnonymous]
        public async Task<IActionResult> IsDateFullyBlocked([FromQuery] string? date)
        {
            if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParse(date, out var parsedDate))
            {
                return BadRequest(new { message = "Data inválida. Use o formato yyyy-MM-dd." });
            }

            try
            {
                var isBlocked = await _availabilityService.IsDateFullyBlockedAsync(parsedDate);
                return Ok(new { date = parsedDate.ToString("yyyy-MM-dd"), isFullyBlocked = isBlocked });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao verificar bloqueio de data.", details = ex.Message });
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
