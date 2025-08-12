using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PSQLModels.Contracts;
using PSQLModels.Contracts.ServiceResponses;
using PSQLModels.Tables;
using WebAPIProgram.Models;
using WebAPIProgram.Util;
using Response = WebAPIProgram.Models.Response;

namespace WebAPIProgram.v1.Controllers.User;

public class IdentityRepository : IIdentityRepository
{
    private readonly UserManager<IdentityUserExtended> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<IdentityRepository> _logger;

    public IdentityRepository(UserManager<IdentityUserExtended> userManager, ApplicationDbContext context, IPublishEndpoint publishEndpoint, ILogger<IdentityRepository> logger)
    {
        _userManager = userManager;
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
      public async Task<Response> FindResourceOwner(string username, string password, Boolean checkPassword = true)
    {

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return new Response
            {
                Error = AppConstants.invalidUser
            };
        if (checkPassword && !await _userManager.CheckPasswordAsync(user, password))
        {
            return new Response
            {
                Error = AppConstants.invalidPassword
            };
        }

        return new Response
        {
            Result = user
        };
    }

    public OAuthClient? FindClient(string clientId, string clientSecret, Boolean checkPassword = true)
    {

        OAuthClient? client;
        if (checkPassword)
        {
            client = _context.OAuthClients.SingleOrDefault(c =>
                c.ClientId == clientId && c.ClientSecret == clientSecret);
            return client;
        }

        client = _context.OAuthClients.SingleOrDefault(c =>
            c.ClientId == clientId);

        return client;
    }

    public IdentityUserExtended? FindResourceOwnerById(string id)
    {
        var user = _userManager.FindByIdAsync(id).Result;
        return user;
    }

    public async Task<OAuthClient?> FindClientById(string id)
    {
        var client = await _context.OAuthClients.SingleOrDefaultAsync(client => client.ClientId == id);
        return client;
    }

    public async Task<Response> CreateResourceOwner(Register register)
    {
        var user = new IdentityUserExtended
        {
            UserName = register.Username,
            Email = register.Email,
        };
        var result = await _userManager.CreateAsync(user, register.Password);
        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return new Response
                {
                    Error = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            // TODO: Send verification code to RabbitMQ consumer
            await GenerateEmailConfirmationToken(user);
            
            return new Response
            {
                Result = AppConstants.successfullRegistration
            };
        }

        return new Response
        {
            Error = string.Join(", ", result.Errors.Select(e => e.Description)),
        };
    }

    public async Task<Response> CreateClient(ClientCreationRequest request)
    {
        var existingClient = FindClient(request.ClientId, request.ClientSecret, false);
        if (existingClient == null)
        {
            await _context.OAuthClients.AddAsync(new OAuthClient
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Roles = request.Roles,
                AllowedScopes = request.AllowedScopes,

            });
            await _context.SaveChangesAsync();
            return new Response
            {
                Result = "Client successfully created."
            };
        }

        return new Response
        {
            Error = "Failed to create client."
        };
    }

    public UserInfoFull GetUserInfo(ClaimsPrincipal user)
    {
        return new UserInfoFull
        {
            Id = user.Claims.Where(c => c.Type == AppConstants.idClaim).Select(c => c.Value).FirstOrDefault()!,
            Username = user.Claims.Where(c => c.Type == AppConstants.usernameClaim).Select(c => c.Value).FirstOrDefault()!,
            Email = user.Claims.Where(c => c.Type == AppConstants.emailClaim).Select(c => c.Value).FirstOrDefault()!,
            Roles = user.Claims.Where(c => c.Type == AppConstants.role).Select(c => c.Value).ToList(),
            Scopes = user.Claims.Where(c => c.Type == AppConstants.scope).Select(c => c.Value).ToList(),
        };
    }

    public object GetClientInfo(ClaimsPrincipal client)
    {
        throw new NotImplementedException();
    }

    public async Task<Response> ChangePassword(IdentityUserExtended user, string currentPassword, string newPassword)
    {
        var previousPasswords = GetPreviouslyUsedPasswords(user.Id);
        if (previousPasswords is not null)
        {
            if (CheckIfPasswordWasPreviouslyUsed(user, previousPasswords, newPassword))
            {
                return new Response
                {
                    Error = AppConstants.invalidPassword
                };
            }
        }
        
        
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
        await _publishEndpoint.Publish<PasswordChangedEvent>(new
        {
            UserId = user.Id,
            PreviousPasswordHash = _userManager.PasswordHasher.HashPassword(user, currentPassword),
            Email = user.Email,
            Timestamp = DateTime.UtcNow
        });
        return new Response
        {
            Result = AppConstants.successfullyChangedPassword
        };
        }

        return new Response
        {
            Error = AppConstants.internalError
        };
    }

    public async Task<Response> ChangeEmail(IdentityUserExtended user, string newEmail, string token)
    {
        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
        if (result.Succeeded)
        {
            await _publishEndpoint.Publish<EmailChangedEvent>(new
            {
                UserId = user.Id,
                Email = user.Email,
                Timestamp = DateTime.UtcNow
            });
            return new Response
            {
                Result = AppConstants.successfullyChangedEmail
            };
        }
        
        return new Response
        {
            Error = AppConstants.internalError
        };
    }
    

    
    public IEnumerable<string>? GetPreviouslyUsedPasswords(string id)
    {
      var list =_context.PreviouslyUsedPasswords.Where(u => u.UserId == id).Select(p => p.OldPasswords).FirstOrDefault();
      if (list is null)
      {
          _context.PreviouslyUsedPasswords.Add(new PreviouslyUsedPasswords
          {
              UserId = id,
              OldPasswords = new List<string>()
          });
          return null;
      }
      return list;
    }

    public bool CheckIfPasswordWasPreviouslyUsed(IdentityUserExtended user, IEnumerable<string> passwords, string newPassword)
    {
        foreach (var password in passwords)
        {
            var result = _userManager.PasswordHasher.VerifyHashedPassword(user, password, newPassword);
            if (result == PasswordVerificationResult.Failed)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<String> GenerateEmailConfirmationToken(IdentityUserExtended user)
    {
        _logger.LogInformation($"Generating email confirmation token for user: {user.Id}");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        _logger.LogInformation($"Generated email verification token is {token}. Sending to email.");
        await _publishEndpoint.Publish<EmailVerificationTokenCreatedEvent>(new
        {
            UserId = user.Id,
            Email = user.Email,
            Token = token,
            Timestamp = DateTime.UtcNow
        });
        return token;
    }

    public Task<Response> VerifyEmail(IdentityUserExtended user, string token)
    {
        throw new NotImplementedException();
    }
}