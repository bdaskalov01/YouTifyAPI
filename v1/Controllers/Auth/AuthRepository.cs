using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPIProgram.Models;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Util;

namespace WebAPIProgram.Repositories;

public class AuthRepository : IAuthRepository
{

    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly ApplicationDbContext _context;

    public AuthRepository(UserManager<IdentityUserExtended> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Response> FindResourceOwner(string username, string password, Boolean checkPassword = true)
    {

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return new Response
            {
                Error = AuthConstants.invalidUser
            };
        if (checkPassword && !await _userManager.CheckPasswordAsync(user, password))
        {
            return new Response
            {
                Error = AuthConstants.invalidPassword
            };
        }

        return new Response
        {
            Result = user
        };
    }

    public OAuthClient? FindClient(string clientId, string clientSecret, Boolean checkPassword = true)
    {

        OAuthClient? client;
        if (checkPassword)
        {
            client = _context.OAuthClients.SingleOrDefault(c =>
                c.ClientId == clientId && c.ClientSecret == clientSecret);
            return client;
        }

        client = _context.OAuthClients.SingleOrDefault(c =>
            c.ClientId == clientId);

        return client;
    }

    public IdentityUserExtended? FindResourceOwnerById(string id)
    {
        var user = _userManager.FindByIdAsync(id).Result;
        return user;
    }

    public async Task<OAuthClient?> FindClientById(string id)
    {
        var client = await _context.OAuthClients.SingleOrDefaultAsync(client => client.ClientId == id);
        return client;
    }

    public async Task<Response> CreateResourceOwner(Register register)
    {
        var user = new IdentityUserExtended
        {
            UserName = register.Username,
            Email = register.Email,
        };
        var result = await _userManager.CreateAsync(user, register.Password);
        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return new Response
                {
                    Error = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            return new Response
            {
                Result = AuthConstants.successfullRegistration
            };
        }

        return new Response
        {
            Error = string.Join(", ", result.Errors.Select(e => e.Description)),
        };
    }

    public async Task<Response> CreateClient(ClientCreationRequest request)
    {
        var existingClient = FindClient(request.ClientId, request.ClientSecret, false);
        if (existingClient == null)
        {
            await _context.OAuthClients.AddAsync(new OAuthClient
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Roles = request.Roles,
                AllowedScopes = request.AllowedScopes,

            });
            await _context.SaveChangesAsync();
            return new Response
            {
                Result = "Client successfully created."
            };
        }

        return new Response
        {
            Error = "Failed to create client."
        };
    }

    public async Task SaveRefreshToken(String id, String token, String grantType, String scopes, String? user = null)
    {
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = token,
                ClientId = id,
                GrantType = grantType,
                UserId = user,
                Scope = scopes,
                ExpiryTime = DateTime.UtcNow.AddDays(AuthConstants.sevenDays)
            });
            await _context.SaveChangesAsync();
    }

    public async Task RemoveRefreshToken(string token)
    {
        await _context.RefreshTokens
            .Where(t => t.Token == token)
            .ExecuteDeleteAsync();
    }

    public async Task<Boolean> doesRefreshTokenExist(String token)
    {
        return await _context.RefreshTokens.AnyAsync(t => t.Token == token);
    }
}