using System.Security.Claims;
using ErpBase.Api.Authorization;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Users;
using ErpBase.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [HasPermission("usuarios.ver")]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Email)
            .Select(x => new UserResponse(
                x.Id,
                x.Email,
                x.Activo,
                x.Bloqueado,
                x.MotivoBloqueo,
                x.BloqueadoAt,
                x.UserRoles.Select(ur => ur.Role.Nombre).ToArray(),
                x.CreatedAt))
            .ToArrayAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("usuarios.ver")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new UserResponse(
                x.Id,
                x.Email,
                x.Activo,
                x.Bloqueado,
                x.MotivoBloqueo,
                x.BloqueadoAt,
                x.UserRoles.Select(ur => ur.Role.Nombre).ToArray(),
                x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPatch("{id:guid}/status")]
    [HasPermission("usuarios.editar")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Activo = request.Activo;
        user.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            UserId = GetCurrentUserId(),
            Accion = "ESTADO_ACTUALIZADO",
            Entidad = "usuarios"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/block")]
    [HasPermission("usuarios.editar")]
    public async Task<IActionResult> Block(Guid id, BlockUserRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (!user.Activo)
        {
            return BadRequest(new { message = "Solo se pueden bloquear usuarios activos." });
        }

        user.Bloqueado = true;
        user.MotivoBloqueo = request.Motivo.Trim();
        user.BloqueadoAt = DateTime.Now;
        user.ReactivadoAt = null;
        user.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            UserId = GetCurrentUserId(),
            Accion = "BLOQUEO",
            Entidad = "usuarios"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    [HasPermission("usuarios.editar")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Activo = true;
        user.Bloqueado = false;
        user.MotivoBloqueo = null;
        user.ReactivadoAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            UserId = GetCurrentUserId(),
            Accion = "REACTIVACION",
            Entidad = "usuarios"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/roles")]
    [HasPermission("usuarios.editar")]
    public async Task<IActionResult> AssignRoles(Guid id, AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var roleIds = request.RoleIds.Distinct().ToArray();
        var existingRolesCount = await dbContext.Roles
            .CountAsync(x => roleIds.Contains(x.Id) && x.DeletedAt == null, cancellationToken);

        if (existingRolesCount != roleIds.Length)
        {
            return BadRequest(new { message = "Uno o más roles no existen." });
        }

        user.UserRoles.Clear();
        foreach (var roleId in roleIds)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        }

        user.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            UserId = GetCurrentUserId(),
            Accion = "ROLES_ASIGNADOS",
            Entidad = "usuarios"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
