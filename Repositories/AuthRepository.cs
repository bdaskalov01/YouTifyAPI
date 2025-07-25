using Microsoft.AspNetCore.Identity;
using WebAPIProgram.Models;
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
}