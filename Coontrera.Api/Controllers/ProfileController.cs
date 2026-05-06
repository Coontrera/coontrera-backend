using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Coontrera.Application.Interfaces;

namespace Coontrera.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var firebaseUid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;


        if (string.IsNullOrEmpty(firebaseUid))
            return Unauthorized();

        var profile = await _userService.GetOrCreateProfileAsync(firebaseUid, email);

        return Ok(profile);
    }

    [HttpGet("admin-data")]
    public async Task<IActionResult> GetAdminData()
    {
        var firebaseUid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(firebaseUid))
            return Unauthorized();

        try
        {
            var adminData = await _userService.GetAdminDataAsync(firebaseUid);
            return Ok(adminData);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        
    }
}