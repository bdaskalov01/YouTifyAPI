using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPIProgram.Models;

namespace WebAPIProgram.Controllers;

[ApiController]
public class IAAController: ControllerBase
{

    private readonly List<OAuthClient> _clients;
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager;
    
    public IAAController(List<OAuthClient> clients,IConfiguration config, UserManager<IdentityUser> userManager)
    {
        _clients = clients;
        _config = config;
        _userManager = userManager;
    }



    [HttpPost("Token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest request)
    {
        
        if (request.GrantType == "client_credentials")
        {
            return HandleCCFlow(request);
        }
        else if (request.GrantType == "password")
        {
            return await HandleROFlow(request);
        }

        return BadRequest("Unsupported grant_type");
    } 
    
    
    private IActionResult HandleCCFlow(
       TokenRequest request)
    {
        var client = _clients.SingleOrDefault(c =>
            c.ClientId == request.ClientId && c.ClientSecret == request.ClientSecret);

        if (client == null)
            return Unauthorized("Invalid client credentials");

        var requestedScopes = request.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        var grantedScopes = requestedScopes.Intersect(client.AllowedScopes).ToList();
        if (!grantedScopes.Any())
            return BadRequest("Invalid scope/s.");

        var claims = new List<Claim>();
        claims.AddRange(client.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(grantedScopes.Select(s => new Claim("scope", s)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            access_token = tokenStr,
            token_type = "Bearer",
            expires_in = 3600,
            scope = string.Join(" ", grantedScopes)
        });
    }
    
    private async Task<IActionResult> HandleROFlow(TokenRequest model)
    {
        if (model.GrantType != "password")
            return BadRequest(new { error = "unsupported_grant_type" });

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized(new { error = "invalid_grant" });

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? "")
        };

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (!string.IsNullOrEmpty(model.Scope))
            claims.Add(new Claim("scope", model.Scope));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            access_token = jwt,
            token_type = "Bearer",
            expires_in = 3600
        });
    }


}