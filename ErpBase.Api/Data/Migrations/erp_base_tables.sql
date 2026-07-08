CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'auditoria') THEN
        CREATE SCHEMA auditoria;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'crm') THEN
        CREATE SCHEMA crm;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'contratos') THEN
        CREATE SCHEMA contratos;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'propiedades') THEN
        CREATE SCHEMA propiedades;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'seguridad') THEN
        CREATE SCHEMA seguridad;
    END IF;
END $EF$;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE contratos.estados_contrato (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(50) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_estados_contrato" PRIMARY KEY (id)
);

CREATE TABLE propiedades.estados_propiedad (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(50) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_estados_propiedad" PRIMARY KEY (id)
);

CREATE TABLE seguridad.permisos (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    codigo character varying(50) NOT NULL,
    nombre character varying(100) NOT NULL,
    descripcion text,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_permisos" PRIMARY KEY (id)
);

CREATE TABLE seguridad.roles (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(100) NOT NULL,
    descripcion text,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_roles" PRIMARY KEY (id)
);

CREATE TABLE crm.tipos_cliente (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(50) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_tipos_cliente" PRIMARY KEY (id)
);

CREATE TABLE contratos.tipos_contrato (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(100) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_tipos_contrato" PRIMARY KEY (id)
);

CREATE TABLE crm.tipos_documento_identidad (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(20) NOT NULL,
    regex_validacion character varying(500),
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_tipos_documento_identidad" PRIMARY KEY (id)
);

CREATE TABLE propiedades.tipos_propiedad (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    nombre character varying(100) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_tipos_propiedad" PRIMARY KEY (id)
);

CREATE TABLE seguridad.usuarios (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    email character varying(255) NOT NULL,
    password_hash text NOT NULL,
    activo boolean NOT NULL DEFAULT TRUE,
    bloqueado boolean NOT NULL DEFAULT FALSE,
    motivo_bloqueo character varying(500),
    bloqueado_at timestamp,
    reactivado_at timestamp,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_usuarios" PRIMARY KEY (id)
);

CREATE TABLE seguridad.roles_permisos (
    rol_id uuid NOT NULL,
    permiso_id uuid NOT NULL,
    CONSTRAINT "PK_roles_permisos" PRIMARY KEY (rol_id, permiso_id),
    CONSTRAINT "FK_roles_permisos_permisos_permiso_id" FOREIGN KEY (permiso_id) REFERENCES seguridad.permisos (id) ON DELETE CASCADE,
    CONSTRAINT "FK_roles_permisos_roles_rol_id" FOREIGN KEY (rol_id) REFERENCES seguridad.roles (id) ON DELETE CASCADE
);

CREATE TABLE crm.clientes (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    tipo_cliente_id uuid NOT NULL,
    tipo_documento_identidad_id uuid NOT NULL,
    documento_identidad character varying(20) NOT NULL,
    nombres character varying(150),
    apellidos character varying(150),
    razon_social character varying(250),
    telefono character varying(50),
    email character varying(255),
    direccion text,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_clientes" PRIMARY KEY (id),
    CONSTRAINT chk_cliente_nombre_empresa CHECK ((razon_social IS NOT NULL AND nombres IS NULL AND apellidos IS NULL) OR (razon_social IS NULL AND nombres IS NOT NULL AND apellidos IS NOT NULL)),
    CONSTRAINT "FK_clientes_tipos_cliente_tipo_cliente_id" FOREIGN KEY (tipo_cliente_id) REFERENCES crm.tipos_cliente (id) ON DELETE CASCADE,
    CONSTRAINT "FK_clientes_tipos_documento_identidad_tipo_documento_identidad~" FOREIGN KEY (tipo_documento_identidad_id) REFERENCES crm.tipos_documento_identidad (id) ON DELETE CASCADE
);

