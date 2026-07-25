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
[Route("api/tareas")]
public sealed class TareasController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [HasPermission("tareas.ver")]
    public async Task<ActionResult<TareaResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await BaseTareaQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return tarea is null ? NotFound() : Ok(ToResponse(tarea));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("tareas.editar")]
    public async Task<ActionResult<TareaResponse>> Update(Guid id, UpdateTareaRequest request, CancellationToken cancellationToken)
    {
        var tarea = await dbContext.ProyectoTareas.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (tarea is null) return NotFound();

        var estadoId = request.EstadoTareaId ?? tarea.EstadoTareaId;
        var prioridadId = request.PrioridadId ?? tarea.PrioridadId;
        if (!await dbContext.EstadosTarea.AnyAsync(x => x.Id == estadoId && x.DeletedAt == null, cancellationToken)) return BadRequest(new { message = "El estado de tarea no existe." });
        if (!await dbContext.PrioridadesTarea.AnyAsync(x => x.Id == prioridadId && x.DeletedAt == null, cancellationToken)) return BadRequest(new { message = "La prioridad de tarea no existe." });

        var fechaInicio = request.FechaInicio ?? tarea.FechaInicio;
        var fechaFin = request.FechaFin ?? tarea.FechaFin;
        var fechaProrroga = request.FechaProrroga ?? tarea.FechaProrroga;
        var advance = request.PorcentajeAvance ?? tarea.PorcentajeAvance;
        var validation = ValidateSchedule(fechaInicio, fechaFin, fechaProrroga, advance);
        if (validation is not null) return BadRequest(new { message = validation });

        tarea.EstadoTareaId = estadoId;
        tarea.PrioridadId = prioridadId;
        tarea.Nombre = request.Nombre?.Trim() ?? tarea.Nombre;
        tarea.Descripcion = request.Descripcion is null ? tarea.Descripcion : NormalizeOptional(request.Descripcion);
        tarea.FechaInicio = fechaInicio;
        tarea.FechaFin = fechaFin;
        tarea.FechaProrroga = fechaProrroga;
        tarea.PorcentajeAvance = advance;
        tarea.UpdatedAt = DateTime.Now;

        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "proyecto_tareas" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await LoadTareaResponse(id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("tareas.eliminar")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await dbContext.ProyectoTareas.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (tarea is null) return NotFound();

        tarea.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ELIMINACION", Entidad = "proyecto_tareas" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{tareaId:guid}/responsables")]
    [HasPermission("tareas.ver")]
    public async Task<ActionResult<IReadOnlyCollection<TareaResponsableResponse>>> GetResponsables(Guid tareaId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ProyectoTareas.AnyAsync(x => x.Id == tareaId && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var responsables = await dbContext.ProyectoTareaResponsables.AsNoTracking()
            .Include(x => x.Empleado).ThenInclude(x => x.Departamento)
            .Include(x => x.Empleado).ThenInclude(x => x.Cargo)
            .Where(x => x.ProyectoTareaId == tareaId && x.Empleado.DeletedAt == null)
            .OrderBy(x => x.Empleado.Nombres)
            .ToArrayAsync(cancellationToken);

        return Ok(responsables.Select(ToResponse).ToArray());
    }

    [HttpPost("{tareaId:guid}/responsables")]
    [HasPermission("tareas.asignar_responsables")]
    public async Task<ActionResult<IReadOnlyCollection<TareaResponsableResponse>>> AddResponsables(Guid tareaId, AsignarResponsablesRequest request, CancellationToken cancellationToken)
    {
        var tareaExists = await dbContext.ProyectoTareas.AnyAsync(x => x.Id == tareaId && x.DeletedAt == null, cancellationToken);
        if (!tareaExists) return NotFound();

        var empleadoIds = request.EmpleadoIds.Distinct().ToArray();
        if (empleadoIds.Length == 0) return BadRequest(new { message = "Debe enviar al menos un responsable." });
        if (empleadoIds.Length != request.EmpleadoIds.Count) return BadRequest(new { message = "La lista contiene empleados repetidos." });

        var empleados = await dbContext.Empleados
            .Include(x => x.Departamento)
            .Include(x => x.Cargo)
            .Where(x => empleadoIds.Contains(x.Id) && x.DeletedAt == null)
            .ToArrayAsync(cancellationToken);
        if (empleados.Length != empleadoIds.Length) return BadRequest(new { message = "Uno o más empleados no existen o no están activos." });

        var assignedIds = await dbContext.ProyectoTareaResponsables
            .Where(x => x.ProyectoTareaId == tareaId && empleadoIds.Contains(x.EmpleadoId))
            .Select(x => x.EmpleadoId)
            .ToArrayAsync(cancellationToken);
        if (assignedIds.Length > 0) return Conflict(new { message = "Uno o más empleados ya están asignados a esta tarea.", empleadoIds = assignedIds });

        var responsables = empleados.Select(x => new ProyectoTareaResponsable { ProyectoTareaId = tareaId, EmpleadoId = x.Id, Empleado = x }).ToArray();
        dbContext.ProyectoTareaResponsables.AddRange(responsables);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = tareaId, UserId = GetCurrentUserId(), Accion = "RESPONSABLE_ASIGNADO", Entidad = "proyecto_tareas" });
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetResponsables), new { tareaId }, responsables.Select(ToResponse).ToArray());
    }

    [HttpDelete("{tareaId:guid}/responsables/{empleadoId:guid}")]
    [HasPermission("tareas.asignar_responsables")]
    public async Task<IActionResult> DeleteResponsable(Guid tareaId, Guid empleadoId, CancellationToken cancellationToken)
    {
        var responsable = await dbContext.ProyectoTareaResponsables.FirstOrDefaultAsync(x => x.ProyectoTareaId == tareaId && x.EmpleadoId == empleadoId, cancellationToken);
        if (responsable is null) return NotFound();

        dbContext.ProyectoTareaResponsables.Remove(responsable);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = tareaId, UserId = GetCurrentUserId(), Accion = "RESPONSABLE_REMOVIDO", Entidad = "proyecto_tareas" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<ProyectoTarea> BaseTareaQuery() =>
        dbContext.ProyectoTareas.AsNoTracking()
            .Include(x => x.EstadoTarea)
            .Include(x => x.Prioridad)
            .Where(x => x.DeletedAt == null);

    private async Task<TareaResponse> LoadTareaResponse(Guid id, CancellationToken cancellationToken)
    {
        var tarea = await BaseTareaQuery().FirstAsync(x => x.Id == id, cancellationToken);
        return ToResponse(tarea);
    }

    private static string? ValidateSchedule(DateOnly fechaInicio, DateOnly fechaFin, DateOnly? fechaProrroga, decimal porcentajeAvance)
    {
        if (fechaFin < fechaInicio) return "La fecha fin debe ser mayor o igual a la fecha inicio.";
        if (fechaProrroga.HasValue && fechaProrroga.Value < fechaFin) return "La fecha prórroga debe ser mayor o igual a la fecha fin.";
        if (porcentajeAvance < 0 || porcentajeAvance > 100) return "El porcentaje de avance debe estar entre 0 y 100.";
        return null;
    }

    private static TareaResponse ToResponse(ProyectoTarea x) =>
        new(x.Id, x.ProyectoFaseId, new CatalogoBasicoResponse(x.EstadoTareaId, x.EstadoTarea.Nombre), new CatalogoBasicoResponse(x.PrioridadId, x.Prioridad.Nombre), x.Nombre, x.Descripcion, x.FechaInicio, x.FechaFin, x.FechaProrroga, x.PorcentajeAvance, x.CreatedAt, x.UpdatedAt);

    private static TareaResponsableResponse ToResponse(ProyectoTareaResponsable x) =>
        new(x.EmpleadoId, $"{x.Empleado.Nombres} {x.Empleado.Apellidos}".Trim(), x.Empleado.Departamento.Nombre, x.Empleado.Cargo.Nombre);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
