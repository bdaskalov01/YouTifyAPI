using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    
    private readonly UserManager<IdentityUser> _userManager;
    public UserController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    
}