CREATE TABLE propiedades.propiedades (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    codigo character varying(50) NOT NULL,
    tipo_propiedad_id uuid NOT NULL,
    estado_propiedad_id uuid NOT NULL,
    titulo character varying(255) NOT NULL,
    descripcion text,
    latitud numeric(10,8) NOT NULL,
    longitud numeric(11,8) NOT NULL,
    direccion character varying(1000) NOT NULL,
    metros_cuadrados_terreno numeric(10,2) NOT NULL,
    metros_cuadrados_construccion numeric(10,2),
    habitaciones integer,
    banos integer,
    precio_venta numeric(14,2),
    precio_alquiler numeric(14,2),
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_propiedades" PRIMARY KEY (id),
    CONSTRAINT chk_area_construccion CHECK (metros_cuadrados_construccion IS NULL OR (metros_cuadrados_construccion > 0 AND metros_cuadrados_construccion <= metros_cuadrados_terreno)),
    CONSTRAINT chk_banos CHECK (banos IS NULL OR banos >= 0),
    CONSTRAINT chk_habitaciones CHECK (habitaciones IS NULL OR habitaciones >= 0),
    CONSTRAINT chk_latitud CHECK (latitud BETWEEN -90 AND 90),
    CONSTRAINT chk_longitud CHECK (longitud BETWEEN -180 AND 180),
    CONSTRAINT chk_metros_cuadrados_terreno CHECK (metros_cuadrados_terreno > 0),
    CONSTRAINT "FK_propiedades_estados_propiedad_estado_propiedad_id" FOREIGN KEY (estado_propiedad_id) REFERENCES propiedades.estados_propiedad (id) ON DELETE CASCADE,
    CONSTRAINT "FK_propiedades_tipos_propiedad_tipo_propiedad_id" FOREIGN KEY (tipo_propiedad_id) REFERENCES propiedades.tipos_propiedad (id) ON DELETE CASCADE
);

CREATE TABLE auditoria.auditorias (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    usuario_id uuid,
    accion character varying(100) NOT NULL,
    entidad character varying(100) NOT NULL,
    registro_id uuid,
    fecha timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_auditorias" PRIMARY KEY (id),
    CONSTRAINT "FK_auditorias_usuarios_usuario_id" FOREIGN KEY (usuario_id) REFERENCES seguridad.usuarios (id) ON DELETE SET NULL
);

CREATE TABLE seguridad.password_reset_tokens (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    usuario_id uuid NOT NULL,
    token_hash character varying(128) NOT NULL,
    expires_at timestamp NOT NULL,
    used_at timestamp,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_password_reset_tokens" PRIMARY KEY (id),
    CONSTRAINT "FK_password_reset_tokens_usuarios_usuario_id" FOREIGN KEY (usuario_id) REFERENCES seguridad.usuarios (id) ON DELETE CASCADE
);

CREATE TABLE seguridad.usuario_roles (
    usuario_id uuid NOT NULL,
    rol_id uuid NOT NULL,
    CONSTRAINT "PK_usuario_roles" PRIMARY KEY (usuario_id, rol_id),
    CONSTRAINT "FK_usuario_roles_roles_rol_id" FOREIGN KEY (rol_id) REFERENCES seguridad.roles (id) ON DELETE CASCADE,
    CONSTRAINT "FK_usuario_roles_usuarios_usuario_id" FOREIGN KEY (usuario_id) REFERENCES seguridad.usuarios (id) ON DELETE CASCADE
);

