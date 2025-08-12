using System.Security.Claims;
using PSQLModels.Contracts.ServiceResponses;
using PSQLModels.Tables;
using WebAPIProgram.Models;

namespace WebAPIProgram.v1.Controllers.User;

public interface IIdentityService
{
    public Response GetUserInfo(ClaimsPrincipal user);
    public Response GetClientInfo(ClaimsPrincipal claimsPrincipal);
    public Task<Response> ChangePassword(ClaimsPrincipal user, string currentPassword, string newPassword);
    public Task<Response> ChangeEmail(string userId, string email, string token);
    public Task<Response> CreateVerificationCode(ClaimsPrincipal user);
    public Task<Response> VerifyEmail(String userId, string email, string token);

    //TODO:
    //Get user info endpoints RO/CC (prefferably multiple functions)
    //Update user roles = WIP
    //Update user claims(email, etc.)
    //Delete user
    //Create roles
    //Delete roles
}