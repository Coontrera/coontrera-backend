using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Coontrera.Application.DTOs;
using Coontrera.Application.Interfaces;

namespace Coontrera.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicServicesController : ControllerBase
    {
        private readonly IClinicServiceService _clinicServiceService;

        public ClinicServicesController(IClinicServiceService clinicServiceService)
        {
            _clinicServiceService = clinicServiceService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] ClinicServiceCreateDTO dto)
        {
            try
            {
                var response = await _clinicServiceService.CreateClinicServiceAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao criar o serviço.", details = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _clinicServiceService.GetAllClinicServicesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao listar os serviços.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var response = await _clinicServiceService.GetClinicServiceByIdAsync(id);
                if (response == null) return NotFound(new { message = "Serviço não encontrado." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao buscar o serviço.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(string id, [FromBody] ClinicServiceCreateDTO dto)
        {
            try
            {
                await _clinicServiceService.UpdateClinicServiceAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao atualizar o serviço.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _clinicServiceService.DeleteClinicServiceAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao excluir o serviço.", details = ex.Message });
            }
        }

        [HttpPatch("{id}/activate")]
        [AllowAnonymous]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                await _clinicServiceService.ActivateClinicServiceAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao ativar o serviço.", details = ex.Message });
            }
        }

        [HttpPatch("{id}/deactivate")]
        [AllowAnonymous]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                await _clinicServiceService.DeactivateClinicServiceAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao inativar o serviço.", details = ex.Message });
            }
        }
    }
}
