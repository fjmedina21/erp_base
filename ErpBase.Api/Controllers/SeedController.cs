using ErpBase.Api.Data;
using ErpBase.Api.Entities;
using ErpBase.Api.Services.AuthService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("seed")]
public sealed class SeedController(AppDbContext dbContext, IPasswordHasher passwordHasher, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SeedResponse>> Seed(CancellationToken cancellationToken)
    {
        var result = new SeedResult();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        result.TiposCliente = await SeedSimpleCatalog(dbContext.TiposCliente, ["Persona", "Empresa"], cancellationToken);
        result.TiposPropiedad = await SeedTiposPropiedad(cancellationToken);
        result.EstadosPropiedad = await SeedSimpleCatalog(dbContext.EstadosPropiedad, ["Disponible", "Reservada", "Vendida", "Alquilada"], cancellationToken);
        result.TiposContrato = await SeedSimpleCatalog(dbContext.TiposContrato, ["Venta", "Alquiler", "Arrendamiento"], cancellationToken);
        result.EstadosContrato = await SeedSimpleCatalog(dbContext.EstadosContrato, ["Activo", "Vencido", "Cancelado"], cancellationToken);
        result.TiposDocumentoIdentidad = await SeedTiposDocumentoIdentidad(cancellationToken);
        result.Roles = await SeedRoles(cancellationToken);
        result.Permisos = await SeedPermisos(cancellationToken);
        result.RolesPermisos = await SeedRolesPermisos(cancellationToken);
        var adminUserSeed = await SeedAdminUser(cancellationToken);
        result.Usuarios = adminUserSeed.UserInserted;
        result.UsuarioRoles = adminUserSeed.UserRoleInserted;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new SeedResponse(
            "Seed ejecutado correctamente.",
            result.TiposCliente,
            result.TiposDocumentoIdentidad,
            result.TiposPropiedad,
            result.EstadosPropiedad,
            result.TiposContrato,
            result.EstadosContrato,
            result.Roles,
            result.Permisos,
            result.RolesPermisos,
            result.Usuarios,
            result.UsuarioRoles));
    }

    private async Task<int> SeedSimpleCatalog<TEntity>(DbSet<TEntity> dbSet, IReadOnlyCollection<string> names, CancellationToken cancellationToken)
        where TEntity : class, INamedCatalog, new()
    {
        var current = await dbSet.Where(x => names.Contains(x.Nombre)).ToArrayAsync(cancellationToken);
        var inserted = 0;

        foreach (var name in names)
        {
            var existing = current.FirstOrDefault(x => string.Equals(x.Nombre, name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.DeletedAt = null;
                continue;
            }

            dbSet.Add(new TEntity { Nombre = name });
            inserted++;
        }

        return inserted;
    }

    private async Task<int> SeedTiposDocumentoIdentidad(CancellationToken cancellationToken)
    {
        var seed = new[]
        {
            new TipoDocumentoIdentidad { Nombre = "Cedula", RegexValidacion = "^\\d{11}$" },
            new TipoDocumentoIdentidad { Nombre = "RNC", RegexValidacion = "^\\d{9}$" },
            new TipoDocumentoIdentidad { Nombre = "Pasaporte", RegexValidacion = "^[A-Z0-9]{6,20}$" }
        };

        var names = seed.Select(x => x.Nombre).ToArray();
        var current = await dbContext.TiposDocumentoIdentidad.Where(x => names.Contains(x.Nombre)).ToArrayAsync(cancellationToken);
        var inserted = 0;

        foreach (var item in seed)
        {
            var existing = current.FirstOrDefault(x => string.Equals(x.Nombre, item.Nombre, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.RegexValidacion = item.RegexValidacion;
                existing.DeletedAt = null;
                continue;
            }

            dbContext.TiposDocumentoIdentidad.Add(item);
            inserted++;
        }

        return inserted;
    }

    private async Task<int> SeedTiposPropiedad(CancellationToken cancellationToken)
    {
        var seed = new[]
        {
            new TipoPropiedad { Nombre = "Apartamento", Codigo = "APT" },
            new TipoPropiedad { Nombre = "Casa", Codigo = "CAS" },
            new TipoPropiedad { Nombre = "Solar", Codigo = "SOL" },
            new TipoPropiedad { Nombre = "Local Comercial", Codigo = "LOC" },
            new TipoPropiedad { Nombre = "Oficina", Codigo = "OFI" }
        };

        var names = seed.Select(x => x.Nombre).ToArray();
        var current = await dbContext.TiposPropiedad.Where(x => names.Contains(x.Nombre)).ToArrayAsync(cancellationToken);
        var inserted = 0;

        foreach (var item in seed)
        {
            var existing = current.FirstOrDefault(x => string.Equals(x.Nombre, item.Nombre, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Codigo = item.Codigo;
                existing.DeletedAt = null;
                continue;
            }

            dbContext.TiposPropiedad.Add(item);
            inserted++;
        }

        return inserted;
    }

    private async Task<int> SeedRoles(CancellationToken cancellationToken)
    {
        var names = new[] { "Administrador", "Gerente", "Arquitecto", "Agente Inmobiliario" };
        var current = await dbContext.Roles.Where(x => names.Contains(x.Nombre)).ToArrayAsync(cancellationToken);
        var inserted = 0;

        foreach (var name in names)
        {
            var existing = current.FirstOrDefault(x => string.Equals(x.Nombre, name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.DeletedAt = null;
                continue;
            }

            dbContext.Roles.Add(new Role { Nombre = name });
            inserted++;
        }

        return inserted;
    }

    private async Task<int> SeedPermisos(CancellationToken cancellationToken)
    {
        var seed = GetPermisos();
        var codes = seed.Select(x => x.Codigo).ToArray();
        var current = await dbContext.Permissions.Where(x => codes.Contains(x.Codigo)).ToArrayAsync(cancellationToken);
        var inserted = 0;

        foreach (var item in seed)
        {
            var existing = current.FirstOrDefault(x => string.Equals(x.Codigo, item.Codigo, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                existing.Nombre = item.Nombre;
                existing.Descripcion = item.Descripcion;
                existing.DeletedAt = null;
                continue;
            }

            dbContext.Permissions.Add(new Permission
            {
                Codigo = item.Codigo,
                Nombre = item.Nombre,
                Descripcion = item.Descripcion
            });
            inserted++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return inserted;
    }

    private async Task<int> SeedRolesPermisos(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = await dbContext.Roles.Where(x => x.DeletedAt == null).ToDictionaryAsync(x => x.Nombre, x => x.Id, cancellationToken);
        var permissions = await dbContext.Permissions.Where(x => x.DeletedAt == null).ToDictionaryAsync(x => x.Codigo, x => x.Id, cancellationToken);
        var current = await dbContext.RolePermissions.ToArrayAsync(cancellationToken);
        var inserted = 0;

        foreach (var assignment in GetRolePermissionAssignments(permissions.Keys))
        {
            if (!roles.TryGetValue(assignment.RoleName, out var roleId)) continue;

            foreach (var code in assignment.PermissionCodes)
            {
                if (!permissions.TryGetValue(code, out var permissionId)) continue;
                if (current.Any(x => x.RoleId == roleId && x.PermissionId == permissionId)) continue;

                dbContext.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
                inserted++;
            }
        }

        return inserted;
    }

    private async Task<AdminUserSeedResult> SeedAdminUser(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);

        var email = configuration["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Configure Seed:AdminEmail y Seed:AdminPassword para crear el usuario administrador.");
        }

        var userInserted = 0;
        var userRoleInserted = 0;

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null)
        {
            user = new User
            {
                Email = email,
                PasswordHash = passwordHasher.Hash(password),
                Activo = true,
                Bloqueado = false
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
            userInserted = 1;
        }
        else
        {
            user.Activo = true;
            user.Bloqueado = false;
            user.MotivoBloqueo = null;
            user.BloqueadoAt = null;
            user.ReactivadoAt = DateTime.Now;
            user.DeletedAt = null;
            user.UpdatedAt = DateTime.Now;
        }

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(x => x.Nombre == "Administrador" && x.DeletedAt == null, cancellationToken);
        if (adminRole is not null)
        {
            var exists = await dbContext.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == adminRole.Id, cancellationToken);
            if (!exists)
            {
                dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
                userRoleInserted = 1;
            }
        }

        return new AdminUserSeedResult(userInserted, userRoleInserted);
    }

    private static IReadOnlyCollection<PermissionSeed> GetPermisos() =>
    [
        new("usuarios.ver", "Ver usuarios", "Permite consultar usuarios"),
        new("usuarios.crear", "Crear usuarios", "Permite registrar usuarios"),
        new("usuarios.editar", "Editar usuarios", "Permite modificar usuarios"),
        new("usuarios.eliminar", "Eliminar usuarios", "Permite eliminar usuarios"),
        new("roles.ver", "Ver roles", "Permite consultar roles"),
        new("roles.crear", "Crear roles", "Permite registrar roles"),
        new("roles.editar", "Editar roles", "Permite modificar roles"),
        new("roles.eliminar", "Eliminar roles", "Permite eliminar roles"),
        new("permisos.ver", "Ver permisos", "Permite consultar permisos"),
        new("permisos.asignar", "Asignar permisos", "Permite asignar permisos a roles"),
        new("empleados.ver", "Ver empleados", "Permite consultar empleados"),
        new("empleados.crear", "Crear empleados", "Permite registrar empleados"),
        new("empleados.editar", "Editar empleados", "Permite modificar empleados"),
        new("empleados.eliminar", "Eliminar empleados", "Permite eliminar empleados"),
        new("departamentos.ver", "Ver departamentos", "Permite consultar departamentos"),
        new("departamentos.crear", "Crear departamentos", "Permite registrar departamentos"),
        new("departamentos.editar", "Editar departamentos", "Permite modificar departamentos"),
        new("departamentos.eliminar", "Eliminar departamentos", "Permite eliminar departamentos"),
        new("cargos.ver", "Ver cargos", "Permite consultar cargos"),
        new("cargos.crear", "Crear cargos", "Permite registrar cargos"),
        new("cargos.editar", "Editar cargos", "Permite modificar cargos"),
        new("cargos.eliminar", "Eliminar cargos", "Permite eliminar cargos"),
        new("clientes.ver", "Ver clientes", "Permite consultar clientes"),
        new("clientes.crear", "Crear clientes", "Permite registrar clientes"),
        new("clientes.editar", "Editar clientes", "Permite modificar clientes"),
        new("clientes.eliminar", "Eliminar clientes", "Permite eliminar clientes"),
        new("tipos_cliente.ver", "Ver tipos de cliente", "Permite consultar tipos de cliente"),
        new("tipos_documento.ver", "Ver tipos de documento", "Permite consultar tipos de documento"),
        new("propiedades.ver", "Ver propiedades", "Permite consultar propiedades"),
        new("propiedades.crear", "Crear propiedades", "Permite registrar propiedades"),
        new("propiedades.editar", "Editar propiedades", "Permite modificar propiedades"),
        new("propiedades.eliminar", "Eliminar propiedades", "Permite eliminar propiedades"),
        new("propiedades.publicar", "Publicar propiedades", "Permite publicar propiedades"),
        new("propiedades.cambiar_estado", "Cambiar estado de propiedades", "Permite cambiar el estado de una propiedad"),
        new("tipos_propiedad.ver", "Ver tipos de propiedad", "Permite consultar tipos de propiedad"),
        new("estados_propiedad.ver", "Ver estados de propiedad", "Permite consultar estados de propiedad"),
        new("imagenes_propiedad.ver", "Ver imágenes de propiedad", "Permite consultar imágenes"),
        new("imagenes_propiedad.crear", "Crear imágenes de propiedad", "Permite agregar imágenes"),
        new("imagenes_propiedad.eliminar", "Eliminar imágenes de propiedad", "Permite eliminar imágenes"),
        new("contratos.ver", "Ver contratos", "Permite consultar contratos"),
        new("contratos.crear", "Crear contratos", "Permite registrar contratos"),
        new("contratos.editar", "Editar contratos", "Permite modificar contratos"),
        new("contratos.eliminar", "Eliminar contratos", "Permite eliminar contratos"),
        new("contratos.aprobar", "Aprobar contratos", "Permite aprobar contratos"),
        new("contratos.cancelar", "Cancelar contratos", "Permite cancelar contratos"),
        new("tipos_contrato.ver", "Ver tipos de contrato", "Permite consultar tipos de contrato"),
        new("estados_contrato.ver", "Ver estados de contrato", "Permite consultar estados de contrato"),
        new("proyectos.ver", "Ver proyectos", "Permite consultar proyectos"),
        new("proyectos.crear", "Crear proyectos", "Permite registrar proyectos"),
        new("proyectos.editar", "Editar proyectos", "Permite modificar proyectos"),
        new("proyectos.eliminar", "Eliminar proyectos", "Permite eliminar proyectos"),
        new("proyectos.cambiar_estado", "Cambiar estado de proyectos", "Permite cambiar estado de proyectos"),
        new("proyectos.ver_avance", "Ver avance de proyectos", "Permite consultar avances"),
        new("estados_proyecto.ver", "Ver estados de proyecto", "Permite consultar estados"),
        new("fases.ver", "Ver fases", "Permite consultar fases"),
        new("fases.crear", "Crear fases", "Permite registrar fases"),
        new("fases.editar", "Editar fases", "Permite modificar fases"),
        new("fases.eliminar", "Eliminar fases", "Permite eliminar fases"),
        new("fases.cambiar_estado", "Cambiar estado de fases", "Permite cambiar estado de fases"),
        new("estados_fase.ver", "Ver estados de fase", "Permite consultar estados"),
        new("tareas.ver", "Ver tareas", "Permite consultar tareas"),
        new("tareas.crear", "Crear tareas", "Permite registrar tareas"),
        new("tareas.editar", "Editar tareas", "Permite modificar tareas"),
        new("tareas.eliminar", "Eliminar tareas", "Permite eliminar tareas"),
        new("tareas.asignar_responsables", "Asignar responsables", "Permite asignar responsables"),
        new("tareas.cambiar_estado", "Cambiar estado de tareas", "Permite cambiar estado"),
        new("tareas.actualizar_avance", "Actualizar avance", "Permite actualizar porcentaje de avance"),
        new("prioridades_tarea.ver", "Ver prioridades", "Permite consultar prioridades"),
        new("estados_tarea.ver", "Ver estados de tarea", "Permite consultar estados"),
        new("auditoria.ver", "Ver auditoría", "Permite consultar registros de auditoría")
    ];

    private static IReadOnlyCollection<RolePermissionSeed> GetRolePermissionAssignments(IEnumerable<string> allPermissionCodes)
    {
        string[] gerente =
        [
            "empleados.ver", "clientes.ver", "clientes.crear", "clientes.editar",
            "propiedades.ver", "propiedades.crear", "propiedades.editar", "propiedades.publicar", "propiedades.cambiar_estado",
            "contratos.ver", "contratos.crear", "contratos.editar", "contratos.aprobar", "contratos.cancelar",
            "proyectos.ver", "proyectos.crear", "proyectos.editar", "proyectos.cambiar_estado", "proyectos.ver_avance",
            "fases.ver", "tareas.ver", "auditoria.ver"
        ];

        string[] arquitecto =
        [
            "clientes.ver", "propiedades.ver",
            "proyectos.ver", "proyectos.crear", "proyectos.editar", "proyectos.ver_avance",
            "fases.ver", "fases.crear", "fases.editar", "fases.cambiar_estado",
            "tareas.ver", "tareas.crear", "tareas.editar", "tareas.asignar_responsables", "tareas.cambiar_estado", "tareas.actualizar_avance",
            "estados_proyecto.ver", "estados_fase.ver", "estados_tarea.ver", "prioridades_tarea.ver"
        ];

        string[] agente =
        [
            "clientes.ver", "clientes.crear", "clientes.editar",
            "propiedades.ver", "propiedades.crear", "propiedades.editar", "propiedades.publicar", "propiedades.cambiar_estado",
            "imagenes_propiedad.ver", "imagenes_propiedad.crear", "imagenes_propiedad.eliminar",
            "contratos.ver", "contratos.crear", "contratos.editar",
            "tipos_propiedad.ver", "estados_propiedad.ver", "tipos_contrato.ver", "estados_contrato.ver"
        ];

        return
        [
            new("Administrador", allPermissionCodes.ToArray()),
            new("Gerente", gerente),
            new("Arquitecto", arquitecto),
            new("Agente Inmobiliario", agente)
        ];
    }

    private sealed record PermissionSeed(string Codigo, string Nombre, string Descripcion);
    private sealed record RolePermissionSeed(string RoleName, IReadOnlyCollection<string> PermissionCodes);
    private sealed record AdminUserSeedResult(int UserInserted, int UserRoleInserted);
    private sealed record SeedResult
    {
        public int TiposCliente { get; set; }
        public int TiposDocumentoIdentidad { get; set; }
        public int TiposPropiedad { get; set; }
        public int EstadosPropiedad { get; set; }
        public int TiposContrato { get; set; }
        public int EstadosContrato { get; set; }
        public int Roles { get; set; }
        public int Permisos { get; set; }
        public int RolesPermisos { get; set; }
        public int Usuarios { get; set; }
        public int UsuarioRoles { get; set; }
    }
}

public sealed record SeedResponse(
    string Message,
    int TiposCliente,
    int TiposDocumentoIdentidad,
    int TiposPropiedad,
    int EstadosPropiedad,
    int TiposContrato,
    int EstadosContrato,
    int Roles,
    int Permisos,
    int RolesPermisos,
    int Usuarios,
    int UsuarioRoles);
