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
[Route("api/empleados")]
public sealed class EmpleadosController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [HasPermission("empleados.ver")]
    public async Task<ActionResult<IReadOnlyCollection<EmpleadoResponse>>> GetAll([FromQuery] Guid? departamentoId, [FromQuery] Guid? cargoId, CancellationToken cancellationToken)
    {
        var query = dbContext.Empleados.AsNoTracking()
            .Include(x => x.Usuario)
            .Include(x => x.Departamento)
            .Include(x => x.Cargo)
            .Where(x => x.DeletedAt == null);

        if (departamentoId.HasValue) query = query.Where(x => x.DepartamentoId == departamentoId.Value);
        if (cargoId.HasValue) query = query.Where(x => x.CargoId == cargoId.Value);

        var empleados = await query.OrderBy(x => x.Nombres).ThenBy(x => x.Apellidos).ToArrayAsync(cancellationToken);
        return Ok(empleados.Select(ToResponse).ToArray());
    }

    [HttpGet("{id:guid}")]
    [HasPermission("empleados.ver")]
    public async Task<ActionResult<EmpleadoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var empleado = await dbContext.Empleados.AsNoTracking()
            .Include(x => x.Usuario)
            .Include(x => x.Departamento)
            .Include(x => x.Cargo)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        return empleado is null ? NotFound() : Ok(ToResponse(empleado));
    }

    [HttpPost]
    [HasPermission("empleados.crear")]
    public async Task<ActionResult<EmpleadoResponse>> Create(CreateEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateReferences(request.UsuarioId, request.DepartamentoId, request.CargoId, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });
        if (request.FechaSalida.HasValue && request.FechaSalida.Value < request.FechaIngreso) return BadRequest(new { message = "La fecha salida debe ser mayor o igual a la fecha ingreso." });

        var duplicate = await dbContext.Empleados.AnyAsync(x => x.UsuarioId == request.UsuarioId && x.DeletedAt == null, cancellationToken);
        if (duplicate) return Conflict(new { message = "Ya existe un empleado activo asociado a este usuario." });

        var empleado = new Empleado
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            DepartamentoId = request.DepartamentoId,
            CargoId = request.CargoId,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Telefono = NormalizeOptional(request.Telefono),
            FechaIngreso = request.FechaIngreso,
            FechaSalida = request.FechaSalida
        };

        dbContext.Empleados.Add(empleado);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = empleado.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "empleados" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = empleado.Id }, await LoadResponse(empleado.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [HasPermission("empleados.editar")]
    public async Task<ActionResult<EmpleadoResponse>> Update(Guid id, UpdateEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var empleado = await dbContext.Empleados.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (empleado is null) return NotFound();

        var departamentoId = request.DepartamentoId ?? empleado.DepartamentoId;
        var cargoId = request.CargoId ?? empleado.CargoId;
        var validation = await ValidateReferences(empleado.UsuarioId, departamentoId, cargoId, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });

        var fechaIngreso = request.FechaIngreso ?? empleado.FechaIngreso;
        var fechaSalida = request.FechaSalida ?? empleado.FechaSalida;
        if (fechaSalida.HasValue && fechaSalida.Value < fechaIngreso) return BadRequest(new { message = "La fecha salida debe ser mayor o igual a la fecha ingreso." });

        empleado.DepartamentoId = departamentoId;
        empleado.CargoId = cargoId;
        empleado.Nombres = request.Nombres?.Trim() ?? empleado.Nombres;
        empleado.Apellidos = request.Apellidos?.Trim() ?? empleado.Apellidos;
        empleado.Telefono = request.Telefono is null ? empleado.Telefono : NormalizeOptional(request.Telefono);
        empleado.FechaIngreso = fechaIngreso;
        empleado.FechaSalida = fechaSalida;
        empleado.UpdatedAt = DateTime.Now;

        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "empleados" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await LoadResponse(id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("empleados.eliminar")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var empleado = await dbContext.Empleados.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (empleado is null) return NotFound();

        var assigned = await dbContext.ProyectoTareaResponsables.AnyAsync(x => x.EmpleadoId == id, cancellationToken);
        if (assigned) return Conflict(new { message = "No se puede eliminar el empleado porque está asignado a tareas." });

        empleado.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ELIMINACION", Entidad = "empleados" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<string?> ValidateReferences(Guid usuarioId, Guid departamentoId, Guid cargoId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Users.AnyAsync(x => x.Id == usuarioId && x.DeletedAt == null && x.Activo, cancellationToken)) return "El usuario no existe o no está activo.";
        if (!await dbContext.Departamentos.AnyAsync(x => x.Id == departamentoId && x.DeletedAt == null, cancellationToken)) return "El departamento no existe.";
        if (!await dbContext.Cargos.AnyAsync(x => x.Id == cargoId && x.DeletedAt == null, cancellationToken)) return "El cargo no existe.";
        return null;
    }

    private async Task<EmpleadoResponse> LoadResponse(Guid id, CancellationToken cancellationToken)
    {
        var empleado = await dbContext.Empleados.AsNoTracking()
            .Include(x => x.Usuario)
            .Include(x => x.Departamento)
            .Include(x => x.Cargo)
            .FirstAsync(x => x.Id == id, cancellationToken);

        return ToResponse(empleado);
    }

    private static EmpleadoResponse ToResponse(Empleado x) =>
        new(x.Id, x.UsuarioId, x.Usuario.Email, x.DepartamentoId, x.Departamento.Nombre, x.CargoId, x.Cargo.Nombre, x.Nombres, x.Apellidos, x.Telefono, x.FechaIngreso, x.FechaSalida, x.CreatedAt, x.UpdatedAt);

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
