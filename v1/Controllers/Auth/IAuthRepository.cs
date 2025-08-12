using PSQLModels.Tables;
using WebAPIProgram.Models;

namespace WebAPIProgram.v1.Controllers.Auth;

public interface IAuthRepository
{
    public Task SaveRefreshToken(String id, String token, String grantType, String scopes, String? userId = null);
    public Task RemoveRefreshToken(string token);
    public Task<Boolean> doesRefreshTokenExist(string token);
}