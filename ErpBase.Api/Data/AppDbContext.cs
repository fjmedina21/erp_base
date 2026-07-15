using ErpBase.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ErpBase.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<TipoCliente> TiposCliente => Set<TipoCliente>();
    public DbSet<TipoDocumentoIdentidad> TiposDocumentoIdentidad => Set<TipoDocumentoIdentidad>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<TipoPropiedad> TiposPropiedad => Set<TipoPropiedad>();
    public DbSet<EstadoPropiedad> EstadosPropiedad => Set<EstadoPropiedad>();
    public DbSet<Propiedad> Propiedades => Set<Propiedad>();
    public DbSet<PropiedadImagen> PropiedadImagenes => Set<PropiedadImagen>();
    public DbSet<PropiedadDocumento> PropiedadDocumentos => Set<PropiedadDocumento>();
    public DbSet<PropiedadInversionista> PropiedadInversionistas => Set<PropiedadInversionista>();
    public DbSet<TipoContrato> TiposContrato => Set<TipoContrato>();
    public DbSet<EstadoContrato> EstadosContrato => Set<EstadoContrato>();
    public DbSet<Contrato> Contratos => Set<Contrato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("usuarios","seguridad");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true).IsRequired();
            entity.Property(x => x.Bloqueado).HasColumnName("bloqueado").HasDefaultValue(false).IsRequired();
            entity.Property(x => x.MotivoBloqueo).HasColumnName("motivo_bloqueo").HasMaxLength(500);
            entity.Property(x => x.BloqueadoAt).HasColumnName("bloqueado_at").HasColumnType("timestamp");
            entity.Property(x => x.ReactivadoAt).HasColumnName("reactivado_at").HasColumnType("timestamp");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles","seguridad");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Descripcion).HasColumnName("descripcion");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permisos","seguridad");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Descripcion).HasColumnName("descripcion");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Codigo).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("usuario_roles","seguridad");
            entity.HasKey(x => new { x.UserId, x.RoleId });
            entity.Property(x => x.UserId).HasColumnName("usuario_id");
            entity.Property(x => x.RoleId).HasColumnName("rol_id");
            entity.HasOne<User>().WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("roles_permisos","seguridad");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });
            entity.Property(x => x.RoleId).HasColumnName("rol_id").HasColumnOrder(0);
            entity.Property(x => x.PermissionId).HasColumnName("permiso_id").HasColumnOrder(1);
            entity.HasOne<Role>().WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Permission).WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("password_reset_tokens", "seguridad");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.UserId).HasColumnName("usuario_id").IsRequired();
            entity.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamp").IsRequired();
            entity.Property(x => x.UsedAt).HasColumnName("used_at").HasColumnType("timestamp");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TipoCliente>(entity =>
        {
            entity.ToTable("tipos_cliente", "crm");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<TipoDocumentoIdentidad>(entity =>
        {
            entity.ToTable("tipos_documento_identidad", "crm");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(20).IsRequired();
            entity.Property(x => x.RegexValidacion).HasColumnName("regex_validacion").HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes", "crm");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.TipoClienteId).HasColumnName("tipo_cliente_id").IsRequired();
            entity.Property(x => x.TipoDocumentoIdentidadId).HasColumnName("tipo_documento_identidad_id").IsRequired();
            entity.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(150);
            entity.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(150);
            entity.Property(x => x.RazonSocial).HasColumnName("razon_social").HasMaxLength(250);
            entity.Property(x => x.DocumentoIdentidad).HasColumnName("documento_identidad").HasMaxLength(20).IsRequired();
            entity.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(50);
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(x => x.Direccion).HasColumnName("direccion");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => new { x.TipoDocumentoIdentidadId, x.DocumentoIdentidad }).IsUnique();
            entity.HasOne(x => x.TipoCliente).WithMany().HasForeignKey(x => x.TipoClienteId);
            entity.HasOne(x => x.TipoDocumentoIdentidad).WithMany().HasForeignKey(x => x.TipoDocumentoIdentidadId);
            entity.ToTable(t => t.HasCheckConstraint(
                "chk_cliente_nombre_empresa",
                "(razon_social IS NOT NULL AND nombres IS NULL AND apellidos IS NULL) OR (razon_social IS NULL AND nombres IS NOT NULL AND apellidos IS NOT NULL)"));
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.ToTable("auditorias", "auditoria");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.UserId).HasColumnName("usuario_id");
            entity.Property(x => x.Accion).HasColumnName("accion").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Entidad).HasColumnName("entidad").HasMaxLength(100).IsRequired();
            entity.Property(x => x.RegistroId).HasColumnName("registro_id");
            entity.Property(x => x.Fecha).HasColumnName("fecha").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.Entidad, x.RegistroId });
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TipoPropiedad>(entity =>
        {
            entity.ToTable("tipos_propiedad", "propiedades");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
            entity.HasIndex(x => x.Codigo).IsUnique();
        });

        modelBuilder.Entity<EstadoPropiedad>(entity =>
        {
            entity.ToTable("estados_propiedad", "propiedades");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Propiedad>(entity =>
        {
            entity.ToTable("propiedades", "propiedades");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
            entity.Property(x => x.TipoPropiedadId).HasColumnName("tipo_propiedad_id").IsRequired();
            entity.Property(x => x.EstadoPropiedadId).HasColumnName("estado_propiedad_id").IsRequired();
            entity.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Descripcion).HasColumnName("descripcion");
            entity.Property(x => x.Latitud).HasColumnName("latitud").HasPrecision(10, 8).IsRequired();
            entity.Property(x => x.Longitud).HasColumnName("longitud").HasPrecision(11, 8).IsRequired();
            entity.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.MetrosCuadradosTerreno).HasColumnName("metros_cuadrados_terreno").HasPrecision(10, 2).IsRequired();
            entity.Property(x => x.MetrosCuadradosConstruccion).HasColumnName("metros_cuadrados_construccion").HasPrecision(10, 2);
            entity.Property(x => x.Habitaciones).HasColumnName("habitaciones");
            entity.Property(x => x.Banos).HasColumnName("banos");
            entity.Property(x => x.PrecioVenta).HasColumnName("precio_venta").HasPrecision(14, 2);
            entity.Property(x => x.PrecioAlquiler).HasColumnName("precio_alquiler").HasPrecision(14, 2);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.HasOne(x => x.TipoPropiedad).WithMany().HasForeignKey(x => x.TipoPropiedadId);
            entity.HasOne(x => x.EstadoPropiedad).WithMany().HasForeignKey(x => x.EstadoPropiedadId);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("chk_latitud", "latitud BETWEEN -90 AND 90");
                t.HasCheckConstraint("chk_longitud", "longitud BETWEEN -180 AND 180");
                t.HasCheckConstraint("chk_habitaciones", "habitaciones IS NULL OR habitaciones >= 0");
                t.HasCheckConstraint("chk_banos", "banos IS NULL OR banos >= 0");
                t.HasCheckConstraint("chk_metros_cuadrados_terreno", "metros_cuadrados_terreno > 0");
                t.HasCheckConstraint("chk_area_construccion", "metros_cuadrados_construccion IS NULL OR (metros_cuadrados_construccion > 0 AND metros_cuadrados_construccion <= metros_cuadrados_terreno)");
            });
        });

        modelBuilder.Entity<PropiedadImagen>(entity =>
        {
            entity.ToTable("propiedad_imagenes", "propiedades");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.PropiedadId).HasColumnName("propiedad_id").IsRequired();
            entity.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255).IsRequired();
            entity.Property(x => x.RutaArchivo).HasColumnName("ruta_archivo").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.CloudinaryPublicId).HasColumnName("cloudinary_public_id").HasMaxLength(255);
            entity.Property(x => x.CloudinaryResourceType).HasColumnName("cloudinary_resource_type").HasMaxLength(50);
            entity.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
            entity.Property(x => x.EsPrincipal).HasColumnName("es_principal").HasDefaultValue(false).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasOne<Propiedad>().WithMany().HasForeignKey(x => x.PropiedadId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropiedadDocumento>(entity =>
        {
            entity.ToTable("propiedad_documentos", "propiedades");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.PropiedadId).HasColumnName("propiedad_id").IsRequired();
            entity.Property(x => x.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255).IsRequired();
            entity.Property(x => x.RutaArchivo).HasColumnName("ruta_archivo").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.TipoContenido).HasColumnName("tipo_contenido").HasMaxLength(100).IsRequired();
            entity.Property(x => x.CloudinaryPublicId).HasColumnName("cloudinary_public_id").HasMaxLength(255);
            entity.Property(x => x.CloudinaryResourceType).HasColumnName("cloudinary_resource_type").HasMaxLength(50);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasOne<Propiedad>().WithMany().HasForeignKey(x => x.PropiedadId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PropiedadInversionista>(entity =>
        {
            entity.ToTable("propiedad_inversionistas", "propiedades");
            entity.HasKey(x => new { x.PropiedadId, x.ClienteId });
            entity.Property(x => x.PropiedadId).HasColumnName("propiedad_id");
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id");
            entity.Property(x => x.PorcentajeParticipacion).HasColumnName("porcentaje_participacion").HasPrecision(5, 2).IsRequired();
            entity.Property(x => x.Observaciones).HasColumnName("observaciones");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasOne<Propiedad>().WithMany().HasForeignKey(x => x.PropiedadId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("chk_propiedad_inversionista_porcentaje", "porcentaje_participacion > 0 AND porcentaje_participacion <= 100"));
        });

        modelBuilder.Entity<TipoContrato>(entity =>
        {
            entity.ToTable("tipos_contrato", "contratos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<EstadoContrato>(entity =>
        {
            entity.ToTable("estados_contrato", "contratos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Contrato>(entity =>
        {
            entity.ToTable("contratos", "contratos");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
            entity.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
            entity.Property(x => x.PropiedadId).HasColumnName("propiedad_id").IsRequired();
            entity.Property(x => x.TipoContratoId).HasColumnName("tipo_contrato_id").IsRequired();
            entity.Property(x => x.EstadoContratoId).HasColumnName("estado_contrato_id").IsRequired();
            entity.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
            entity.Property(x => x.FechaFin).HasColumnName("fecha_fin");
            entity.Property(x => x.Observaciones).HasColumnName("observaciones");
            entity.Property(x => x.RutaArchivo).HasColumnName("ruta_archivo").HasMaxLength(1000).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp");
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at").HasColumnType("timestamp");
            entity.HasIndex(x => x.Codigo).IsUnique();
            entity.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Propiedad>().WithMany().HasForeignKey(x => x.PropiedadId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TipoContrato).WithMany().HasForeignKey(x => x.TipoContratoId);
            entity.HasOne(x => x.EstadoContrato).WithMany().HasForeignKey(x => x.EstadoContratoId);
            entity.ToTable(t => t.HasCheckConstraint("chk_contratos_fechas", "fecha_fin IS NULL OR fecha_fin >= fecha_inicio"));
        });

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    private void SetTimestamps()
    {
        var now = DateTime.Now;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetPropertyIfExists(entry.Entity, "CreatedAt", now);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetPropertyIfExists(entry.Entity, "UpdatedAt", now);
            }
        }
    }

    private static void SetPropertyIfExists(object entity, string propertyName, DateTime value)
    {
        var property = entity.GetType().GetProperty(propertyName);
        if (property is not null && property.PropertyType == typeof(DateTime))
        {
            property.SetValue(entity, value);
        }
        else if (property is not null && property.PropertyType == typeof(DateTime?))
        {
            property.SetValue(entity, value);
        }
    }
}
