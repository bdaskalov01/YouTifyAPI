using System.Security.Claims;
using PSQLModels.Contracts.ServiceResponses;
using PSQLModels.Tables;
using WebAPIProgram.Models;

namespace WebAPIProgram.v1.Controllers.User;

public interface IIdentityRepository
{
    public Task<Response> FindResourceOwner(string username, string password, Boolean checkPassword = true);
    public OAuthClient? FindClient(string clientId, string clientSecret, Boolean checkPassword = true);
    public IdentityUserExtended? FindResourceOwnerById(string id);
    public Task<OAuthClient?> FindClientById(string id);
    public Task<Response> CreateResourceOwner(Register register);
    public Task<Response> CreateClient(ClientCreationRequest request);
    
    public UserInfoFull GetUserInfo(ClaimsPrincipal user);
    public Object GetClientInfo(ClaimsPrincipal client);
    public Task<Response> ChangePassword(IdentityUserExtended user, string currentPassword, string newPassword);
    public Task<Response> ChangeEmail(IdentityUserExtended user, string newEmail, string token);
    public Task<Response> VerifyEmail(IdentityUserExtended user, string token);
    public IEnumerable<String>? GetPreviouslyUsedPasswords(string id);
    public bool CheckIfPasswordWasPreviouslyUsed(IdentityUserExtended user, IEnumerable<string> passwords, string newPassword);
    public Task<String> GenerateEmailConfirmationToken(IdentityUserExtended user);
}