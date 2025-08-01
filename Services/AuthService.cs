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

    public async Task<Response> Register(Register register)
    {
        var result = await _repository.CreateResourceOwner(register);
        return result;
    }

    public async Task<Response> FindUser(Login login, Boolean checkPassword = true)
    {
        var result = await _repository.FindResourceOwner(login.Username, login.Password, checkPassword);
        return result;
    }

    public Response FindClient(String clientId, String clientSecret)
    {
        var client = _repository.FindClient(clientId, clientSecret);
        if (client == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidClient
            };
        }

        return new Response
        {
            Result = client
        };
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

    public Response HandleCcFlow(AccessTokenRequest request)
    {
        var client = _repository.FindClient(request.ClientId, request.ClientSecret);

        if (client == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidClient
            };
        }

        var token = GenerateCcToken(client, request.Scope);

        if (token == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidScope
            };
        }

        return new Response
        {
            Result = token
        };
    }

    public async Task<Response> HandleRoFlow(AccessTokenRequest request)
    {
        var user = await _repository.FindResourceOwner(request.Username, request.Password);

        if (user.Result == null)
        {
            return user;
        }
        
        var client = _repository.FindClient(request.ClientId, request.ClientSecret);
        if (client == null)
        {
            return new Response
            {
                Error = AuthConstants.invalidClient,
                Result = null
            };
        }

        var token = await GenerateRoToken((IdentityUser)user.Result, client, request.Scope);

        return new Response
        {
            Result = token
        };
    }

    public async Task<object?> GenerateCcToken(OAuthClient client, String scopes)
    {
        var requestedScopes = scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var grantedScopes = requestedScopes.Intersect(client.AllowedScopes).ToList();
        if (!grantedScopes.Any())
            return null;

        var claims = new List<Claim>();
        claims.AddRange(client.Roles.Select(role => new Claim(AuthConstants.role, role)));
        claims.AddRange(grantedScopes.Select(s => new Claim(AuthConstants.scope, s)));
        claims.Add(new Claim(AuthConstants.idClaim, client.Id));
        claims.Add(new Claim(AuthConstants.grantType, AuthConstants.clientCredentialsGrant));

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

        return new TokenResponse
        {
            TokenType = AuthConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AuthConstants.oneHourInSeconds,
        };
    }

    public async Task<object?> GenerateRoToken(IdentityUser resourceowner, OAuthClient client, String scopes)
    {
        var userRoles = await _userManager.GetRolesAsync(resourceowner);

        var claims = new List<Claim>
        {
            new Claim(AuthConstants.idClaim, resourceowner.Id),
            new Claim(AuthConstants.emailClaim, resourceowner.Email ?? ""),
            new Claim(AuthConstants.usernameClaim, resourceowner.UserName ?? "")
        };

        claims.AddRange(userRoles.Select(role => new Claim(AuthConstants.role, role)));
        claims.Add(new Claim(AuthConstants.grantType, AuthConstants.resourceOwnerGrant));
        claims.Add(new Claim(AuthConstants.scope, scopes));
        claims.Add(new Claim(AuthConstants.clientIDClaim, client.ClientId));

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
        var refreshtoken = await GenerateRefreshToken(resourceowner.Id, AuthConstants.resourceOwnerGrant, scopes, client.ClientId);
        return new TokenResponse
        {
            TokenType = AuthConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AuthConstants.oneHourInSeconds,
            RefreshToken = refreshtoken
        };
    }

    public async Task<string?> GenerateRefreshToken(String user, String grantType, String scopes, String clientId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);
        try
        {
            await _repository.SaveRefreshToken(clientId, token, grantType, scopes, user);
        }
        catch (Exception e)
        {
            return null;
        }
        return token;
    }

    public async Task<Response> RefreshToken(RefreshTokenRequest request)
    {
        ParsedRefreshToken parsedToken;
        if (!await _repository.doesRefreshTokenExist(request.RefreshToken))
        {
            return new Response
            {
                Error = AuthConstants.invalidRefreshToken
            };
        }

        try
        {
            parsedToken = ValidateToken(request.AccessToken);
        }
        catch (Exception e)
        {
            return new Response
            {
                Error = e.Message
            };
        }

        var user = _repository.FindResourceOwnerById(parsedToken.UserId);
        OAuthClient? client;
        try
        {
            client = await _repository.FindClientById(parsedToken.ClientId);
        }
        catch (Exception e)
        {
            return new Response
            {
                Error = AuthConstants.invalidClient
            };
        }


        if (parsedToken.TokenType == AuthConstants.resourceOwnerGrant && user != null)
        {
            var token = await GenerateRoToken(user, client, parsedToken.Scopes);
            await _repository.RemoveRefreshToken(request.RefreshToken);
            return new Response
            {
                Result = token
            };
        }
        

        throw new NullReferenceException();
    }

    private ParsedRefreshToken ValidateToken(string token)
    {
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration[AuthConstants.jwtIssuer],
            ValidAudience = _configuration[AuthConstants.jwtAudience],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AuthConstants.jwtKey]!))
        };
        try
        {
            var result = new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
            var tokenType = result.Claims.FirstOrDefault(s => s.Type == AuthConstants.grantType)!.Value;
            var clientid = result.Claims.FirstOrDefault(s => s.Type == AuthConstants.clientIDClaim)!.Value;
            var id = result.Claims.FirstOrDefault(s => s.Type == AuthConstants.idClaim)!.Value;
            var scopes = result.Claims.FirstOrDefault(s => s.Type == AuthConstants.scope)!.Value;

            return new ParsedRefreshToken
            {
                ClientId = clientid,
                TokenType = tokenType,
                UserId = id,
                Scopes = scopes
            };
        }
        catch (Exception e)
        {
            throw new Exception("Invalid token");
        }
    }

    public Task<Response> UpdateUserRoles(UpdateUserRolesRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response> UpdateUserInfo()
    {
        throw new NotImplementedException();
    }
}