using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//Application
using Coontrera.Application.Interfaces;
using Coontrera.Application.DTOs;

namespace Coontrera.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous] 
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var profile = await _userService.RegisterNewUserAsync(dto);
                var token = _tokenService.GenerateToken(profile.Email, profile.Id);
                return CreatedAtAction(nameof(Register), new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginResponseDTO dto)
        {
            try
            {
                var token = await _userService.LoginUserAsync(dto.Email, dto.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });    
            }
        

        }   
    }   
}