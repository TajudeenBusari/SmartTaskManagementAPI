using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace SmartTaskManagementAPI.Authentication.AuthorizationHandler;

public class UserRequestAuthorizationHandler: AuthorizationHandler<UserRequestRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserRequestAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRequestRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        //extract userId from Uri
        var userIdFromUri = httpContext?.Request.RouteValues["userId"]?.ToString();
        
        //extract userId from the claims in the JWT
        var userIdFromJwt = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        
        //check if user has user role
        var isRoleUser = context.User.IsInRole("User");
        
        //check if user has user Admin
        var isRoleAdmin = context.User.IsInRole("Admin");
        
        //compare userIds
        var userIdsMatch = userIdFromUri != null && userIdFromUri.Equals(userIdFromJwt);

        if (isRoleAdmin || (isRoleUser && userIdsMatch))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
        
    }
}

public class UserRequestRequirement : IAuthorizationRequirement
{
    
}

//Register this custom Authorization policy in program.cs or startup.cs