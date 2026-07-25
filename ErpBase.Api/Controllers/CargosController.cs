using System.Security.Claims;
using ErpBase.Api.Authorization;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Rrhh;
using ErpBase.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cargos")]
public sealed class CargosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [HasPermission("cargos.ver")]
    [Tags("Catalog")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogoAdminResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await dbContext.Cargos.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdminResponse(x.Id, x.Nombre, x.CreatedAt, x.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("cargos.ver")]
    public async Task<ActionResult<CatalogoAdminResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cargo = await dbContext.Cargos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        return cargo is null ? NotFound() : Ok(ToResponse(cargo));
    }

    [HttpPost]
    [HasPermission("cargos.crear")]
    public async Task<ActionResult<CatalogoAdminResponse>> Create(CreateCatalogoRequest request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();
        var existing = await dbContext.Cargos.FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (existing is not null && existing.DeletedAt == null) return Conflict(new { message = "Ya existe un cargo con este nombre." });

        var cargo = existing ?? new Cargo { Id = Guid.NewGuid() };
        cargo.Nombre = nombre;
        cargo.DeletedAt = null;
        cargo.UpdatedAt = existing is null ? null : DateTime.Now;
        if (existing is null) dbContext.Cargos.Add(cargo);

        dbContext.Auditorias.Add(new Auditoria { RegistroId = cargo.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "cargos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = cargo.Id }, ToResponse(cargo));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("cargos.editar")]
    public async Task<ActionResult<CatalogoAdminResponse>> Update(Guid id, UpdateCatalogoRequest request, CancellationToken cancellationToken)
    {
        var cargo = await dbContext.Cargos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (cargo is null) return NotFound();

        var nombre = request.Nombre.Trim();
        var duplicate = await dbContext.Cargos.AnyAsync(x => x.Id != id && x.DeletedAt == null && x.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (duplicate) return Conflict(new { message = "Ya existe otro cargo con este nombre." });

        cargo.Nombre = nombre;
        cargo.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "cargos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(cargo));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("cargos.eliminar")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var cargo = await dbContext.Cargos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (cargo is null) return NotFound();

        var used = await dbContext.Empleados.AnyAsync(x => x.CargoId == id && x.DeletedAt == null, cancellationToken);
        if (used) return Conflict(new { message = "No se puede eliminar el cargo porque tiene empleados activos asociados." });

        cargo.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ELIMINACION", Entidad = "cargos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static CatalogoAdminResponse ToResponse(Cargo x) => new(x.Id, x.Nombre, x.CreatedAt, x.UpdatedAt);

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
