using System.Security.Claims;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Catalogos;
using ErpBase.Api.DTOs.Contratos;
using ErpBase.Api.Entities;
using ErpBase.Api.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/contratos")]
public sealed class ContratosController(AppDbContext dbContext, ICloudinaryStorageService storageService) : ControllerBase
{
    [HttpGet("tipos")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogoItemResponse>>> GetTiposContrato(CancellationToken cancellationToken)
    {
        return Ok(await dbContext.TiposContrato
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoItemResponse(x.Id, x.Nombre))
            .ToArrayAsync(cancellationToken));
    }

    [HttpGet("estados")]
    public async Task<ActionResult<IReadOnlyCollection<CatalogoItemResponse>>> GetEstadosContrato(CancellationToken cancellationToken)
    {
        return Ok(await dbContext.EstadosContrato
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.Nombre)
            .Select(x => new CatalogoItemResponse(x.Id, x.Nombre))
            .ToArrayAsync(cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ContratoResponse>> Create([FromForm] CreateContratoRequest request, CancellationToken cancellationToken)
    {
        if (request.Archivo is null || request.Archivo.Length == 0) return BadRequest(new { message = "El archivo del contrato es requerido." });
        if (!await dbContext.Clientes.AnyAsync(x => x.Id == request.ClienteId && x.DeletedAt == null, cancellationToken)) return BadRequest(new { message = "El cliente no existe." });
        if (!await dbContext.Propiedades.AnyAsync(x => x.Id == request.PropiedadId && x.DeletedAt == null, cancellationToken)) return BadRequest(new { message = "La propiedad no existe." });
        if (request.FechaFin.HasValue && request.FechaFin.Value < request.FechaInicio) return BadRequest(new { message = "La fecha fin debe ser mayor o igual a la fecha inicio." });

        var tipo = await dbContext.TiposContrato.FirstOrDefaultAsync(x => x.Id == request.TipoContratoId && x.DeletedAt == null, cancellationToken);
        if (tipo is null) return BadRequest(new { message = "El tipo de contrato no existe." });

        var estado = request.EstadoContratoId.HasValue
            ? await dbContext.EstadosContrato.FirstOrDefaultAsync(x => x.Id == request.EstadoContratoId.Value && x.DeletedAt == null, cancellationToken)
            : await FindEstadoContrato("Activo", cancellationToken);

        if (estado is null) return BadRequest(new { message = "El estado de contrato no existe o falta el catálogo Activo." });

        var activeDuplicate = await dbContext.Contratos.AnyAsync(x => x.PropiedadId == request.PropiedadId && x.EstadoContratoId == estado.Id && x.DeletedAt == null, cancellationToken);
        if (activeDuplicate) return Conflict(new { message = "Ya existe un contrato activo para esta propiedad." });

        var contratoId = Guid.NewGuid();
        CloudinaryUploadResult upload;
        try
        {
            upload = await storageService.UploadAsync(request.Archivo, $"contratos/{contratoId}", CloudinaryResourceType.Raw, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "No se pudo subir el contrato a Cloudinary.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        var contrato = new Contrato
        {
            Id = contratoId,
            Codigo = $"CTR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            ClienteId = request.ClienteId,
            PropiedadId = request.PropiedadId,
            TipoContratoId = tipo.Id,
            EstadoContratoId = estado.Id,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            RutaArchivo = upload.Url,
            Observaciones = request.Observaciones
        };

        dbContext.Contratos.Add(contrato);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = contrato.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "contratos" });
        await dbContext.SaveChangesAsync(cancellationToken);
        contrato.TipoContrato = tipo;
        contrato.EstadoContrato = estado;
        return CreatedAtAction(nameof(GetById), new { id = contrato.Id }, ToResponse(contrato));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ContratoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var contratos = await dbContext.Contratos.AsNoTracking()
            .Include(x => x.TipoContrato)
            .Include(x => x.EstadoContrato)
            .Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);

        return Ok(contratos.Select(ToResponse).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContratoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var contrato = await dbContext.Contratos.AsNoTracking()
            .Include(x => x.TipoContrato)
            .Include(x => x.EstadoContrato)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        return contrato is null ? NotFound() : Ok(ToResponse(contrato));
    }

    [HttpGet("proximos-vencer")]
    public async Task<ActionResult<IReadOnlyCollection<ContratoResponse>>> GetProximosVencer([FromQuery] int dias = 30, CancellationToken cancellationToken = default)
    {
        var hasta = DateOnly.FromDateTime(DateTime.Today.AddDays(dias));
        var contratos = await dbContext.Contratos.AsNoTracking().Include(x => x.TipoContrato).Include(x => x.EstadoContrato)
            .Where(x => x.DeletedAt == null && x.FechaFin != null && x.FechaFin <= hasta)
            .OrderBy(x => x.FechaFin)
            .ToArrayAsync(cancellationToken);

        return Ok(contratos.Select(ToResponse).ToArray());
    }

    private async Task<EstadoContrato?> FindEstadoContrato(string nombre, CancellationToken cancellationToken)
    {
        return await dbContext.EstadosContrato.FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower() && x.DeletedAt == null, cancellationToken);
    }

    private static ContratoResponse ToResponse(Contrato x)
    {
        return new ContratoResponse(x.Id, x.Codigo, x.ClienteId, x.PropiedadId, x.TipoContrato.Nombre, x.EstadoContrato.Nombre, x.FechaInicio, x.FechaFin, x.RutaArchivo, x.Observaciones, x.CreatedAt);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
