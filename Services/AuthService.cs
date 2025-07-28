using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        var token = await GenerateAccessTokenDuringLogin((IdentityUser)response.Result);

        return new Response
        {
            Error = null,
            Result = token
        };
    }

    public async Task<Response> Register(Register register)
    {
        var result = await _repository.CreateUser(register);
        if (result.Result != null)
        {
            var token = await GenerateAccessTokenDuringLogin((IdentityUser)result.Result);
            return new Response
            {
                Error = null,
                Result = token
            };
        }

        return result;
    }

    public async Task<Response> FindUser(Login login)
    {
        var result = await _repository.FindUser(login);
        return result;
    }

    public async Task<Response> CreateClient(ClientCreationRequest request)
    {
        var result = await _repository.CreateClient(request);
        return result;
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
            return response;
        }

        var token = await GenerateRoToken((IdentityUser)response.Result, request);

        return new Response
        {
            Error = null,
            Result = token
        };
    }
    public async Task<TokenResponse> GenerateAccessTokenDuringLogin(IdentityUser user)
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
        var refreshtoken = await GenerateRefreshToken(user.Id);

        return new TokenResponse
        {
            TokenType = AuthConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AuthConstants.oneHourInSeconds,
            RefreshToken = refreshtoken
        };
    }

    public async Task<object?> GenerateCcToken(OAuthClient client, TokenRequest request)
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

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshtoken = await GenerateRefreshToken(client.Id);

        return new TokenResponse
        {
            TokenType = AuthConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AuthConstants.oneHourInSeconds,
            RefreshToken = refreshtoken
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
        var refreshtoken = await GenerateRefreshToken(resourceowner.Id);

        return new TokenResponse
        {
            TokenType = AuthConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AuthConstants.oneHourInSeconds,
            RefreshToken = refreshtoken
        };
    }

    public async Task<string?> GenerateRefreshToken(String id)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);
        try
        {
            await _repository.SaveRefreshToken(id, token);
        }
        catch (Exception e)
        {
            return null;
        }
        return token;
    }
}