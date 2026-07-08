using System.Security.Claims;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Propiedades;
using ErpBase.Api.Entities;
using ErpBase.Api.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/propiedades")]
public sealed class PropiedadesController(AppDbContext dbContext, ICloudinaryStorageService storageService) : ControllerBase
{
    private static readonly string[] ImageTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private static readonly string[] DocumentTypes = ["application/pdf", .. ImageTypes];

    [HttpPost]
    public async Task<ActionResult<PropiedadResponse>> Create(CreatePropiedadRequest request, CancellationToken cancellationToken)
    {
        var validation = await ValidateReferences(request.TipoPropiedadId, request.EstadoPropiedadId, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });
        var propertyValidation = ValidatePropertyValues(request.Latitud, request.Longitud, request.MetrosCuadradosTerreno, request.MetrosCuadradosConstruccion, request.Habitaciones, request.Banos, request.PrecioVenta, request.PrecioAlquiler);
        if (propertyValidation is not null) return BadRequest(new { message = propertyValidation });

        var exists = await dbContext.Propiedades.AnyAsync(x => x.Codigo == request.Codigo.Trim() && x.DeletedAt == null, cancellationToken);
        if (exists) return Conflict(new { message = "Ya existe una propiedad con este código." });

        var propiedad = new Propiedad
        {
            Id = Guid.NewGuid(),
            Codigo = request.Codigo.Trim().ToUpperInvariant(),
            TipoPropiedadId = request.TipoPropiedadId,
            EstadoPropiedadId = request.EstadoPropiedadId,
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion,
            Latitud = request.Latitud,
            Longitud = request.Longitud,
            Direccion = request.Direccion.Trim(),
            MetrosCuadradosTerreno = request.MetrosCuadradosTerreno,
            MetrosCuadradosConstruccion = request.MetrosCuadradosConstruccion,
            Habitaciones = request.Habitaciones,
            Banos = request.Banos,
            PrecioVenta = request.PrecioVenta,
            PrecioAlquiler = request.PrecioAlquiler
        };

