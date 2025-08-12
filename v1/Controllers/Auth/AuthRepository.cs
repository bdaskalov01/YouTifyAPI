using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PSQLModels.Tables;
using WebAPIProgram.Models;
using WebAPIProgram.Util;

namespace WebAPIProgram.v1.Controllers.Auth;

public class AuthRepository : IAuthRepository
{

    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly ApplicationDbContext _context;

    public AuthRepository(UserManager<IdentityUserExtended> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
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
                ExpiryTime = DateTime.UtcNow.AddDays(AppConstants.sevenDays)
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