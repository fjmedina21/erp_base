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
[Route("api/proyectos")]
public sealed class ProyectosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [HasPermission("proyectos.ver")]
    public async Task<ActionResult<IReadOnlyCollection<ProyectoResponse>>> GetAll([FromQuery] Guid? estadoId, [FromQuery] Guid? clienteId, CancellationToken cancellationToken)
    {
        var query = BaseProyectoQuery();
        if (estadoId.HasValue) query = query.Where(x => x.EstadoProyectoId == estadoId.Value);
        if (clienteId.HasValue) query = query.Where(x => x.ClienteId == clienteId.Value);

        var ids = await query.OrderByDescending(x => x.CreatedAt).Select(x => x.Id).ToArrayAsync(cancellationToken);
        var proyectos = new List<ProyectoResponse>();
        foreach (var id in ids)
        {
            proyectos.Add(await LoadProyectoResponse(id, cancellationToken));
        }

        return Ok(proyectos);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("proyectos.ver")]
    public async Task<ActionResult<ProyectoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Proyectos.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        return exists ? Ok(await LoadProyectoResponse(id, cancellationToken)) : NotFound();
    }

    [HttpPost]
    [HasPermission("proyectos.crear")]
    public async Task<ActionResult<ProyectoResponse>> Create(CreateProyectoRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateProyectoReferences(request.ClienteId, null, cancellationToken);
        if (validation.Error is not null) return BadRequest(new { message = validation.Error });

        var estado = await FindEstadoProyecto("Planificado", cancellationToken);
        if (estado is null) return BadRequest(new { message = "No existe el estado de proyecto Planificado." });

        var dateValidation = ValidateSchedule(request.FechaInicio, request.FechaFin, request.FechaProrroga);
        if (dateValidation is not null) return BadRequest(new { message = dateValidation });

        var proyecto = new Proyecto
        {
            Id = Guid.NewGuid(),
            Codigo = await GenerateCodigoProyecto(cancellationToken),
            ClienteId = request.ClienteId,
            EstadoProyectoId = estado.Id,
            Nombre = request.Nombre.Trim(),
            Descripcion = NormalizeOptional(request.Descripcion),
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            FechaProrroga = request.FechaProrroga,
            PorcentajeAvance = 0
        };

        dbContext.Proyectos.Add(proyecto);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = proyecto.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "proyectos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = proyecto.Id }, await LoadProyectoResponse(proyecto.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("proyectos.editar")]
    public async Task<ActionResult<ProyectoResponse>> Update(Guid id, UpdateProyectoRequest request, CancellationToken cancellationToken)
    {
        var proyecto = await dbContext.Proyectos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (proyecto is null) return NotFound();

        var clienteId = request.ClienteId ?? proyecto.ClienteId;
        var estadoId = request.EstadoProyectoId ?? proyecto.EstadoProyectoId;
        var validation = await ValidateProyectoReferences(clienteId, estadoId, cancellationToken);
        if (validation.Error is not null) return BadRequest(new { message = validation.Error });

        var fechaInicio = request.FechaInicio ?? proyecto.FechaInicio;
        var fechaFin = request.FechaFin ?? proyecto.FechaFin;
        var fechaProrroga = request.FechaProrroga ?? proyecto.FechaProrroga;
        var dateValidation = ValidateSchedule(fechaInicio, fechaFin, fechaProrroga);
        if (dateValidation is not null) return BadRequest(new { message = dateValidation });

        proyecto.ClienteId = clienteId;
        proyecto.EstadoProyectoId = estadoId;
        proyecto.Nombre = request.Nombre?.Trim() ?? proyecto.Nombre;
        proyecto.Descripcion = request.Descripcion is null ? proyecto.Descripcion : NormalizeOptional(request.Descripcion);
        proyecto.FechaInicio = fechaInicio;
        proyecto.FechaFin = fechaFin;
        proyecto.FechaProrroga = fechaProrroga;
        proyecto.UpdatedAt = DateTime.Now;

        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "proyectos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await LoadProyectoResponse(id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("proyectos.eliminar")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var proyecto = await dbContext.Proyectos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (proyecto is null) return NotFound();

        proyecto.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ELIMINACION", Entidad = "proyectos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{proyectoId:guid}/fases")]
    [HasPermission("fases.ver")]
    public async Task<ActionResult<IReadOnlyCollection<FaseResponse>>> GetFases(Guid proyectoId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Proyectos.AnyAsync(x => x.Id == proyectoId && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var faseIds = await BaseFaseQuery()
            .Where(x => x.ProyectoId == proyectoId)
            .OrderBy(x => x.FechaInicio)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        var fases = new List<FaseResponse>();
        foreach (var faseId in faseIds)
        {
            fases.Add(await LoadFaseResponse(faseId, cancellationToken));
        }

        return Ok(fases);
    }

    [HttpPost("{proyectoId:guid}/fases")]
    [HasPermission("fases.crear")]
    public async Task<ActionResult<FaseResponse>> CreateFase(Guid proyectoId, CreateFaseRequest request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Proyectos.AnyAsync(x => x.Id == proyectoId && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var estado = await FindEstadoFase("Pendiente", cancellationToken);
        if (estado is null) return BadRequest(new { message = "El estado de fase no existe o falta el catálogo Pendiente." });

        var validation = ValidateSchedule(request.FechaInicio, request.FechaFin, request.FechaProrroga);
        if (validation is not null) return BadRequest(new { message = validation });

        var fase = new ProyectoFase
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyectoId,
            EstadoFaseId = estado.Id,
            Nombre = request.Nombre.Trim(),
            Descripcion = NormalizeOptional(request.Descripcion),
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            FechaProrroga = request.FechaProrroga,
            PorcentajeAvance = 0
        };

        dbContext.ProyectoFases.Add(fase);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = fase.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "proyecto_fases" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetFases), new { proyectoId }, await LoadFaseResponse(fase.Id, cancellationToken));
    }

    private IQueryable<Proyecto> BaseProyectoQuery() =>
        dbContext.Proyectos.AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.EstadoProyecto)
            .Where(x => x.DeletedAt == null);

    private IQueryable<ProyectoFase> BaseFaseQuery() =>
        dbContext.ProyectoFases.AsNoTracking()
            .Include(x => x.EstadoFase)
            .Where(x => x.DeletedAt == null);

    private async Task<ProyectoResponse> LoadProyectoResponse(Guid id, CancellationToken cancellationToken)
    {
        var proyecto = await BaseProyectoQuery().FirstAsync(x => x.Id == id, cancellationToken);
        var avance = await CalculateProyectoAvance(id, cancellationToken);
        return ToResponse(proyecto, avance);
    }

    private async Task<FaseResponse> LoadFaseResponse(Guid id, CancellationToken cancellationToken)
    {
        var fase = await BaseFaseQuery().FirstAsync(x => x.Id == id, cancellationToken);
        var avance = await CalculateFaseAvance(id, cancellationToken);
        return ToResponse(fase, avance);
    }

    private async Task<(string? Error, EstadoProyecto? Estado)> ValidateProyectoReferences(Guid? clienteId, Guid? estadoProyectoId, CancellationToken cancellationToken)
    {
        if (clienteId.HasValue && !await dbContext.Clientes.AnyAsync(x => x.Id == clienteId.Value && x.DeletedAt == null, cancellationToken)) return ("El cliente no existe.", null);
        if (!estadoProyectoId.HasValue) return (null, null);

        var estado = await dbContext.EstadosProyecto.FirstOrDefaultAsync(x => x.Id == estadoProyectoId.Value && x.DeletedAt == null, cancellationToken);
        return estado is null ? ("El estado de proyecto no existe.", null) : (null, estado);
    }

    private async Task<EstadoProyecto?> FindEstadoProyecto(string nombre, CancellationToken cancellationToken) =>
        await dbContext.EstadosProyecto.FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower() && x.DeletedAt == null, cancellationToken);

    private async Task<EstadoFase?> FindEstadoFase(string nombre, CancellationToken cancellationToken) =>
        await dbContext.EstadosFase.FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower() && x.DeletedAt == null, cancellationToken);

    private async Task<string> GenerateCodigoProyecto(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PROY-{year}-";
        var codes = await dbContext.Proyectos.AsNoTracking()
            .Where(x => x.Codigo.StartsWith(prefix))
            .Select(x => x.Codigo)
            .ToArrayAsync(cancellationToken);

        var next = codes.Select(x => int.TryParse(x[prefix.Length..], out var value) ? value : 0).DefaultIfEmpty(0).Max() + 1;
        return $"{prefix}{next:0000}";
    }

    private static string? ValidateSchedule(DateOnly fechaInicio, DateOnly fechaFin, DateOnly? fechaProrroga)
    {
        if (fechaFin < fechaInicio) return "La fecha fin debe ser mayor o igual a la fecha inicio.";
        if (fechaProrroga.HasValue && fechaProrroga.Value < fechaFin) return "La fecha prórroga debe ser mayor o igual a la fecha fin.";
        return null;
    }

    private async Task<decimal> CalculateProyectoAvance(Guid proyectoId, CancellationToken cancellationToken)
    {
        var fases = await dbContext.ProyectoFases
            .AsNoTracking()
            .Include(x => x.EstadoFase)
            .Where(x => x.ProyectoId == proyectoId && x.DeletedAt == null)
            .Select(x => x.EstadoFase.Nombre)
            .ToArrayAsync(cancellationToken);

        return CalculateCompletionPercentage(fases, "Finalizada");
    }

    private async Task<decimal> CalculateFaseAvance(Guid faseId, CancellationToken cancellationToken)
    {
        var tareas = await dbContext.ProyectoTareas
            .AsNoTracking()
            .Include(x => x.EstadoTarea)
            .Where(x => x.ProyectoFaseId == faseId && x.DeletedAt == null)
            .Select(x => x.EstadoTarea.Nombre)
            .ToArrayAsync(cancellationToken);

        return CalculateCompletionPercentage(tareas, "Completada");
    }

    private static decimal CalculateCompletionPercentage(IReadOnlyCollection<string> states, string completedState)
    {
        if (states.Count == 0) return 0;

        var completed = states.Count(x => string.Equals(x, completedState, StringComparison.OrdinalIgnoreCase));
        return Math.Round(completed * 100m / states.Count, 2);
    }

    private static ProyectoResponse ToResponse(Proyecto x, decimal porcentajeAvance) =>
        new(x.Id, x.Codigo, x.ClienteId, FormatCliente(x.Cliente), new CatalogoBasicoResponse(x.EstadoProyectoId, x.EstadoProyecto.Nombre), x.Nombre, x.Descripcion, x.FechaInicio, x.FechaFin, x.FechaProrroga, porcentajeAvance, x.CreatedAt, x.UpdatedAt);

    private static FaseResponse ToResponse(ProyectoFase x, decimal porcentajeAvance) =>
        new(x.Id, x.ProyectoId, new CatalogoBasicoResponse(x.EstadoFaseId, x.EstadoFase.Nombre), x.Nombre, x.Descripcion, x.FechaInicio, x.FechaFin, x.FechaProrroga, porcentajeAvance, x.CreatedAt, x.UpdatedAt);

    private static string? FormatCliente(Cliente? cliente)
    {
        if (cliente is null) return null;
        if (!string.IsNullOrWhiteSpace(cliente.RazonSocial)) return cliente.RazonSocial;
        return $"{cliente.Nombres} {cliente.Apellidos}".Trim();
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
