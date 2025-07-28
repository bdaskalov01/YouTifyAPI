using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPIProgram.Models;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Util;

namespace WebAPIProgram.Repositories;

public class AuthRepository : IAuthRepository
{
    
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AuthRepository(UserManager<IdentityUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    
    public async Task<Response> FindUser(Login login)
    {
        var user = await _userManager.FindByNameAsync(login.username);
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

    public async Task<Response> CreateUser(Register register)
    {
        var user = new IdentityUser
        {
            UserName = register.username,
            Email = register.email,
        };
        var result = await _userManager.CreateAsync(user, register.password);
        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                
                return new Response
                {
                    Error = string.Join(", ", roleResult.Errors.Select(e => e.Description)),
                    Result = null
                };
            }
            return new Response
            {
                Error = null,
                Result = user
            };
        }

        return new Response
        {
            Error = string.Join(", ", result.Errors.Select(e => e.Description)),
            Result = null
        };
    }

    public async Task<Response> CreateClient(ClientCreationRequest request)
    {
        var existingClient = FindClient(new TokenRequest
        {
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
        });
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
                Error = null,
                Result = "Client successfully created."
            };
        }

        return new Response
        {
            Error = "Failed to create client.",
            Result = null
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
                Error = AuthConstants.invalidUser,
                Result = null
            };
        
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

    public async Task SaveRefreshToken(String id, String token)
    {
        var selectedUser = await _context.RefreshTokens.FirstOrDefaultAsync(u => u.UserId == id);
        if (selectedUser == null)
        {
            await _context.RefreshTokens.AddAsync(new RefreshToken
            {
                Token = token,
                UserId = id,
                ExpiryTime = DateTime.UtcNow.AddSeconds(AuthConstants.oneHourInSeconds)
            });
            await _context.SaveChangesAsync();
            return;
        }
        selectedUser.Token = token;
        selectedUser.ExpiryTime = DateTime.UtcNow.AddSeconds(AuthConstants.oneHourInSeconds);
        await _context.SaveChangesAsync();

    }
}