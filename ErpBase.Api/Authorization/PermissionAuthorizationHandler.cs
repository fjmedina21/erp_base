using System.Security.Claims;
using ErpBase.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Authorization;

public sealed class PermissionAuthorizationHandler(AppDbContext dbContext) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userIdValue = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var permission = requirement.Permission.Trim().ToLowerInvariant();
        var hasPermission = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user =>
                user.Id == userId &&
                user.DeletedAt == null &&
                user.Activo &&
                !user.Bloqueado &&
                user.UserRoles.Any(userRole =>
                    userRole.Role.DeletedAt == null &&
                    userRole.Role.RolePermissions.Any(rolePermission =>
                        rolePermission.Permission.DeletedAt == null &&
                        rolePermission.Permission.Codigo.ToLower() == permission)));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
