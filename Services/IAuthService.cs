using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using WebAPIProgram.Models;

namespace WebAPIProgram.Services;

public interface IAuthService
{
    
    public Task<Response> Login(Login login, Boolean checkPassword = true);
    public Task<Response> Register(Register register);
    public Response HandleCcFlow(AccessTokenRequest request);
    public Task<Response> HandleRoFlow(AccessTokenRequest request);
    public Task<Response> CreateClient(ClientCreationRequest request);
    
}