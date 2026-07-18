using System.Security.Claims;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Rrhh;
using ErpBase.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/departamentos")]
public sealed class DepartamentosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Tags("Catalog")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogoAdminResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await dbContext.Departamentos.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoAdminResponse(x.Id, x.Nombre, x.CreatedAt, x.UpdatedAt))
            .ToArrayAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogoAdminResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var departamento = await dbContext.Departamentos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        return departamento is null ? NotFound() : Ok(ToResponse(departamento));
    }

    [HttpPost]
    public async Task<ActionResult<CatalogoAdminResponse>> Create(CreateCatalogoRequest request, CancellationToken cancellationToken)
    {
        var nombre = request.Nombre.Trim();
        var existing = await dbContext.Departamentos.FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (existing is not null && existing.DeletedAt == null) return Conflict(new { message = "Ya existe un departamento con este nombre." });

        var departamento = existing ?? new Departamento { Id = Guid.NewGuid() };
        departamento.Nombre = nombre;
        departamento.DeletedAt = null;
        departamento.UpdatedAt = existing is null ? null : DateTime.Now;
        if (existing is null) dbContext.Departamentos.Add(departamento);

        dbContext.Auditorias.Add(new Auditoria { RegistroId = departamento.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "departamentos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = departamento.Id }, ToResponse(departamento));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CatalogoAdminResponse>> Update(Guid id, UpdateCatalogoRequest request, CancellationToken cancellationToken)
    {
        var departamento = await dbContext.Departamentos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (departamento is null) return NotFound();

        var nombre = request.Nombre.Trim();
        var duplicate = await dbContext.Departamentos.AnyAsync(x => x.Id != id && x.DeletedAt == null && x.Nombre.ToLower() == nombre.ToLower(), cancellationToken);
        if (duplicate) return Conflict(new { message = "Ya existe otro departamento con este nombre." });

        departamento.Nombre = nombre;
        departamento.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "departamentos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(departamento));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var departamento = await dbContext.Departamentos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (departamento is null) return NotFound();

        var used = await dbContext.Empleados.AnyAsync(x => x.DepartamentoId == id && x.DeletedAt == null, cancellationToken);
        if (used) return Conflict(new { message = "No se puede eliminar el departamento porque tiene empleados activos asociados." });

        departamento.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ELIMINACION", Entidad = "departamentos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static CatalogoAdminResponse ToResponse(Departamento x) => new(x.Id, x.Nombre, x.CreatedAt, x.UpdatedAt);

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
