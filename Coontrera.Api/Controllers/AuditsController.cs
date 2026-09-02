using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Coontrera.Application.Interfaces;

namespace Coontrera.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditsController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditsController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var audits = await _auditService.GetAllAuditsAsync();
                return Ok(audits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar logs de auditoria.", details = ex.Message });
            }
        }

        [HttpGet("entity/{entityName}/{entityId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByEntity(string entityName, string entityId)
        {
            try
            {
                var audits = await _auditService.GetAuditsByEntityAsync(entityName, entityId);
                return Ok(audits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar auditoria da entidade.", details = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUser(string userId)
        {
            try
            {
                var audits = await _auditService.GetAuditsByUserAsync(userId);
                return Ok(audits);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar auditoria do usuário.", details = ex.Message });
            }
        }
    }
}
