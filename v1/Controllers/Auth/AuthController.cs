using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPIProgram.Filters.Action;
using WebAPIProgram.Models;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Services;
using WebAPIProgram.Util;

namespace WebAPIProgram.Controllers;

[ApiController]
[Route("v1/[Controller]")]
public class AuthController: ControllerBase
{

    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly IAuthService _authService;
    
    public AuthController(IConfiguration configuration, UserManager<IdentityUserExtended> userManager, IAuthService iAAService)
    {
        _configuration = configuration;
        _userManager = userManager;
        _authService = iAAService;
    }


    [HttpPost("Login")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Login([FromForm] AccessTokenRequest login)
    {
        var response = await _authService.HandleRoFlow(login);
        if (response.Result == null)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromForm] Register register)
    {
        var response = await _authService.Register(register);
        if (response.Result == null)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }


    [HttpPost("Token")]
    [ServiceFilter(typeof(ArtistClickedFilter))]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] AccessTokenRequest request)
    {
        
        Console.WriteLine(request.GrantType);
        if (request.GrantType == AuthConstants.clientCredentialsGrant)
        {
            var response = _authService.HandleCcFlow(request);
            if (response.Result == null)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
        if (request.GrantType == AuthConstants.resourceOwnerGrant)
        {
            var result = await _authService.HandleRoFlow(request);
            if (result.Result == null)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        return BadRequest(AuthConstants.invalidGrantType);
    }

    [HttpPost("Refresh")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Refresh([FromForm] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshToken(request);
        if (response.Error != null)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
    

    [HttpGet("UserInfo")]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        var id = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == AuthConstants.role)
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            Id = id,
            Username = username,
            Email = email,
            Roles = roles,
        });
    }
    
    [HttpGet("ClientInfo")]
    [Authorize]
    public IActionResult GetClientInfo()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == AuthConstants.role)
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            Roles = roles,
        });
    }

    [HttpPost("CreateClient")]
    public async Task<IActionResult> CreateClient([FromForm] ClientCreationRequest request)
    {
        var result = await _authService.CreateClient(request);
        if (result.Error != null)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
    
}