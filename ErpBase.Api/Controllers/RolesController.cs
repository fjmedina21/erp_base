using System.Security.Claims;
using ErpBase.Api.Authorization;
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
    [HasPermission("roles.ver")]
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

    [HttpGet("permissions")]
    [HasPermission("permisos.ver")]
    public async Task<ActionResult<IReadOnlyCollection<PermissionResponse>>> GetPermissions(
        [FromQuery] string? modulo,
        CancellationToken cancellationToken)
    {
        var moduleFilter = NormalizeModuleFilter(modulo);
        var query = dbContext.Permissions
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (moduleFilter is not null)
        {
            query = query.Where(x => x.Codigo.ToLower().StartsWith($"{moduleFilter}."));
        }

        var permissions = await query
            .OrderBy(x => x.Codigo)
            .Select(x => new PermissionResponse(
                x.Id,
                x.Codigo,
                GetPermissionModule(x.Codigo),
                x.Nombre,
                x.Descripcion,
                x.CreatedAt))
            .ToArrayAsync(cancellationToken);

        return Ok(permissions);
    }

    [HttpPost]
    [HasPermission("roles.crear")]
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
            Id = Guid.NewGuid(),
            Nombre = name,
            Descripcion = request.Descripcion
        };

        dbContext.Roles.Add(role);
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = role.Id,
            UserId = GetCurrentUserId(),
            Accion = "CREACION",
            Entidad = "roles"
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new RoleResponse(role.Id, role.Nombre, role.Descripcion, [], role.CreatedAt);
        return CreatedAtAction(nameof(GetAll), response);
    }

    [HttpPut("{id:guid}/permissions")]
    [HasPermission("permisos.asignar")]
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
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = role.Id,
            UserId = GetCurrentUserId(),
            Accion = "PERMISOS_ASIGNADOS",
            Entidad = "roles"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("permissions")]
    [HasPermission("permisos.asignar")]
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
            Id = Guid.NewGuid(),
            Codigo = code,
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion
        };

        dbContext.Permissions.Add(permission);
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = permission.Id,
            UserId = GetCurrentUserId(),
            Accion = "CREACION",
            Entidad = "permisos"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Created("api/roles/permissions", new PermissionResponse(
            permission.Id,
            permission.Codigo,
            GetPermissionModule(permission.Codigo),
            permission.Nombre,
            permission.Descripcion,
            permission.CreatedAt));
    }

    private static string? NormalizeModuleFilter(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized.TrimEnd('.');
    }

    private static string GetPermissionModule(string code)
    {
        var separatorIndex = code.IndexOf('.', StringComparison.Ordinal);
        return separatorIndex <= 0 ? code : code[..separatorIndex];
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
