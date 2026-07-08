using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Roles;
using ErpBase.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/roles")]
public sealed class RolesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RoleResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new RoleResponse(
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.RolePermissions.Select(rp => rp.Permission.Codigo).ToArray(),
                x.CreatedAt))
            .ToArrayAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var name = request.Nombre.Trim();
        var exists = await dbContext.Roles.AnyAsync(x => x.Nombre == name, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Ya existe un rol con este nombre." });
        }

        var role = new Role
        {
            Nombre = name,
            Descripcion = request.Descripcion
        };

        dbContext.Roles.Add(role);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new RoleResponse(role.Id, role.Nombre, role.Descripcion, [], role.CreatedAt);
        return CreatedAtAction(nameof(GetAll), response);
    }

    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> AssignPermissions(Guid id, AssignPermissionsRequest request, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        var permissionIds = request.PermissionIds.Distinct().ToArray();
        var existingPermissionsCount = await dbContext.Permissions
            .CountAsync(x => permissionIds.Contains(x.Id) && x.DeletedAt == null, cancellationToken);

        if (existingPermissionsCount != permissionIds.Length)
        {
            return BadRequest(new { message = "Uno o más permisos no existen." });
        }

        role.RolePermissions.Clear();
        foreach (var permissionId in permissionIds)
        {
            role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permissionId });
        }

        role.UpdatedAt = DateTime.Now;

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("permissions")]
    public async Task<ActionResult> CreatePermission(CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        var code = request.Codigo.Trim().ToUpperInvariant();
        var exists = await dbContext.Permissions.AnyAsync(x => x.Codigo == code, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Ya existe un permiso con este código." });
        }

        var permission = new Permission
        {
            Codigo = code,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion
        };

        dbContext.Permissions.Add(permission);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Created("api/roles/permissions", new
        {
            permission.Id,
            permission.Codigo,
            permission.Nombre,
            permission.Descripcion
        });
    }
}
