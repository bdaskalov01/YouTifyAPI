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
    
    public object GetClientInfo(IdentityUser client);
    public object GetUserInfo(IdentityUser user);
    public Task<String> GenerateAccessTokenDuringLogin(IdentityUser user);
    public object? GenerateCcToken(OAuthClient client, TokenRequest request);
    public Task<object?> GenerateRoToken(IdentityUser owner, TokenRequest request);


}