using Microsoft.AspNetCore.Identity;
using WebAPIProgram.Models;
using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Repositories;

public interface IAuthRepository
{
    public Task<Response> FindResourceOwner(string username, string password, Boolean checkPassword = true);
    public OAuthClient? FindClient(string clientId, string clientSecret, Boolean checkPassword = true);
    public IdentityUserExtended? FindResourceOwnerById(string id);
    public Task<OAuthClient?> FindClientById(string id);
    public Task<Response> CreateResourceOwner(Register register);
    public Task<Response> CreateClient(ClientCreationRequest request);
    
    public Task SaveRefreshToken(String id, String token, String grantType, String scopes, String? userId = null);
    public Task RemoveRefreshToken(string token);
    public Task<Boolean> doesRefreshTokenExist(string token);
}