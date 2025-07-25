using Microsoft.AspNetCore.Identity;
using WebAPIProgram.Models;

namespace WebAPIProgram.Repositories;

public interface IAuthRepository
{
    public Task<Response> FindUser(Login login);
    public Task<Response> FindResourceOwner(TokenRequest request);
    public OAuthClient? FindClient(TokenRequest request);
}