using PSQLModels.Tables;
using WebAPIProgram.Requirements;

namespace WebAPIProgram.Handlers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public class EmailConfirmedHandler : AuthorizationHandler<EmailConfirmedRequirement>
{
    private readonly UserManager<IdentityUserExtended> _userManager;

    public EmailConfirmedHandler(UserManager<IdentityUserExtended> userManager)
    {
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmailConfirmedRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return;

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && await _userManager.IsEmailConfirmedAsync(user))
        {
            context.Succeed(requirement);
        }
        
        else
            context.Fail();
    }
}
