using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Util;

namespace WebAPIProgram.Controllers;

[ApiController]
[Route("v1/api/[controller]")]
public class UserController : ControllerBase
{
    
    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly IUserService _userService;
    public UserController(UserManager<IdentityUserExtended> userManager, IUserService userService)
    {
        _userManager = userManager;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = AuthConstants.apiPolicy)]
    public async Task<IActionResult> GetUser([FromForm] string userId)
    {
        await _userService.getUserIdTest(User.Identity.Name, userId);
        return Ok();
    }

    
}