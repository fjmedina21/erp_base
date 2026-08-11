using System.Security.Claims;
using ErpBase.Api.Authorization;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Catalogos;
using ErpBase.Api.DTOs.Proyectos;
using ErpBase.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fases")]
public sealed class FasesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [HasPermission("fases.ver")]
    public async Task<ActionResult<FaseResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ProyectoFases.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        return exists ? Ok(await LoadFaseResponse(id, cancellationToken)) : NotFound();
    }

    [HttpPut("{id:guid}")]
    [HasPermission("fases.editar")]
    public async Task<ActionResult<FaseResponse>> Update(Guid id, UpdateFaseRequest request, CancellationToken cancellationToken)
    {
        var fase = await dbContext.ProyectoFases.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (fase is null) return NotFound();

        var estadoId = request.EstadoFaseId ?? fase.EstadoFaseId;
        var estadoExists = await dbContext.EstadosFase.AnyAsync(x => x.Id == estadoId && x.DeletedAt == null, cancellationToken);
        if (!estadoExists) return BadRequest(new { message = "El estado de fase no existe." });

        var fechaInicio = request.FechaInicio ?? fase.FechaInicio;
        var fechaFin = request.FechaFin ?? fase.FechaFin;
        var fechaProrroga = request.FechaProrroga ?? fase.FechaProrroga;
        var validation = ValidateSchedule(fechaInicio, fechaFin, fechaProrroga);
        if (validation is not null) return BadRequest(new { message = validation });

        fase.EstadoFaseId = estadoId;
        fase.Nombre = request.Nombre?.Trim() ?? fase.Nombre;
        fase.Descripcion = request.Descripcion is null ? fase.Descripcion : NormalizeOptional(request.Descripcion);
        fase.FechaInicio = fechaInicio;
        fase.FechaFin = fechaFin;
        fase.FechaProrroga = fechaProrroga;
        fase.UpdatedAt = DateTime.Now;

        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "proyecto_fases" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await LoadFaseResponse(id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("fases.eliminar")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var fase = await dbContext.ProyectoFases.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (fase is null) return NotFound();

        fase.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ELIMINACION", Entidad = "proyecto_fases" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{faseId:guid}/tareas")]
    [HasPermission("tareas.ver")]
    public async Task<ActionResult<IReadOnlyCollection<TareaResponse>>> GetTareas(Guid faseId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ProyectoFases.AnyAsync(x => x.Id == faseId && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var tareas = await BaseTareaQuery()
            .Where(x => x.ProyectoFaseId == faseId)
            .OrderBy(x => x.FechaInicio)
            .ToArrayAsync(cancellationToken);

        return Ok(tareas.Select(ToResponse).ToArray());
    }

    [HttpPost("{faseId:guid}/tareas")]
    [HasPermission("tareas.crear")]
    public async Task<ActionResult<TareaResponse>> CreateTarea(Guid faseId, CreateTareaRequest request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ProyectoFases.AnyAsync(x => x.Id == faseId && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var estado = await FindEstadoTarea("Pendiente", cancellationToken);
        if (estado is null) return BadRequest(new { message = "El estado de tarea no existe o falta el catálogo Pendiente." });

        var prioridadExists = await dbContext.PrioridadesTarea.AnyAsync(x => x.Id == request.PrioridadId && x.DeletedAt == null, cancellationToken);
        if (!prioridadExists) return BadRequest(new { message = "La prioridad de tarea no existe." });

        var advance = request.PorcentajeAvance ?? 0;
        var validation = ValidateSchedule(request.FechaInicio, request.FechaFin, request.FechaProrroga, advance);
        if (validation is not null) return BadRequest(new { message = validation });

        var tarea = new ProyectoTarea
        {
            Id = Guid.NewGuid(),
            ProyectoFaseId = faseId,
            EstadoTareaId = estado.Id,
            PrioridadId = request.PrioridadId,
            Nombre = request.Nombre.Trim(),
            Descripcion = NormalizeOptional(request.Descripcion),
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            FechaProrroga = request.FechaProrroga,
            PorcentajeAvance = advance
        };

        dbContext.ProyectoTareas.Add(tarea);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = tarea.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "proyecto_tareas" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetTareas), new { faseId }, await LoadTareaResponse(tarea.Id, cancellationToken));
    }

    private IQueryable<ProyectoFase> BaseFaseQuery() =>
        dbContext.ProyectoFases.AsNoTracking()
            .Include(x => x.EstadoFase)
            .Where(x => x.DeletedAt == null);

    private IQueryable<ProyectoTarea> BaseTareaQuery() =>
        dbContext.ProyectoTareas.AsNoTracking()
            .Include(x => x.EstadoTarea)
            .Include(x => x.Prioridad)
            .Where(x => x.DeletedAt == null);

    private async Task<FaseResponse> LoadFaseResponse(Guid id, CancellationToken cancellationToken)
    {
        var fase = await BaseFaseQuery().FirstAsync(x => x.Id == id, cancellationToken);
        var avance = await CalculateFaseAvance(id, cancellationToken);
        return ToResponse(fase, avance);
    }

    private async Task<TareaResponse> LoadTareaResponse(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await BaseTareaQuery().FirstAsync(x => x.Id == id, cancellationToken);
        return ToResponse(tarea);
    }

    private async Task<EstadoTarea?> FindEstadoTarea(string nombre, CancellationToken cancellationToken) =>
        await dbContext.EstadosTarea.FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower() && x.DeletedAt == null, cancellationToken);

    private static string? ValidateSchedule(DateOnly fechaInicio, DateOnly fechaFin, DateOnly? fechaProrroga)
    {
        if (fechaFin < fechaInicio) return "La fecha fin debe ser mayor o igual a la fecha inicio.";
        if (fechaProrroga.HasValue && fechaProrroga.Value < fechaFin) return "La fecha prórroga debe ser mayor o igual a la fecha fin.";
        return null;
    }

    private static string? ValidateSchedule(DateOnly fechaInicio, DateOnly fechaFin, DateOnly? fechaProrroga, decimal porcentajeAvance)
    {
        if (fechaFin < fechaInicio) return "La fecha fin debe ser mayor o igual a la fecha inicio.";
        if (fechaProrroga.HasValue && fechaProrroga.Value < fechaFin) return "La fecha prórroga debe ser mayor o igual a la fecha fin.";
        if (porcentajeAvance < 0 || porcentajeAvance > 100) return "El porcentaje de avance debe estar entre 0 y 100.";
        return null;
    }

    private async Task<decimal> CalculateFaseAvance(Guid faseId, CancellationToken cancellationToken)
    {
        var tareas = await dbContext.ProyectoTareas
            .AsNoTracking()
            .Include(x => x.EstadoTarea)
            .Where(x => x.ProyectoFaseId == faseId && x.DeletedAt == null)
            .Select(x => x.EstadoTarea.Nombre)
            .ToArrayAsync(cancellationToken);

        if (tareas.Length == 0) return 0;

        var completadas = tareas.Count(x => string.Equals(x, "Completada", StringComparison.OrdinalIgnoreCase));
        return Math.Round(completadas * 100m / tareas.Length, 2);
    }

    private static FaseResponse ToResponse(ProyectoFase x, decimal porcentajeAvance) =>
        new(x.Id, x.ProyectoId, new CatalogoBasicoResponse(x.EstadoFaseId, x.EstadoFase.Nombre), x.Nombre, x.Descripcion, x.FechaInicio, x.FechaFin, x.FechaProrroga, porcentajeAvance, x.CreatedAt, x.UpdatedAt);

    private static TareaResponse ToResponse(ProyectoTarea x) =>
        new(x.Id, x.ProyectoFaseId, new CatalogoBasicoResponse(x.EstadoTareaId, x.EstadoTarea.Nombre), new CatalogoBasicoResponse(x.PrioridadId, x.Prioridad.Nombre), x.Nombre, x.Descripcion, x.FechaInicio, x.FechaFin, x.FechaProrroga, x.PorcentajeAvance, x.CreatedAt, x.UpdatedAt);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
