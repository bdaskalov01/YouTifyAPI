using WebAPIProgram.Models;

namespace WebAPIProgram.v1.Controllers.Auth;

public interface IAuthService
{
    public Task<Response> Register(Register register);
    public Response HandleCcFlow(AccessTokenRequest request);
    public Task<Response> HandleRoFlow(AccessTokenRequest request);
    public Task<Response> CreateClient(ClientCreationRequest request);
    public Task<Response> RefreshToken(RefreshTokenRequest request);


    //TODO:
    //Update user roles = WIP
    //Update user claims(email, etc.)
    //Delete user
    //Create roles
    //Delete roles

}