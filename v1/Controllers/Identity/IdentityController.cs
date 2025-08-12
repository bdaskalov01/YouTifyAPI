using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PSQLModels.Tables;
using WebAPIProgram.Models;
using WebAPIProgram.Util;
using WebAPIProgram.v1.Controllers.User.Requests;

namespace WebAPIProgram.v1.Controllers.User;


[ApiController]
[Route("v1/[controller]")]
public class IdentityController : ControllerBase
{
    
    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly IIdentityService _identityService;
    public IdentityController(UserManager<IdentityUserExtended> userManager, IIdentityService identityService)
    {
        _userManager = userManager;
        _identityService = identityService;
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        return Ok(_identityService.GetUserInfo(User));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var response = await _identityService.ChangePassword(User, request.CurrentPassword, request.NewPassword);
        if (response.Error != null)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("change-email")] 
    public async Task<IActionResult> ChangeEmail([FromQuery]ChangeEmailRequest request)
    {
        var response = await _identityService.ChangeEmail(request.UserId, request.Email, request.Token);
        if (response.Error != null)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] VerifyEmailRequest request)
    {
        var response = await _identityService.VerifyEmail(request.UserId, request.Email, request.Token);
        if (response.Error != null)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    // TODO: Apply RO Only policy!!!!!! Clients must NOT be able to generate email verificatiotn tokens because they don't have emails!
    [HttpGet("send-email-verification")] 
    [Authorize]
    public async Task<IActionResult> SendEmailVerificationToken()
    {
        var result = await _identityService.CreateVerificationCode(User);
        return Ok(result);
    }
    
    
}