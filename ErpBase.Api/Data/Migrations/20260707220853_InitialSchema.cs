using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBase.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auditoria");

            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.EnsureSchema(
                name: "contratos");

            migrationBuilder.EnsureSchema(
                name: "propiedades");

            migrationBuilder.EnsureSchema(
                name: "seguridad");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "estados_contrato",
                schema: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_contrato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estados_propiedad",
                schema: "propiedades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_propiedad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permisos",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_cliente",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_cliente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_contrato",
                schema: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_contrato", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_documento_identidad",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    regex_validacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_documento_identidad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_propiedad",
                schema: "propiedades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_propiedad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    bloqueado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    motivo_bloqueo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    bloqueado_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    reactivado_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles_permisos",
                schema: "seguridad",
                columns: table => new
                {
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permiso_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_permisos", x => new { x.rol_id, x.permiso_id });
                    table.ForeignKey(
                        name: "FK_roles_permisos_permisos_permiso_id",
                        column: x => x.permiso_id,
                        principalSchema: "seguridad",
                        principalTable: "permisos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roles_permisos_roles_rol_id",
                        column: x => x.rol_id,
                        principalSchema: "seguridad",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    tipo_cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento_identidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_identidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombres = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    razon_social = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                    table.CheckConstraint("chk_cliente_nombre_empresa", "(razon_social IS NOT NULL AND nombres IS NULL AND apellidos IS NULL) OR (razon_social IS NULL AND nombres IS NOT NULL AND apellidos IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_clientes_tipos_cliente_tipo_cliente_id",
                        column: x => x.tipo_cliente_id,
                        principalSchema: "crm",
                        principalTable: "tipos_cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_clientes_tipos_documento_identidad_tipo_documento_identidad~",
                        column: x => x.tipo_documento_identidad_id,
                        principalSchema: "crm",
                        principalTable: "tipos_documento_identidad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "propiedades",
                schema: "propiedades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo_propiedad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_propiedad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    titulo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    latitud = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: false),
                    longitud = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: false),
                    direccion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    metros_cuadrados_terreno = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    metros_cuadrados_construccion = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    habitaciones = table.Column<int>(type: "integer", nullable: true),
                    banos = table.Column<int>(type: "integer", nullable: true),
                    precio_venta = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    precio_alquiler = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedades", x => x.id);
                    table.CheckConstraint("chk_area_construccion", "metros_cuadrados_construccion IS NULL OR (metros_cuadrados_construccion > 0 AND metros_cuadrados_construccion <= metros_cuadrados_terreno)");
                    table.CheckConstraint("chk_banos", "banos IS NULL OR banos >= 0");
                    table.CheckConstraint("chk_habitaciones", "habitaciones IS NULL OR habitaciones >= 0");
                    table.CheckConstraint("chk_latitud", "latitud BETWEEN -90 AND 90");
                    table.CheckConstraint("chk_longitud", "longitud BETWEEN -180 AND 180");
                    table.CheckConstraint("chk_metros_cuadrados_terreno", "metros_cuadrados_terreno > 0");
                    table.ForeignKey(
                        name: "FK_propiedades_estados_propiedad_estado_propiedad_id",
                        column: x => x.estado_propiedad_id,
                        principalSchema: "propiedades",
                        principalTable: "estados_propiedad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_propiedades_tipos_propiedad_tipo_propiedad_id",
                        column: x => x.tipo_propiedad_id,
                        principalSchema: "propiedades",
                        principalTable: "tipos_propiedad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "auditorias",
                schema: "auditoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    accion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    registro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditorias", x => x.id);
                    table.ForeignKey(
                        name: "FK_auditorias_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguridad",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                schema: "seguridad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguridad",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                schema: "seguridad",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rol_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => new { x.usuario_id, x.rol_id });
                    table.ForeignKey(
                        name: "FK_usuario_roles_roles_rol_id",
                        column: x => x.rol_id,
                        principalSchema: "seguridad",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalSchema: "seguridad",
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contratos",
                schema: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    propiedad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_contrato_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    ruta_archivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contratos", x => x.id);
                    table.CheckConstraint("chk_contratos_fechas", "fecha_fin IS NULL OR fecha_fin >= fecha_inicio");
                    table.ForeignKey(
                        name: "FK_contratos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "crm",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contratos_estados_contrato_estado_contrato_id",
                        column: x => x.estado_contrato_id,
                        principalSchema: "contratos",
                        principalTable: "estados_contrato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_contratos_propiedades_propiedad_id",
                        column: x => x.propiedad_id,
                        principalSchema: "propiedades",
                        principalTable: "propiedades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contratos_tipos_contrato_tipo_contrato_id",
                        column: x => x.tipo_contrato_id,
                        principalSchema: "contratos",
                        principalTable: "tipos_contrato",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "propiedad_documentos",
                schema: "propiedades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    propiedad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ruta_archivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    tipo_contenido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedad_documentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_propiedad_documentos_propiedades_propiedad_id",
                        column: x => x.propiedad_id,
                        principalSchema: "propiedades",
                        principalTable: "propiedades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "propiedad_imagenes",
                schema: "propiedades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    propiedad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ruta_archivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    es_principal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedad_imagenes", x => x.id);
                    table.ForeignKey(
                        name: "FK_propiedad_imagenes_propiedades_propiedad_id",
                        column: x => x.propiedad_id,
                        principalSchema: "propiedades",
                        principalTable: "propiedades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "propiedad_inversionistas",
                schema: "propiedades",
                columns: table => new
                {
                    propiedad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    porcentaje_participacion = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedad_inversionistas", x => new { x.propiedad_id, x.cliente_id });
                    table.CheckConstraint("chk_propiedad_inversionista_porcentaje", "porcentaje_participacion > 0 AND porcentaje_participacion <= 100");
                    table.ForeignKey(
                        name: "FK_propiedad_inversionistas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalSchema: "crm",
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_propiedad_inversionistas_propiedades_propiedad_id",
                        column: x => x.propiedad_id,
                        principalSchema: "propiedades",
                        principalTable: "propiedades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_entidad_registro_id",
                schema: "auditoria",
                table: "auditorias",
                columns: new[] { "entidad", "registro_id" });

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_usuario_id",
                schema: "auditoria",
                table: "auditorias",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_tipo_cliente_id",
                schema: "crm",
                table: "clientes",
                column: "tipo_cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_tipo_documento_identidad_id_documento_identidad",
                schema: "crm",
                table: "clientes",
                columns: new[] { "tipo_documento_identidad_id", "documento_identidad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contratos_cliente_id",
                schema: "contratos",
                table: "contratos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_codigo",
                schema: "contratos",
                table: "contratos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contratos_estado_contrato_id",
                schema: "contratos",
                table: "contratos",
                column: "estado_contrato_id");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_propiedad_id",
                schema: "contratos",
                table: "contratos",
                column: "propiedad_id");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_tipo_contrato_id",
                schema: "contratos",
                table: "contratos",
                column: "tipo_contrato_id");

            migrationBuilder.CreateIndex(
                name: "IX_estados_contrato_nombre",
                schema: "contratos",
                table: "estados_contrato",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estados_propiedad_nombre",
                schema: "propiedades",
                table: "estados_propiedad",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_token_hash",
                schema: "seguridad",
                table: "password_reset_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_usuario_id",
                schema: "seguridad",
                table: "password_reset_tokens",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_permisos_codigo",
                schema: "seguridad",
                table: "permisos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_propiedad_documentos_propiedad_id",
                schema: "propiedades",
                table: "propiedad_documentos",
                column: "propiedad_id");

            migrationBuilder.CreateIndex(
                name: "IX_propiedad_imagenes_propiedad_id",
                schema: "propiedades",
                table: "propiedad_imagenes",
                column: "propiedad_id");

            migrationBuilder.CreateIndex(
                name: "IX_propiedad_inversionistas_cliente_id",
                schema: "propiedades",
                table: "propiedad_inversionistas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_codigo",
                schema: "propiedades",
                table: "propiedades",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_estado_propiedad_id",
                schema: "propiedades",
                table: "propiedades",
                column: "estado_propiedad_id");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_tipo_propiedad_id",
                schema: "propiedades",
                table: "propiedades",
                column: "tipo_propiedad_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre",
                schema: "seguridad",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_permisos_permiso_id",
                schema: "seguridad",
                table: "roles_permisos",
                column: "permiso_id");

            migrationBuilder.CreateIndex(
                name: "IX_tipos_cliente_nombre",
                schema: "crm",
                table: "tipos_cliente",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_contrato_nombre",
                schema: "contratos",
                table: "tipos_contrato",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_documento_identidad_nombre",
                schema: "crm",
                table: "tipos_documento_identidad",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_propiedad_nombre",
                schema: "propiedades",
                table: "tipos_propiedad",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_rol_id",
                schema: "seguridad",
                table: "usuario_roles",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                schema: "seguridad",
                table: "usuarios",
                column: "email",
                unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditorias",
                schema: "auditoria");

            migrationBuilder.DropTable(
                name: "contratos",
                schema: "contratos");

            migrationBuilder.DropTable(
                name: "password_reset_tokens",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "propiedad_documentos",
                schema: "propiedades");

            migrationBuilder.DropTable(
                name: "propiedad_imagenes",
                schema: "propiedades");

            migrationBuilder.DropTable(
                name: "propiedad_inversionistas",
                schema: "propiedades");

            migrationBuilder.DropTable(
                name: "roles_permisos",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuario_roles",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "estados_contrato",
                schema: "contratos");

            migrationBuilder.DropTable(
                name: "tipos_contrato",
                schema: "contratos");

            migrationBuilder.DropTable(
                name: "clientes",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "propiedades",
                schema: "propiedades");

            migrationBuilder.DropTable(
                name: "permisos",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "seguridad");

            migrationBuilder.DropTable(
                name: "tipos_cliente",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "tipos_documento_identidad",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "estados_propiedad",
                schema: "propiedades");

            migrationBuilder.DropTable(
                name: "tipos_propiedad",
                schema: "propiedades");
        }
    }
}
