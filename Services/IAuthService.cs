using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using WebAPIProgram.Models;

namespace WebAPIProgram.Services;

public interface IAuthService
{
    
    public Task<Response> Login(Login login);
    public Task<Response> Register(Register register);
    public Response HandleCcFlow(TokenRequest request);
    public Task<Response> HandleRoFlow(TokenRequest request);
    public Task<Response> CreateClient(ClientCreationRequest request);
    public object GetClientInfo(IdentityUser client);
    public object GetUserInfo(IdentityUser user);
    public Task<TokenResponse> GenerateAccessTokenDuringLogin(IdentityUser user);
    public Task<object?>GenerateCcToken(OAuthClient client, TokenRequest request);
    public Task<object?> GenerateRoToken(IdentityUser owner, TokenRequest request);

    public Task<string?> GenerateRefreshToken(String id);


}