CREATE TABLE contratos.contratos (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    codigo character varying(50) NOT NULL,
    cliente_id uuid NOT NULL,
    propiedad_id uuid NOT NULL,
    tipo_contrato_id uuid NOT NULL,
    estado_contrato_id uuid NOT NULL,
    fecha_inicio date NOT NULL,
    fecha_fin date,
    observaciones text,
    ruta_archivo character varying(1000) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_contratos" PRIMARY KEY (id),
    CONSTRAINT chk_contratos_fechas CHECK (fecha_fin IS NULL OR fecha_fin >= fecha_inicio),
    CONSTRAINT "FK_contratos_clientes_cliente_id" FOREIGN KEY (cliente_id) REFERENCES crm.clientes (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_contratos_estados_contrato_estado_contrato_id" FOREIGN KEY (estado_contrato_id) REFERENCES contratos.estados_contrato (id) ON DELETE CASCADE,
    CONSTRAINT "FK_contratos_propiedades_propiedad_id" FOREIGN KEY (propiedad_id) REFERENCES propiedades.propiedades (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_contratos_tipos_contrato_tipo_contrato_id" FOREIGN KEY (tipo_contrato_id) REFERENCES contratos.tipos_contrato (id) ON DELETE CASCADE
);

CREATE TABLE propiedades.propiedad_documentos (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    propiedad_id uuid NOT NULL,
    nombre_archivo character varying(255) NOT NULL,
    ruta_archivo character varying(1000) NOT NULL,
    tipo_contenido character varying(100) NOT NULL,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_propiedad_documentos" PRIMARY KEY (id),
    CONSTRAINT "FK_propiedad_documentos_propiedades_propiedad_id" FOREIGN KEY (propiedad_id) REFERENCES propiedades.propiedades (id) ON DELETE CASCADE
);

CREATE TABLE propiedades.propiedad_imagenes (
    id uuid NOT NULL DEFAULT (uuid_generate_v4()),
    propiedad_id uuid NOT NULL,
    nombre_archivo character varying(255) NOT NULL,
    ruta_archivo character varying(1000) NOT NULL,
    descripcion character varying(500),
    es_principal boolean NOT NULL DEFAULT FALSE,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_propiedad_imagenes" PRIMARY KEY (id),
    CONSTRAINT "FK_propiedad_imagenes_propiedades_propiedad_id" FOREIGN KEY (propiedad_id) REFERENCES propiedades.propiedades (id) ON DELETE CASCADE
);

CREATE TABLE propiedades.propiedad_inversionistas (
    propiedad_id uuid NOT NULL,
    cliente_id uuid NOT NULL,
    porcentaje_participacion numeric(5,2) NOT NULL,
    observaciones text,
    created_at timestamp NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    updated_at timestamp,
    deleted_at timestamp,
    CONSTRAINT "PK_propiedad_inversionistas" PRIMARY KEY (propiedad_id, cliente_id),
    CONSTRAINT chk_propiedad_inversionista_porcentaje CHECK (porcentaje_participacion > 0 AND porcentaje_participacion <= 100),
    CONSTRAINT "FK_propiedad_inversionistas_clientes_cliente_id" FOREIGN KEY (cliente_id) REFERENCES crm.clientes (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_propiedad_inversionistas_propiedades_propiedad_id" FOREIGN KEY (propiedad_id) REFERENCES propiedades.propiedades (id) ON DELETE CASCADE
);

CREATE INDEX "IX_auditorias_entidad_registro_id" ON auditoria.auditorias (entidad, registro_id);

CREATE INDEX "IX_auditorias_usuario_id" ON auditoria.auditorias (usuario_id);

CREATE INDEX "IX_clientes_tipo_cliente_id" ON crm.clientes (tipo_cliente_id);

CREATE UNIQUE INDEX "IX_clientes_tipo_documento_identidad_id_documento_identidad" ON crm.clientes (tipo_documento_identidad_id, documento_identidad);

CREATE INDEX "IX_contratos_cliente_id" ON contratos.contratos (cliente_id);

CREATE UNIQUE INDEX "IX_contratos_codigo" ON contratos.contratos (codigo);

CREATE INDEX "IX_contratos_estado_contrato_id" ON contratos.contratos (estado_contrato_id);

CREATE INDEX "IX_contratos_propiedad_id" ON contratos.contratos (propiedad_id);

CREATE INDEX "IX_contratos_tipo_contrato_id" ON contratos.contratos (tipo_contrato_id);

CREATE UNIQUE INDEX "IX_estados_contrato_nombre" ON contratos.estados_contrato (nombre);

CREATE UNIQUE INDEX "IX_estados_propiedad_nombre" ON propiedades.estados_propiedad (nombre);

CREATE UNIQUE INDEX "IX_password_reset_tokens_token_hash" ON seguridad.password_reset_tokens (token_hash);

CREATE INDEX "IX_password_reset_tokens_usuario_id" ON seguridad.password_reset_tokens (usuario_id);

CREATE UNIQUE INDEX "IX_permisos_codigo" ON seguridad.permisos (codigo);

CREATE INDEX "IX_propiedad_documentos_propiedad_id" ON propiedades.propiedad_documentos (propiedad_id);

CREATE INDEX "IX_propiedad_imagenes_propiedad_id" ON propiedades.propiedad_imagenes (propiedad_id);

CREATE INDEX "IX_propiedad_inversionistas_cliente_id" ON propiedades.propiedad_inversionistas (cliente_id);

CREATE UNIQUE INDEX "IX_propiedades_codigo" ON propiedades.propiedades (codigo);

CREATE INDEX "IX_propiedades_estado_propiedad_id" ON propiedades.propiedades (estado_propiedad_id);

CREATE INDEX "IX_propiedades_tipo_propiedad_id" ON propiedades.propiedades (tipo_propiedad_id);

CREATE UNIQUE INDEX "IX_roles_nombre" ON seguridad.roles (nombre);

CREATE INDEX "IX_roles_permisos_permiso_id" ON seguridad.roles_permisos (permiso_id);

CREATE UNIQUE INDEX "IX_tipos_cliente_nombre" ON crm.tipos_cliente (nombre);

CREATE UNIQUE INDEX "IX_tipos_contrato_nombre" ON contratos.tipos_contrato (nombre);

CREATE UNIQUE INDEX "IX_tipos_documento_identidad_nombre" ON crm.tipos_documento_identidad (nombre);

CREATE UNIQUE INDEX "IX_tipos_propiedad_nombre" ON propiedades.tipos_propiedad (nombre);

CREATE INDEX "IX_usuario_roles_rol_id" ON seguridad.usuario_roles (rol_id);

CREATE UNIQUE INDEX "IX_usuarios_email" ON seguridad.usuarios (email);

INSERT INTO crm.tipos_cliente(nombre)
VALUES ('Persona'), ('Empresa')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO crm.tipos_documento_identidad(nombre, regex_validacion)
VALUES ('Cedula', '^\d{11}$'), ('RNC', '^\d{9}$'), ('Pasaporte', '^[A-Z0-9]{6,20}$')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO propiedades.tipos_propiedad(nombre)
VALUES ('Apartamento'), ('Casa'), ('Solar'), ('Local Comercial'), ('Oficina')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO propiedades.estados_propiedad(nombre)
VALUES ('Disponible'), ('Reservada'), ('Vendida'), ('Alquilada')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO contratos.tipos_contrato(nombre)
VALUES ('Venta'), ('Alquiler'), ('Arrendamiento')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO contratos.estados_contrato(nombre)
VALUES ('Activo'), ('Vencido'), ('Cancelado')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260707220853_InitialSchema', '10.0.9');

COMMIT;

START TRANSACTION;
ALTER TABLE propiedades.propiedad_imagenes ADD cloudinary_public_id character varying(255);

ALTER TABLE propiedades.propiedad_imagenes ADD cloudinary_resource_type character varying(50);

ALTER TABLE propiedades.propiedad_documentos ADD cloudinary_public_id character varying(255);

ALTER TABLE propiedades.propiedad_documentos ADD cloudinary_resource_type character varying(50);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260707235508_AddCloudinaryStorageFields', '10.0.9');

COMMIT;

