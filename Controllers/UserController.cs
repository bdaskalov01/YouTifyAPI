using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Controllers;

[ApiController]
public class UserController : ControllerBase
{

    [HttpGet("api/user")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .ToList();
        
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        return Ok(new
        {
            Roles = roles,
            Email = email,
            Name = name
        });
    }
}