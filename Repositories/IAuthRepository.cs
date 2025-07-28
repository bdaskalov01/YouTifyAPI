using Microsoft.AspNetCore.Identity;
using WebAPIProgram.Models;

namespace WebAPIProgram.Repositories;

public interface IAuthRepository
{
    public Task<Response> FindUser(Login login);
    public Task<Response> CreateUser(Register register);
    public Task<Response> CreateClient(ClientCreationRequest request);
    public Task<Response> FindResourceOwner(TokenRequest request);
    public OAuthClient? FindClient(TokenRequest request);
    public Task SaveRefreshToken(String id, String token);
}