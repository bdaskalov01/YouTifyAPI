using System.Security.Claims;
using MassTransit;
using PSQLModels.Contracts.ServiceResponses;
using StackExchange.Redis;
using WebAPIProgram.Util;
using Response = WebAPIProgram.Models.Response;

namespace WebAPIProgram.v1.Controllers.User;

public class IdentityService : IIdentityService
{
    private readonly IIdentityRepository _identityRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<IdentityService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IConnectionMultiplexer _reddis;
    private readonly IPublishEndpoint _publishEndpoint;
    
    public IdentityService(IIdentityRepository identityRepository, ApplicationDbContext dbContext, ILogger<IdentityService> logger, IConfiguration configuration, IConnectionMultiplexer reddis, IPublishEndpoint publishEndpoint)
    {
        _identityRepository = identityRepository;
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        _reddis = reddis;
        _publishEndpoint = publishEndpoint;
    }
    
    public Response GetUserInfo(ClaimsPrincipal user)
    {
        return new Response
        {
            Result = _identityRepository.GetUserInfo(user)
        };
    }

    public Response GetClientInfo(ClaimsPrincipal client)
    {
        throw new NotImplementedException();
    }

    public async Task<Response> ChangePassword(ClaimsPrincipal user, string currentPassword, string newPassword)
    {
        var selectedUser = _identityRepository.FindResourceOwnerById(user.Claims.FirstOrDefault(c => c.Type == AppConstants.idClaim)!.Value)!;
        return await _identityRepository.ChangePassword(selectedUser, currentPassword, newPassword);
    }

    public async Task<Response> ChangeEmail(string userId, string email, string token)
    {
        var selectedUser = _identityRepository.FindResourceOwnerById(userId);
        if (selectedUser is null)
        {
            return new Response
            {
                Error = AppConstants.invalidUser
            };
        }
        return await _identityRepository.ChangeEmail(selectedUser, email, token);
    }

    public async Task<Response> CreateVerificationCode(ClaimsPrincipal user)
    {
        var selectedUser = _identityRepository.FindResourceOwnerById(user.Claims.FirstOrDefault(c => c.Type == AppConstants.idClaim)!.Value)!;
        var result = await _identityRepository.GenerateEmailConfirmationToken(selectedUser);
        return new Response
        {
            Result = AppConstants.verificationTokenSuccess
        };
    }

    public Task<Response> VerifyEmail(String userId, string email, string token)
    {
        throw new NotImplementedException();
    }
}