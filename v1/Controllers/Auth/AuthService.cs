using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PSQLModels.Tables;
using WebAPIProgram.Models;
using WebAPIProgram.Util;
using WebAPIProgram.v1.Controllers.User;

namespace WebAPIProgram.v1.Controllers.Auth;

public class 
    AuthService: IAuthService
{
    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly IAuthRepository _authRepository;
    private readonly IIdentityRepository _identityRepository;
    
    public AuthService(UserManager<IdentityUserExtended> userManager, IConfiguration configuration, ApplicationDbContext context, IAuthRepository authRepository, IIdentityRepository identityRepository)
    {
     _userManager = userManager;   
     _configuration = configuration;
     _context = context;
     _authRepository = authRepository;
     _identityRepository = identityRepository;
    }

    public async Task<Response> Register(Register register)
    {
        var result = await _identityRepository.CreateResourceOwner(register);
        return result;
    }

    public async Task<Response> FindUser(Login login, Boolean checkPassword = true)
    {
        var result = await _identityRepository.FindResourceOwner(login.Username, login.Password, checkPassword);
        return result;
    }

    public Response FindClient(String clientId, String clientSecret)
    {
        var client = _identityRepository.FindClient(clientId, clientSecret);
        if (client == null)
        {
            return new Response
            {
                Error = AppConstants.invalidClient
            };
        }

        return new Response
        {
            Result = client
        };
    }

    public async Task<Response> CreateClient(ClientCreationRequest request)
    {
        var result = await _identityRepository.CreateClient(request);
        return result;
    }

    public Response HandleCcFlow(AccessTokenRequest request)
    {
        var client = _identityRepository.FindClient(request.ClientId, request.ClientSecret);

        if (client == null)
        {
            return new Response
            {
                Error = AppConstants.invalidClient
            };
        }

        var token = GenerateCcToken(client, request.Scope);

        if (token == null)
        {
            return new Response
            {
                Error = AppConstants.invalidScope
            };
        }

        return new Response
        {
            Result = token
        };
    }

    public async Task<Response> HandleRoFlow(AccessTokenRequest request)
    {
        var user = await _identityRepository.FindResourceOwner(request.Username, request.Password);

        if (user.Result == null)
        {
            return user;
        }
        
        var client = _identityRepository.FindClient(request.ClientId, request.ClientSecret);
        if (client == null)
        {
            return new Response
            {
                Error = AppConstants.invalidClient,
                Result = null
            };
        }

        var token = await GenerateRoToken((IdentityUserExtended)user.Result, client, request.Scope);

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
        claims.AddRange(client.Roles.Select(role => new Claim(AppConstants.role, role)));
        claims.AddRange(grantedScopes.Select(s => new Claim(AppConstants.scope, s)));
        claims.Add(new Claim(AppConstants.idClaim, client.Id));
        claims.Add(new Claim(AppConstants.grantType, AppConstants.clientCredentialsGrant));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AppConstants.jwtKey]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration[AppConstants.jwtIssuer],
            audience: _configuration[AppConstants.jwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AppConstants.oneHourInSeconds),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResponse
        {
            TokenType = AppConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AppConstants.oneHourInSeconds,
        };
    }

    public async Task<object?> GenerateRoToken(IdentityUserExtended resourceowner, OAuthClient client, String scopes)
    {
        var userRoles = await _userManager.GetRolesAsync(resourceowner);

        var claims = new List<Claim>
        {
            new Claim(AppConstants.idClaim, resourceowner.Id),
            new Claim(AppConstants.emailClaim, resourceowner.Email ?? ""),
            new Claim(AppConstants.usernameClaim, resourceowner.UserName ?? "")
        };

        claims.AddRange(userRoles.Select(role => new Claim(AppConstants.role, role)));
        claims.Add(new Claim(AppConstants.scope, scopes));
        claims.Add(new Claim(AppConstants.clientIDClaim, client.ClientId));
        claims.Add(new Claim(AppConstants.grantType, AppConstants.resourceOwnerGrant));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AppConstants.jwtKey]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration[AppConstants.jwtIssuer],
            audience: _configuration[AppConstants.jwtAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(AppConstants.oneHourInSeconds),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshtoken = await GenerateRefreshToken(resourceowner.Id, AppConstants.resourceOwnerGrant, scopes, client.ClientId);
        return new TokenResponse
        {
            TokenType = AppConstants.bearerTokenType,
            AccessToken = jwt,
            ExpiresIn = AppConstants.oneHourInSeconds,
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
            await _authRepository.SaveRefreshToken(clientId, token, grantType, scopes, user);
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
        if (!await _authRepository.doesRefreshTokenExist(request.RefreshToken))
        {
            return new Response
            {
                Error = AppConstants.invalidRefreshToken
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

        var user = _identityRepository.FindResourceOwnerById(parsedToken.UserId);
        OAuthClient? client;
        try
        {
            client = await _identityRepository.FindClientById(parsedToken.ClientId);
        }
        catch (Exception e)
        {
            return new Response
            {
                Error = AppConstants.invalidClient
            };
        }


        if (parsedToken.TokenType == AppConstants.resourceOwnerGrant && user != null)
        {
            var token = await GenerateRoToken(user, client, parsedToken.Scopes);
            await _authRepository.RemoveRefreshToken(request.RefreshToken);
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
            ValidIssuer = _configuration[AppConstants.jwtIssuer],
            ValidAudience = _configuration[AppConstants.jwtAudience],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration[AppConstants.jwtKey]!))
        };
        try
        {
            var result = new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out _);
            var tokenType = result.Claims.FirstOrDefault(s => s.Type == AppConstants.grantType)!.Value;
            var clientid = result.Claims.FirstOrDefault(s => s.Type == AppConstants.clientIDClaim)!.Value;
            var id = result.Claims.FirstOrDefault(s => s.Type == AppConstants.idClaim)!.Value;
            var scopes = result.Claims.FirstOrDefault(s => s.Type == AppConstants.scope)!.Value;

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
    
}