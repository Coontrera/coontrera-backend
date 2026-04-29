using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Google.Cloud.Firestore;

namespace Coontrera.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly FirestoreDb _db;

    public ProfileController(IConfiguration config)
    {
        _db = FirestoreDb.Create(config["Firebase:ProjectId"]);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var firebaseUid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(firebaseUid))
            return Unauthorized();

        DocumentReference userDoc = _db.Collection("Users").Document(firebaseUid);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            return Ok(snapshot.ToDictionary());
        }

        var newUser = new
        {
            Email = email,
            CreatedAt = Timestamp.GetCurrentTimestamp(),
            RoleLevel = 1
        };

        await userDoc.SetAsync(newUser);
        return Ok(newUser);
    }

    [HttpGet("admin-data")]
    public async Task<IActionResult> GetAdminData()
    {
        var firebaseUid = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(firebaseUid))
            return Unauthorized();

        DocumentSnapshot snapshot = await _db.Collection("Users").Document(firebaseUid).GetSnapshotAsync();

        if (!snapshot.Exists || !snapshot.TryGetValue("RoleLevel", out int roleLevel) || roleLevel < 3)
        {
            return Forbid();
        }

        var adminData = new
        {
            Message = "Acesso autorizado. Bem-vindo ao painel de administração.",
            SystemStatus = "Online",
            Role = roleLevel
        };

        return Ok(adminData);
    }
}