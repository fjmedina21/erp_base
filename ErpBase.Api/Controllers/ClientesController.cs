using System.Security.Claims;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Clientes;
using ErpBase.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clientes")]
public sealed class ClientesController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create(CreateClienteRequest request, CancellationToken cancellationToken)
    {
        var tipoCliente = await FindTipoCliente(request.TipoCliente, cancellationToken);
        if (tipoCliente is null)
        {
            return BadRequest(new { message = "El tipo de cliente no existe en crm.tipos_cliente." });
        }

        var tipoDocumento = await FindTipoDocumento(request.TipoDocumento, cancellationToken);
        if (tipoDocumento is null)
        {
            return BadRequest(new { message = "El tipo de documento no existe en crm.tipos_documento_identidad." });
        }

        var validationError = ValidateCliente(
            tipoCliente.Nombre,
            tipoDocumento.Nombre,
            request.Documento,
            request.Nombres,
            request.Apellidos,
            request.RazonSocial);

        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var documento = NormalizeDocumento(request.Documento);
        var exists = await DocumentoExists(tipoDocumento.Id, documento, excludedClienteId: null, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Ya existe un cliente con este tipo de documento y documento." });
        }

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            TipoClienteId = tipoCliente.Id,
            TipoCliente = tipoCliente,
            TipoDocumentoIdentidadId = tipoDocumento.Id,
            TipoDocumentoIdentidad = tipoDocumento,
            DocumentoIdentidad = documento,
            Nombres = NormalizeOptional(request.Nombres),
            Apellidos = NormalizeOptional(request.Apellidos),
            RazonSocial = NormalizeOptional(request.RazonSocial),
            Telefono = NormalizeOptional(request.Telefono),
            Email = NormalizeEmail(request.Email),
            Direccion = NormalizeOptional(request.Direccion)
        };

        dbContext.Clientes.Add(cliente);
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = cliente.Id,
            UserId = GetCurrentUserId(),
            Accion = "CREACION",
            Entidad = "clientes"
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(cliente);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> Update(Guid id, UpdateClienteRequest request, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes
            .Include(x => x.TipoCliente)
            .Include(x => x.TipoDocumentoIdentidad)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        var tipoCliente = request.TipoCliente is null
            ? cliente.TipoCliente
            : await FindTipoCliente(request.TipoCliente, cancellationToken);

        if (tipoCliente is null)
        {
            return BadRequest(new { message = "El tipo de cliente no existe en crm.tipos_cliente." });
        }

        var tipoDocumento = request.TipoDocumento is null
            ? cliente.TipoDocumentoIdentidad
            : await FindTipoDocumento(request.TipoDocumento, cancellationToken);

        if (tipoDocumento is null)
        {
            return BadRequest(new { message = "El tipo de documento no existe en crm.tipos_documento_identidad." });
        }

        var documento = request.Documento is null ? cliente.DocumentoIdentidad : NormalizeDocumento(request.Documento);
        var nombres = request.Nombres is null ? cliente.Nombres : NormalizeOptional(request.Nombres);
        var apellidos = request.Apellidos is null ? cliente.Apellidos : NormalizeOptional(request.Apellidos);
        var razonSocial = request.RazonSocial is null ? cliente.RazonSocial : NormalizeOptional(request.RazonSocial);

        var validationError = ValidateCliente(tipoCliente.Nombre, tipoDocumento.Nombre, documento, nombres, apellidos, razonSocial);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var exists = await DocumentoExists(tipoDocumento.Id, documento, id, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Ya existe otro cliente con este tipo de documento y documento." });
        }

        cliente.TipoClienteId = tipoCliente.Id;
        cliente.TipoCliente = tipoCliente;
        cliente.TipoDocumentoIdentidadId = tipoDocumento.Id;
        cliente.TipoDocumentoIdentidad = tipoDocumento;
        cliente.DocumentoIdentidad = documento;
        cliente.Nombres = nombres;
        cliente.Apellidos = apellidos;
        cliente.RazonSocial = razonSocial;
        cliente.Telefono = request.Telefono is null ? cliente.Telefono : NormalizeOptional(request.Telefono);
        cliente.Email = request.Email is null ? cliente.Email : NormalizeEmail(request.Email);
        cliente.Direccion = request.Direccion is null ? cliente.Direccion : NormalizeOptional(request.Direccion);
        cliente.UpdatedAt = DateTime.Now;

        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = cliente.Id,
            UserId = GetCurrentUserId(),
            Accion = "ACTUALIZACION",
            Entidad = "clientes"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(cliente));
    }

    [HttpGet]
    public async Task<ActionResult<ClienteSearchResponse>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? nombre,
        [FromQuery] string? documento,
        [FromQuery] string? tipoCliente,
        [FromQuery] string? orderBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.Clientes
            .AsNoTracking()
            .Include(x => x.TipoCliente)
            .Include(x => x.TipoDocumentoIdentidad)
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.Nombres != null && x.Nombres.ToLower().Contains(term)) ||
                (x.Apellidos != null && x.Apellidos.ToLower().Contains(term)) ||
                (x.RazonSocial != null && x.RazonSocial.ToLower().Contains(term)) ||
                x.DocumentoIdentidad.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            var term = nombre.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.Nombres != null && x.Nombres.ToLower().Contains(term)) ||
                (x.Apellidos != null && x.Apellidos.ToLower().Contains(term)) ||
                (x.RazonSocial != null && x.RazonSocial.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(documento))
        {
            var normalizedDocumento = NormalizeDocumento(documento);
            query = query.Where(x => x.DocumentoIdentidad.Contains(normalizedDocumento));
        }

        if (!string.IsNullOrWhiteSpace(tipoCliente))
        {
            var tipoClienteCatalogo = await FindTipoCliente(tipoCliente, cancellationToken);
            if (tipoClienteCatalogo is null)
            {
                return Ok(new ClienteSearchResponse([], page, pageSize, 0));
            }

            query = query.Where(x => x.TipoClienteId == tipoClienteCatalogo.Id);
        }

        query = string.Equals(orderBy, "updated_at", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            : query.OrderByDescending(x => x.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var clientes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        var items = clientes.Select(ToResponse).ToArray();

        return Ok(new ClienteSearchResponse(items, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClienteResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .Include(x => x.TipoCliente)
            .Include(x => x.TipoDocumentoIdentidad)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        return cliente is null ? NotFound() : Ok(ToResponse(cliente));
    }

    [HttpGet("{id:guid}/historial")]
    public async Task<ActionResult<ClienteHistorialResponse>> GetHistorial(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes
            .AsNoTracking()
            .Include(x => x.TipoCliente)
            .Include(x => x.TipoDocumentoIdentidad)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        var historial = await dbContext.Auditorias
            .AsNoTracking()
            .Where(x => x.Entidad == "clientes" && x.RegistroId == id)
            .OrderByDescending(x => x.Fecha)
            .Select(x => new ClienteHistorialItemResponse(
                x.Id,
                x.Accion,
                x.UserId,
                x.Entidad,
                x.RegistroId,
                x.Fecha))
            .ToArrayAsync(cancellationToken);

        var observaciones = Array.Empty<string>();

        var contratos = await dbContext.Contratos
            .AsNoTracking()
            .Where(x => x.ClienteId == id && x.DeletedAt == null)
            .Select(x => new { x.Id, x.Codigo, x.PropiedadId, x.FechaInicio, x.FechaFin, x.RutaArchivo, x.Observaciones })
            .ToArrayAsync(cancellationToken);

        var propiedadIds = contratos.Select(x => x.PropiedadId).Distinct().ToArray();
        var propiedades = await dbContext.Propiedades
            .AsNoTracking()
            .Where(x => propiedadIds.Contains(x.Id) && x.DeletedAt == null)
            .Select(x => new { x.Id, x.Codigo, x.Titulo, x.Direccion })
            .ToArrayAsync(cancellationToken);

        return Ok(new ClienteHistorialResponse(
            ToResponse(cliente),
            Propiedades: propiedades,
            Contratos: contratos,
            Pagos: Array.Empty<object>(),
            Observaciones: observaciones,
            HistorialModificaciones: historial));
    }

    private async Task<bool> DocumentoExists(
        Guid tipoDocumentoIdentidadId,
        string documento,
        Guid? excludedClienteId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Clientes.AnyAsync(x =>
            x.TipoDocumentoIdentidadId == tipoDocumentoIdentidadId &&
            x.DocumentoIdentidad == documento &&
            x.DeletedAt == null &&
            (!excludedClienteId.HasValue || x.Id != excludedClienteId.Value),
            cancellationToken);
    }

    private async Task<TipoCliente?> FindTipoCliente(string nombre, CancellationToken cancellationToken)
    {
        var normalized = NormalizeCatalogName(nombre);
        var aliases = normalized switch
        {
            "PERSONAFISICA" or "FISICA" => new[] { "PERSONAFISICA", "PERSONA" },
            "PERSONAJURIDICA" or "JURIDICA" or "EMPRESA" => new[] { "PERSONAJURIDICA", "EMPRESA" },
            _ => new[] { normalized }
        };

        var tiposCliente = await dbContext.TiposCliente
            .Where(x => x.DeletedAt == null)
            .ToArrayAsync(cancellationToken);

        return tiposCliente.FirstOrDefault(x => aliases.Contains(NormalizeCatalogName(x.Nombre)));
    }

    private async Task<TipoDocumentoIdentidad?> FindTipoDocumento(string nombre, CancellationToken cancellationToken)
    {
        var normalized = NormalizeCatalogName(nombre);
        var tiposDocumento = await dbContext.TiposDocumentoIdentidad
            .Where(x => x.DeletedAt == null)
            .ToArrayAsync(cancellationToken);

        return tiposDocumento.FirstOrDefault(x => NormalizeCatalogName(x.Nombre) == normalized);
    }

    private static string? ValidateCliente(
        string tipoCliente,
        string tipoDocumento,
        string? documento,
        string? nombres,
        string? apellidos,
        string? razonSocial)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            return "El documento es requerido.";
        }

        var normalizedTipoCliente = NormalizeCatalogName(tipoCliente);
        var normalizedTipoDocumento = NormalizeCatalogName(tipoDocumento);
        var isPersonaFisica = normalizedTipoCliente is "PERSONA" or "PERSONAFISICA" or "FISICA";
        var isPersonaJuridica = normalizedTipoCliente is "EMPRESA" or "PERSONAJURIDICA" or "JURIDICA";

        if (isPersonaFisica)
        {
            if (normalizedTipoDocumento is not ("CEDULA" or "PASAPORTE"))
            {
                return "Para persona física, el documento debe ser cédula o pasaporte.";
            }

            if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
            {
                return "Para persona física, nombres y apellidos son requeridos.";
            }

            if (!string.IsNullOrWhiteSpace(razonSocial))
            {
                return "Una persona física no debe tener razón social.";
            }
        }

        if (isPersonaJuridica)
        {
            if (normalizedTipoDocumento != "RNC")
            {
                return "Para persona jurídica, el documento debe ser RNC.";
            }

            if (string.IsNullOrWhiteSpace(razonSocial))
            {
                return "Para persona jurídica, la razón social es requerida.";
            }

            if (!string.IsNullOrWhiteSpace(nombres) || !string.IsNullOrWhiteSpace(apellidos))
            {
                return "Una persona jurídica no debe tener nombres o apellidos de persona física.";
            }
        }

        return null;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }

    private static ClienteResponse ToResponse(Cliente cliente)
    {
        return new ClienteResponse(
            cliente.Id,
            cliente.TipoClienteId,
            cliente.TipoCliente.Nombre,
            cliente.TipoDocumentoIdentidadId,
            cliente.TipoDocumentoIdentidad.Nombre,
            cliente.DocumentoIdentidad,
            cliente.Nombres,
            cliente.Apellidos,
            cliente.RazonSocial,
            GetNombreCompleto(cliente),
            cliente.Email,
            cliente.Telefono,
            cliente.Direccion,
            cliente.CreatedAt,
            cliente.UpdatedAt);
    }

    private static string GetNombreCompleto(Cliente cliente)
    {
        if (!string.IsNullOrWhiteSpace(cliente.RazonSocial))
        {
            return cliente.RazonSocial;
        }

        return string.Join(' ', new[] { cliente.Nombres, cliente.Apellidos }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string NormalizeDocumento(string documento)
    {
        return documento.Trim().ToUpperInvariant();
    }

    private static string NormalizeCatalogName(string value)
    {
        return value.Trim().Replace("_", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