        dbContext.Propiedades.Add(propiedad);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = propiedad.Id, UserId = GetCurrentUserId(), Accion = "CREACION", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = propiedad.Id }, await ToResponse(propiedad.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PropiedadResponse>> Update(Guid id, UpdatePropiedadRequest request, CancellationToken cancellationToken)
    {
        var propiedad = await dbContext.Propiedades.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (propiedad is null) return NotFound();

        var tipoId = request.TipoPropiedadId ?? propiedad.TipoPropiedadId;
        var estadoId = request.EstadoPropiedadId ?? propiedad.EstadoPropiedadId;
        var validation = await ValidateReferences(tipoId, estadoId, cancellationToken);
        if (validation is not null) return BadRequest(new { message = validation });
        var latitud = request.Latitud ?? propiedad.Latitud;
        var longitud = request.Longitud ?? propiedad.Longitud;
        var metrosTerreno = request.MetrosCuadradosTerreno ?? propiedad.MetrosCuadradosTerreno;
        var metrosConstruccion = request.MetrosCuadradosConstruccion ?? propiedad.MetrosCuadradosConstruccion;
        var habitaciones = request.Habitaciones ?? propiedad.Habitaciones;
        var banos = request.Banos ?? propiedad.Banos;
        var precioVenta = request.PrecioVenta ?? propiedad.PrecioVenta;
        var precioAlquiler = request.PrecioAlquiler ?? propiedad.PrecioAlquiler;
        var propertyValidation = ValidatePropertyValues(latitud, longitud, metrosTerreno, metrosConstruccion, habitaciones, banos, precioVenta, precioAlquiler);
        if (propertyValidation is not null) return BadRequest(new { message = propertyValidation });

        propiedad.TipoPropiedadId = tipoId;
        propiedad.EstadoPropiedadId = estadoId;
        propiedad.Titulo = request.Titulo?.Trim() ?? propiedad.Titulo;
        propiedad.Descripcion = request.Descripcion ?? propiedad.Descripcion;
        propiedad.Latitud = latitud;
        propiedad.Longitud = longitud;
        propiedad.Direccion = request.Direccion?.Trim() ?? propiedad.Direccion;
        propiedad.MetrosCuadradosTerreno = metrosTerreno;
        propiedad.MetrosCuadradosConstruccion = metrosConstruccion;
        propiedad.Habitaciones = habitaciones;
        propiedad.Banos = banos;
        propiedad.PrecioVenta = precioVenta;
        propiedad.PrecioAlquiler = precioAlquiler;
        propiedad.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "ACTUALIZACION", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(await ToResponse(id, cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PropiedadResponse>>> GetAll([FromQuery] Guid? tipo, [FromQuery] Guid? estado, CancellationToken cancellationToken)
    {
        var query = dbContext.Propiedades.AsNoTracking().Where(x => x.DeletedAt == null);
        if (tipo.HasValue) query = query.Where(x => x.TipoPropiedadId == tipo.Value);
        if (estado.HasValue) query = query.Where(x => x.EstadoPropiedadId == estado.Value);
        var ids = await query.OrderByDescending(x => x.CreatedAt).Select(x => x.Id).ToArrayAsync(cancellationToken);
        var items = new List<PropiedadResponse>();
        foreach (var id in ids) items.Add(await ToResponse(id, cancellationToken));
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropiedadResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Propiedades.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        return exists ? Ok(await ToResponse(id, cancellationToken)) : NotFound();
    }

    [HttpGet("{id:guid}/inversionistas")]
    public async Task<ActionResult<IReadOnlyCollection<PropiedadInversionistaResponse>>> GetInversionistas(Guid id, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Propiedades.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var inversionistas = await dbContext.PropiedadInversionistas
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Where(x => x.PropiedadId == id && x.DeletedAt == null)
            .OrderByDescending(x => x.PorcentajeParticipacion)
            .ToArrayAsync(cancellationToken);

        return Ok(inversionistas.Select(ToInversionistaResponse).ToArray());
    }

    [HttpPost("{id:guid}/inversionistas")]
    public async Task<ActionResult<PropiedadInversionistaResponse>> AddInversionista(Guid id, InsertPropiedadInversionistaRequest request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Propiedades.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);
        if (!exists) return NotFound();

        var cliente = await dbContext.Clientes.FirstOrDefaultAsync(x => x.Id == request.ClienteId && x.DeletedAt == null, cancellationToken);
        if (cliente is null) return BadRequest(new { message = "El cliente inversionista no existe." });

        var porcentajeValidation = await ValidateParticipacion(id, request.ClienteId, request.PorcentajeParticipacion, cancellationToken);
        if (porcentajeValidation is not null) return BadRequest(new { message = porcentajeValidation });

        var current = await dbContext.PropiedadInversionistas
            .FirstOrDefaultAsync(x => x.PropiedadId == id && x.ClienteId == request.ClienteId, cancellationToken);

        if (current is not null && current.DeletedAt == null)
        {
            return Conflict(new { message = "Este cliente ya está asociado como inversionista de la propiedad." });
        }

        if (current is null)
        {
            current = new PropiedadInversionista
            {
                PropiedadId = id,
                ClienteId = request.ClienteId,
                PorcentajeParticipacion = request.PorcentajeParticipacion,
                Observaciones = request.Observaciones
            };
            dbContext.PropiedadInversionistas.Add(current);
        }
        else
        {
            current.PorcentajeParticipacion = request.PorcentajeParticipacion;
            current.Observaciones = request.Observaciones;
            current.DeletedAt = null;
            current.UpdatedAt = DateTime.Now;
        }

        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "INVERSIONISTA_ASOCIADO", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);

        current.Cliente = cliente;
        return CreatedAtAction(nameof(GetInversionistas), new { id }, ToInversionistaResponse(current));
    }

    [HttpPut("{id:guid}/inversionistas/{clienteId:guid}")]
    public async Task<ActionResult<PropiedadInversionistaResponse>> UpdateInversionista(Guid id, Guid clienteId, UpdatePropiedadInversionistaRequest request, CancellationToken cancellationToken)
    {
        var inversionista = await dbContext.PropiedadInversionistas
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.PropiedadId == id && x.ClienteId == clienteId && x.DeletedAt == null, cancellationToken);

        if (inversionista is null) return NotFound();

        var porcentajeValidation = await ValidateParticipacion(id, clienteId, request.PorcentajeParticipacion, cancellationToken);
        if (porcentajeValidation is not null) return BadRequest(new { message = porcentajeValidation });

        inversionista.PorcentajeParticipacion = request.PorcentajeParticipacion;
        inversionista.Observaciones = request.Observaciones;
        inversionista.UpdatedAt = DateTime.Now;

        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "INVERSIONISTA_ACTUALIZADO", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToInversionistaResponse(inversionista));
    }

    [HttpDelete("{id:guid}/inversionistas/{clienteId:guid}")]
    public async Task<IActionResult> DeleteInversionista(Guid id, Guid clienteId, CancellationToken cancellationToken)
    {
        var inversionista = await dbContext.PropiedadInversionistas
            .FirstOrDefaultAsync(x => x.PropiedadId == id && x.ClienteId == clienteId && x.DeletedAt == null, cancellationToken);

        if (inversionista is null) return NotFound();

        inversionista.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "INVERSIONISTA_REMOVIDO", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/documentos")]
    public async Task<ActionResult<PropiedadDocumentoResponse>> AddDocumento(Guid id, [FromForm] PropiedadDocumentoUploadRequest request, CancellationToken cancellationToken)
    {
        var fileValidation = ValidateFile(request.Archivo, DocumentTypes, "Solo se permiten PDF e imágenes.");
        if (fileValidation is not null) return BadRequest(new { message = fileValidation });
        if (!await dbContext.Propiedades.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken)) return NotFound();

        CloudinaryUploadResult upload;
        try
        {
            upload = await storageService.UploadAsync(request.Archivo, $"propiedades/{id}/documentos", CloudinaryResourceType.Raw, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "No se pudo subir el documento a Cloudinary.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        var doc = new PropiedadDocumento
        {
            PropiedadId = id,
            NombreArchivo = request.Archivo.FileName,
            RutaArchivo = upload.Url,
            TipoContenido = request.Archivo.ContentType,
            CloudinaryPublicId = upload.PublicId,
            CloudinaryResourceType = upload.ResourceType
        };
        dbContext.PropiedadDocumentos.Add(doc);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "DOCUMENTO_AGREGADO", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetDocumentos), new { id }, new PropiedadDocumentoResponse(doc.Id, id, doc.NombreArchivo, doc.RutaArchivo, doc.TipoContenido, doc.CloudinaryPublicId, doc.CloudinaryResourceType, doc.CreatedAt));
    }

    [HttpGet("{id:guid}/documentos")]
    public async Task<ActionResult<IReadOnlyCollection<PropiedadDocumentoResponse>>> GetDocumentos(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await dbContext.PropiedadDocumentos.AsNoTracking().Where(x => x.PropiedadId == id && x.DeletedAt == null).Select(x => new PropiedadDocumentoResponse(x.Id, x.PropiedadId, x.NombreArchivo, x.RutaArchivo, x.TipoContenido, x.CloudinaryPublicId, x.CloudinaryResourceType, x.CreatedAt)).ToArrayAsync(cancellationToken));
    }

    [HttpGet("documentos/{documentoId:guid}/descargar")]
    public async Task<ActionResult> DescargarDocumento(Guid documentoId, CancellationToken cancellationToken)
    {
        var doc = await dbContext.PropiedadDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == documentoId && x.DeletedAt == null, cancellationToken);
        return doc is null ? NotFound() : Ok(new { doc.NombreArchivo, doc.RutaArchivo, doc.TipoContenido });
    }

    [HttpDelete("documentos/{documentoId:guid}")]
    public async Task<IActionResult> DeleteDocumento(Guid documentoId, CancellationToken cancellationToken)
    {
        var doc = await dbContext.PropiedadDocumentos.FirstOrDefaultAsync(x => x.Id == documentoId && x.DeletedAt == null, cancellationToken);
        if (doc is null) return NotFound();
        var deleteResult = await DeleteCloudinaryFile(doc.CloudinaryPublicId, doc.CloudinaryResourceType, cancellationToken);
        if (deleteResult is not null) return deleteResult;
        doc.DeletedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = doc.PropiedadId, UserId = GetCurrentUserId(), Accion = "DOCUMENTO_ELIMINADO", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/fotografias")]
    public async Task<ActionResult<PropiedadFotografiaResponse>> AddFoto(Guid id, [FromForm] PropiedadFotografiaUploadRequest request, CancellationToken cancellationToken)
    {
        var fileValidation = ValidateFile(request.Archivo, ImageTypes, "Solo se permiten imágenes.");
        if (fileValidation is not null) return BadRequest(new { message = fileValidation });
        if (!await dbContext.Propiedades.AnyAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken)) return NotFound();

        CloudinaryUploadResult upload;
        try
        {
            upload = await storageService.UploadAsync(request.Archivo, $"propiedades/{id}/fotografias", CloudinaryResourceType.Image, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "No se pudo subir la fotografia a Cloudinary.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (request.EsPrincipal)
        {
            var actualesPrincipales = await dbContext.PropiedadImagenes
                .Where(x => x.PropiedadId == id && x.DeletedAt == null && x.EsPrincipal)
                .ToArrayAsync(cancellationToken);

            foreach (var actual in actualesPrincipales)
            {
                actual.EsPrincipal = false;
                actual.UpdatedAt = DateTime.Now;
            }

            if (actualesPrincipales.Length > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var foto = new PropiedadImagen
        {
            PropiedadId = id,
            NombreArchivo = request.Archivo.FileName,
            RutaArchivo = upload.Url,
            CloudinaryPublicId = upload.PublicId,
            CloudinaryResourceType = upload.ResourceType,
            Descripcion = request.Descripcion,
            EsPrincipal = request.EsPrincipal
        };
        dbContext.PropiedadImagenes.Add(foto);
        dbContext.Auditorias.Add(new Auditoria { RegistroId = id, UserId = GetCurrentUserId(), Accion = "FOTOGRAFIA_AGREGADA", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return CreatedAtAction(nameof(GetFotos), new { id }, new PropiedadFotografiaResponse(foto.Id, id, foto.NombreArchivo, foto.RutaArchivo, request.Archivo.ContentType, foto.Descripcion, foto.EsPrincipal, foto.CloudinaryPublicId, foto.CloudinaryResourceType, foto.CreatedAt));
    }

    [HttpGet("{id:guid}/fotografias")]
    public async Task<ActionResult<IReadOnlyCollection<PropiedadFotografiaResponse>>> GetFotos(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await dbContext.PropiedadImagenes.AsNoTracking().Where(x => x.PropiedadId == id && x.DeletedAt == null).Select(x => new PropiedadFotografiaResponse(x.Id, x.PropiedadId, x.NombreArchivo, x.RutaArchivo, null, x.Descripcion, x.EsPrincipal, x.CloudinaryPublicId, x.CloudinaryResourceType, x.CreatedAt)).ToArrayAsync(cancellationToken));
    }

    [HttpDelete("fotografias/{fotoId:guid}")]
    public async Task<IActionResult> DeleteFoto(Guid fotoId, CancellationToken cancellationToken)
    {
        var foto = await dbContext.PropiedadImagenes.FirstOrDefaultAsync(x => x.Id == fotoId && x.DeletedAt == null, cancellationToken);
        if (foto is null) return NotFound();
        var deleteResult = await DeleteCloudinaryFile(foto.CloudinaryPublicId, foto.CloudinaryResourceType, cancellationToken);
        if (deleteResult is not null) return deleteResult;
        foto.DeletedAt = DateTime.Now;
        foto.EsPrincipal = false;
        foto.UpdatedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria { RegistroId = foto.PropiedadId, UserId = GetCurrentUserId(), Accion = "FOTOGRAFIA_ELIMINADA", Entidad = "propiedades" });
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? ValidateFile(IFormFile file, string[] allowedTypes, string invalidTypeMessage)
    {
        if (file is null || file.Length == 0) return "Debe enviar un archivo.";
        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return invalidTypeMessage;
        }

        return null;
    }

    private async Task<IActionResult?> DeleteCloudinaryFile(string? publicId, string? resourceType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(publicId) || string.IsNullOrWhiteSpace(resourceType)) return null;

        try
        {
            await storageService.DeleteAsync(publicId, resourceType, cancellationToken);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "No se pudo eliminar el archivo en Cloudinary.", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<string?> ValidateReferences(Guid tipoId, Guid estadoId, CancellationToken ct)
    {
        if (!await dbContext.TiposPropiedad.AnyAsync(x => x.Id == tipoId && x.DeletedAt == null, ct)) return "El tipo de propiedad no existe.";
        if (!await dbContext.EstadosPropiedad.AnyAsync(x => x.Id == estadoId && x.DeletedAt == null, ct)) return "El estado de propiedad no existe.";
        return null;
    }

    private static string? ValidatePropertyValues(decimal latitud, decimal longitud, decimal metrosTerreno, decimal? metrosConstruccion, int? habitaciones, int? banos, decimal? precioVenta, decimal? precioAlquiler)
    {
        if (latitud is < -90 or > 90) return "La latitud debe estar entre -90 y 90.";
        if (longitud is < -180 or > 180) return "La longitud debe estar entre -180 y 180.";
        if (habitaciones.HasValue && habitaciones.Value < 0) return "Las habitaciones no pueden ser negativas.";
        if (banos.HasValue && banos.Value < 0) return "Los banos no pueden ser negativos.";
        if (precioVenta.HasValue && precioVenta.Value < 0) return "El precio de venta no puede ser negativo.";
        if (precioAlquiler.HasValue && precioAlquiler.Value < 0) return "El precio de alquiler no puede ser negativo.";
        if (metrosTerreno <= 0) return "Los metros cuadrados del terreno deben ser mayores que cero.";
        if (metrosConstruccion.HasValue && (metrosConstruccion.Value <= 0 || metrosConstruccion.Value > metrosTerreno))
        {
            return "Los metros cuadrados de construccion deben ser mayores que cero y no exceder el terreno.";
        }

        return null;
    }

    private async Task<string?> ValidateParticipacion(Guid propiedadId, Guid clienteId, decimal porcentaje, CancellationToken cancellationToken)
    {
        if (porcentaje <= 0 || porcentaje > 100) return "El porcentaje de participacion debe ser mayor que cero y menor o igual a 100.";

        var totalActual = await dbContext.PropiedadInversionistas
            .AsNoTracking()
            .Where(x => x.PropiedadId == propiedadId && x.ClienteId != clienteId && x.DeletedAt == null)
            .SumAsync(x => x.PorcentajeParticipacion, cancellationToken);

        return totalActual + porcentaje > 100
            ? "La suma de participacion de los inversionistas no puede exceder 100%."
            : null;
    }

    private async Task<PropiedadResponse> ToResponse(Guid id, CancellationToken ct)
    {
        var p = await dbContext.Propiedades.AsNoTracking().Include(x => x.TipoPropiedad).Include(x => x.EstadoPropiedad).FirstAsync(x => x.Id == id, ct);
        return new PropiedadResponse(p.Id, p.Codigo, p.TipoPropiedadId, p.TipoPropiedad.Nombre, p.EstadoPropiedadId, p.EstadoPropiedad.Nombre, p.Titulo, p.Descripcion, p.Latitud, p.Longitud, p.Direccion, p.MetrosCuadradosTerreno, p.MetrosCuadradosConstruccion, p.Habitaciones, p.Banos, p.PrecioVenta, p.PrecioAlquiler, p.CreatedAt, p.UpdatedAt);
    }

    private static PropiedadInversionistaResponse ToInversionistaResponse(PropiedadInversionista x)
    {
        var nombreCliente = !string.IsNullOrWhiteSpace(x.Cliente.RazonSocial)
            ? x.Cliente.RazonSocial
            : string.Join(' ', new[] { x.Cliente.Nombres, x.Cliente.Apellidos }.Where(v => !string.IsNullOrWhiteSpace(v)));

        return new PropiedadInversionistaResponse(
            x.PropiedadId,
            x.ClienteId,
            nombreCliente,
            x.PorcentajeParticipacion,
            x.Observaciones,
            x.CreatedAt,
            x.UpdatedAt);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
