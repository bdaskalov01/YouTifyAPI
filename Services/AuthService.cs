using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using WebAPIProgram.Models;
using WebAPIProgram.Repositories;
using WebAPIProgram.Util;

namespace WebAPIProgram.Services;

public class AuthService: IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IAuthRepository _repository;
    
    public AuthService(UserManager<IdentityUser> userManager, IConfiguration configuration, ApplicationDbContext context, IAuthRepository repository)
    {
     _userManager = userManager;   
     _configuration = configuration;
     _context = context;
     _repository = repository;
    }

    public async Task<Response> Login(Login login)
    {
        var response = await FindUser(login);

        if (response.Result == null)
        {
            return new Response
            {
                Error = response.Error,
                Result = null
            };
        }

        var token = GenerateAccessTokenDuringLogin((IdentityUser)response.Result);

        return new Response
        {
            Error = null,
            Result = token
        };
    }

    public async Task<Response> Register(Register register)
    {
        return new Response
        {
            
        };
    }

    public async Task<Response> FindUser(Login login)
    {
        var user = _userManager.FindByNameAsync(login.username).Result;
        if (user == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidUser,
                Result = null
            };
        }

        if (!await _userManager.CheckPasswordAsync(user, login.password))
        {
            return new Response
            {
                Error = AuthConstants.invalidPassword,
                Result = null
            };
        }

        return new Response
        {
            Error = null,
            Result = user
        };
    }

    public async Task<Response> FindResourceOwner(TokenRequest request)
    {
        if (request.GrantType != AuthConstants.resourceOwnerGrant)
            return new Response
            {
             Error = AuthConstants.invalidPassword,
             Result = null
            };

        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return new Response
            {
                Error = AuthConstants.invalidGrantType,
                Result = null };
        
        return new Response
        {
            Error = null,
            Result = user
        };
    }

    public OAuthClient? FindClient(TokenRequest request)
    {
        var client = _context.OAuthClients.SingleOrDefault(c =>
            c.ClientId == request.ClientId && c.ClientSecret == request.ClientSecret);
        
        return client;
    }

    public object GetClientInfo(IdentityUser client)
    {
        throw new NotImplementedException();
    }

    public object GetUserInfo(IdentityUser user)
    {
        throw new NotImplementedException();
    }

    public Response HandleCcFlow(TokenRequest request)
    {
        var client = _repository.FindClient(request);

        if (client == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidClient,
                Result = null
            };
        }

        var token = GenerateCcToken(client, request);

        if (token == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidScope,
                Result = null
            };
        }

        return new Response
        {
            Error = null,
            Result = token
        };
    }
    
    public async Task<Response> HandleRoFlow(TokenRequest request)
    {
        var response = await _repository.FindResourceOwner(request);

        if (response.Result == null)
        {
            return new Response
            {
                Error = response.Error,
                Result = null
            };
        }

        var token = await GenerateRoToken((IdentityUser)response.Result, request);

        return new Response
        {
            Error = null,
            Result = token
        };
    }
    public async Task<string> GenerateAccessTokenDuringLogin(IdentityUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? "")
        };

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AuthConstants.jwtKey]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration[AuthConstants.jwtIssuer],
            audience: _configuration[AuthConstants.jwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AuthConstants.oneHourInSeconds),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public object? GenerateCcToken(OAuthClient client, TokenRequest request)
    {
        var requestedScopes = request.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        var grantedScopes = requestedScopes.Intersect(client.AllowedScopes).ToList();
        if (!grantedScopes.Any())
            return null;

        var claims = new List<Claim>();
        claims.AddRange(client.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(grantedScopes.Select(s => new Claim(AuthConstants.scope, s)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AuthConstants.jwtKey]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration[AuthConstants.jwtIssuer],
            audience: _configuration[AuthConstants.jwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AuthConstants.oneHourInSeconds),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return new
        {
            access_token = tokenStr,
            token_type = AuthConstants.bearerToken,
            expires_in = AuthConstants.oneHourInSeconds,
            scope = string.Join(" ", grantedScopes)
        };
    }

    public async Task<object?> GenerateRoToken(IdentityUser resourceowner, TokenRequest request)
    {
        var userRoles = await _userManager.GetRolesAsync(resourceowner);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, resourceowner.Id),
            new Claim(JwtRegisteredClaimNames.Email, resourceowner.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.UniqueName, resourceowner.UserName ?? "")
        };

        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (!string.IsNullOrEmpty(request.Scope))
            claims.Add(new Claim(AuthConstants.scope, request.Scope));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AuthConstants.jwtKey]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration[AuthConstants.jwtIssuer],
            audience: _configuration[AuthConstants.jwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AuthConstants.oneHourInSeconds),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return new
        {
            access_token = jwt,
            token_type = AuthConstants.bearerToken,
            expires_in = AuthConstants.oneHourInSeconds
        };
    }
}