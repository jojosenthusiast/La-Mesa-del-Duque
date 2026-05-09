CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "CategoriaProducto" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(100) NOT NULL,
        "Descripcion" character varying(250),
        "OrdenDisplay" integer NOT NULL DEFAULT 0,
        "Activo" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_CategoriaProducto" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Combos" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(150) NOT NULL,
        "Descripcion" text,
        "PrecioCombo" numeric(10,2) NOT NULL,
        "Activo" boolean NOT NULL DEFAULT TRUE,
        "FechaInicio" date NOT NULL,
        "FechaFin" date,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Combos" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Combo_PrecioCombo" CHECK ("PrecioCombo" > 0)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Mesa" (
        "Id" uuid NOT NULL,
        "Numero" integer NOT NULL,
        "Capacidad" integer NOT NULL,
        "Estado" character varying(30) NOT NULL,
        "Activa" boolean NOT NULL,
        CONSTRAINT "PK_Mesa" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Permisos" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(100) NOT NULL,
        "Modulo" character varying(50) NOT NULL,
        "Descripcion" character varying(250),
        CONSTRAINT "PK_Permisos" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Promociones" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(150) NOT NULL,
        "Descripcion" text,
        "TipoDescuento" character varying(20) NOT NULL,
        "ValorDescuento" numeric(10,2) NOT NULL,
        "FechaInicio" date NOT NULL,
        "FechaFin" date NOT NULL,
        "Activo" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Promociones" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Promocion_TipoDescuento" CHECK ("TipoDescuento" IN ('porcentaje', 'fijo')),
        CONSTRAINT "CK_Promocion_ValorDescuento" CHECK ("ValorDescuento" > 0)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Proveedor" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(200) NOT NULL,
        "Nit" character varying(32) NOT NULL,
        "Contacto" character varying(150),
        "Telefono" character varying(20),
        "Email" character varying(150),
        "Direccion" character varying(300),
        "Activo" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Proveedor" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "RestauranteConfigs" (
        "Id" integer NOT NULL,
        "Nombre" character varying(150) NOT NULL,
        "Direccion" character varying(300) NOT NULL,
        "Telefono" character varying(20),
        "HorarioApertura" time without time zone NOT NULL,
        "HorarioCierre" time without time zone NOT NULL,
        "CantidadMesas" integer NOT NULL,
        "DatosTicketJson" text,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_RestauranteConfigs" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_RestauranteConfig_CantidadMesas" CHECK ("CantidadMesas" > 0)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(50) NOT NULL,
        "Descripcion" character varying(250),
        "Activo" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Producto" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(150) NOT NULL,
        "Precio" numeric(10,2) NOT NULL,
        "CategoriaId" uuid NOT NULL,
        "Activo" boolean NOT NULL,
        "Descripcion" text,
        "ImagenUrl" character varying(500),
        "TiempoPreparacionMin" integer NOT NULL DEFAULT 5,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Producto" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Producto_CategoriaProducto_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "CategoriaProducto" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Pedido" (
        "Id" uuid NOT NULL,
        "MesaId" uuid NOT NULL,
        "Estado" character varying(30) NOT NULL,
        CONSTRAINT "PK_Pedido" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Pedido_Mesa_MesaId" FOREIGN KEY ("MesaId") REFERENCES "Mesa" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Ingrediente" (
        "Id" uuid NOT NULL,
        "Nombre" character varying(150) NOT NULL,
        "UnidadMedida" character varying(20) NOT NULL,
        "StockActual" numeric(10,3) NOT NULL,
        "StockMinimo" numeric(10,3) NOT NULL,
        "CostoUnitario" numeric(10,2) NOT NULL,
        "ProveedorDefaultId" uuid,
        "Activo" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Ingrediente" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Ingrediente_Proveedor_ProveedorDefaultId" FOREIGN KEY ("ProveedorDefaultId") REFERENCES "Proveedor" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "RolesPermisos" (
        "RolId" uuid NOT NULL,
        "PermisoId" uuid NOT NULL,
        CONSTRAINT "PK_RolesPermisos" PRIMARY KEY ("RolId", "PermisoId"),
        CONSTRAINT "FK_RolesPermisos_Permisos_PermisoId" FOREIGN KEY ("PermisoId") REFERENCES "Permisos" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RolesPermisos_Roles_RolId" FOREIGN KEY ("RolId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Usuarios" (
        "Id" uuid NOT NULL,
        "Username" character varying(50) NOT NULL,
        "Email" character varying(150),
        "PasswordHash" character varying(255) NOT NULL,
        "NombreCompleto" character varying(200) NOT NULL,
        "RolId" uuid NOT NULL,
        "Activo" boolean NOT NULL,
        "UltimoAcceso" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_Usuarios" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Usuarios_Roles_RolId" FOREIGN KEY ("RolId") REFERENCES "Roles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "CombosProductos" (
        "ComboId" uuid NOT NULL,
        "ProductoId" uuid NOT NULL,
        "Cantidad" integer NOT NULL DEFAULT 1,
        CONSTRAINT "PK_CombosProductos" PRIMARY KEY ("ComboId", "ProductoId"),
        CONSTRAINT "CK_ComboProducto_Cantidad" CHECK ("Cantidad" > 0),
        CONSTRAINT "FK_CombosProductos_Combos_ComboId" FOREIGN KEY ("ComboId") REFERENCES "Combos" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CombosProductos_Producto_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Producto" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "PromocionesProductos" (
        "PromocionId" uuid NOT NULL,
        "ProductoId" uuid NOT NULL,
        CONSTRAINT "PK_PromocionesProductos" PRIMARY KEY ("PromocionId", "ProductoId"),
        CONSTRAINT "FK_PromocionesProductos_Producto_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Producto" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PromocionesProductos_Promociones_PromocionId" FOREIGN KEY ("PromocionId") REFERENCES "Promociones" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "DetallePedido" (
        "Id" uuid NOT NULL,
        "ProductoId" uuid NOT NULL,
        "Cantidad" integer NOT NULL,
        "PrecioUnitario" numeric(10,2) NOT NULL,
        "PedidoId" uuid,
        CONSTRAINT "PK_DetallePedido" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DetallePedido_Pedido_PedidoId" FOREIGN KEY ("PedidoId") REFERENCES "Pedido" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DetallePedido_Producto_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Producto" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "ProductoIngrediente" (
        "ProductoId" uuid NOT NULL,
        "IngredienteId" uuid NOT NULL,
        "CantidadRequerida" numeric(10,3) NOT NULL,
        CONSTRAINT "PK_ProductoIngrediente" PRIMARY KEY ("ProductoId", "IngredienteId"),
        CONSTRAINT "FK_ProductoIngrediente_Ingrediente_IngredienteId" FOREIGN KEY ("IngredienteId") REFERENCES "Ingrediente" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ProductoIngrediente_Producto_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Producto" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "Auditorias" (
        "Id" bigint GENERATED ALWAYS AS IDENTITY,
        "TablaAfectada" character varying(100) NOT NULL,
        "RegistroId" uuid NOT NULL,
        "Accion" character varying(10) NOT NULL,
        "DatosAnteriores" jsonb,
        "DatosNuevos" jsonb,
        "UsuarioId" uuid NOT NULL,
        "IpAddress" character varying(45),
        "Fecha" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Auditorias" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Auditoria_Accion" CHECK ("Accion" IN ('INSERT', 'UPDATE', 'DELETE')),
        CONSTRAINT "FK_Auditorias_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "CierresDia" (
        "Id" uuid NOT NULL,
        "Fecha" date NOT NULL,
        "TotalVentas" numeric(12,2) NOT NULL,
        "TotalVentasEfectivo" numeric(12,2) NOT NULL,
        "TotalVentasTarjeta" numeric(12,2) NOT NULL,
        "TotalPedidos" integer NOT NULL,
        "TotalPedidosCancelados" integer NOT NULL,
        "TotalMermaValorizada" numeric(12,2) NOT NULL,
        "ResumenJson" text,
        "UsuarioId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_CierresDia" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CierresDia_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "OrdenesCompra" (
        "Id" uuid NOT NULL,
        "ProveedorId" uuid NOT NULL,
        "Estado" character varying(20) NOT NULL DEFAULT 'solicitado',
        "FechaSolicitud" timestamp with time zone NOT NULL,
        "FechaRecepcion" timestamp with time zone,
        "Notas" text,
        "ImpactoFallo" text,
        "UsuarioId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_OrdenesCompra" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_OrdenCompra_Estado" CHECK ("Estado" IN ('solicitado', 'en_camino', 'recibido', 'fallo')),
        CONSTRAINT "FK_OrdenesCompra_Proveedor_ProveedorId" FOREIGN KEY ("ProveedorId") REFERENCES "Proveedor" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrdenesCompra_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "PedidosEstadosLog" (
        "Id" uuid NOT NULL,
        "PedidoId" uuid NOT NULL,
        "EstadoAnterior" character varying(20) NOT NULL,
        "EstadoNuevo" character varying(20) NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "Notas" character varying(500),
        "FechaCambio" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_PedidosEstadosLog" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_PedidosEstadosLog_Pedido_PedidoId" FOREIGN KEY ("PedidoId") REFERENCES "Pedido" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_PedidosEstadosLog_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "ProductosPreciosHistorial" (
        "Id" uuid NOT NULL,
        "ProductoId" uuid NOT NULL,
        "PrecioAnterior" numeric(10,2) NOT NULL,
        "PrecioNuevo" numeric(10,2) NOT NULL,
        "Razon" character varying(500) NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "FechaCambio" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_ProductosPreciosHistorial" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProductosPreciosHistorial_Producto_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Producto" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ProductosPreciosHistorial_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "MermasDiarias" (
        "Id" uuid NOT NULL,
        "CierreDiaId" uuid NOT NULL,
        "IngredienteId" uuid NOT NULL,
        "CantidadDescartada" numeric(10,3) NOT NULL,
        "CostoEstimado" numeric(10,2) NOT NULL DEFAULT 0.0,
        "UsuarioId" uuid NOT NULL,
        "Notas" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_MermasDiarias" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_MermaDiaria_CantidadDescartada" CHECK ("CantidadDescartada" > 0),
        CONSTRAINT "CK_MermaDiaria_CostoEstimado" CHECK ("CostoEstimado" >= 0),
        CONSTRAINT "FK_MermasDiarias_CierresDia_CierreDiaId" FOREIGN KEY ("CierreDiaId") REFERENCES "CierresDia" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_MermasDiarias_Ingrediente_IngredienteId" FOREIGN KEY ("IngredienteId") REFERENCES "Ingrediente" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_MermasDiarias_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE TABLE "OrdenesCompraDetalle" (
        "Id" uuid NOT NULL,
        "OrdenCompraId" uuid NOT NULL,
        "IngredienteId" uuid NOT NULL,
        "CantidadSolicitada" numeric(10,3) NOT NULL,
        "CantidadRecibida" numeric(10,3),
        "PrecioUnitario" numeric(10,2) NOT NULL,
        CONSTRAINT "PK_OrdenesCompraDetalle" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_OrdenCompraDetalle_CantidadRecibida" CHECK ("CantidadRecibida" IS NULL OR "CantidadRecibida" >= 0),
        CONSTRAINT "CK_OrdenCompraDetalle_CantidadSolicitada" CHECK ("CantidadSolicitada" > 0),
        CONSTRAINT "CK_OrdenCompraDetalle_PrecioUnitario" CHECK ("PrecioUnitario" >= 0),
        CONSTRAINT "FK_OrdenesCompraDetalle_Ingrediente_IngredienteId" FOREIGN KEY ("IngredienteId") REFERENCES "Ingrediente" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrdenesCompraDetalle_OrdenesCompra_OrdenCompraId" FOREIGN KEY ("OrdenCompraId") REFERENCES "OrdenesCompra" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Auditorias_Fecha" ON "Auditorias" ("Fecha");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Auditorias_TablaAfectada_RegistroId" ON "Auditorias" ("TablaAfectada", "RegistroId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Auditorias_UsuarioId" ON "Auditorias" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_CategoriaProducto_Nombre" ON "CategoriaProducto" ("Nombre");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_CierresDia_Fecha" ON "CierresDia" ("Fecha");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_CierresDia_UsuarioId" ON "CierresDia" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_CombosProductos_ProductoId" ON "CombosProductos" ("ProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_DetallePedido_PedidoId" ON "DetallePedido" ("PedidoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_DetallePedido_ProductoId" ON "DetallePedido" ("ProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Ingrediente_Nombre" ON "Ingrediente" ("Nombre");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Ingrediente_ProveedorDefaultId" ON "Ingrediente" ("ProveedorDefaultId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_MermasDiarias_CierreDiaId" ON "MermasDiarias" ("CierreDiaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_MermasDiarias_IngredienteId" ON "MermasDiarias" ("IngredienteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_MermasDiarias_UsuarioId" ON "MermasDiarias" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Mesa_Numero" ON "Mesa" ("Numero");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_OrdenesCompra_FechaSolicitud" ON "OrdenesCompra" ("FechaSolicitud");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_OrdenesCompra_ProveedorId" ON "OrdenesCompra" ("ProveedorId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_OrdenesCompra_UsuarioId" ON "OrdenesCompra" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_OrdenesCompraDetalle_IngredienteId" ON "OrdenesCompraDetalle" ("IngredienteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_OrdenesCompraDetalle_OrdenCompraId" ON "OrdenesCompraDetalle" ("OrdenCompraId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Pedido_MesaId" ON "Pedido" ("MesaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_PedidosEstadosLog_FechaCambio" ON "PedidosEstadosLog" ("FechaCambio");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_PedidosEstadosLog_PedidoId" ON "PedidosEstadosLog" ("PedidoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_PedidosEstadosLog_UsuarioId" ON "PedidosEstadosLog" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Permisos_Nombre" ON "Permisos" ("Nombre");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Producto_CategoriaId" ON "Producto" ("CategoriaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_ProductoIngrediente_IngredienteId" ON "ProductoIngrediente" ("IngredienteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_ProductosPreciosHistorial_FechaCambio" ON "ProductosPreciosHistorial" ("FechaCambio");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_ProductosPreciosHistorial_ProductoId" ON "ProductosPreciosHistorial" ("ProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_ProductosPreciosHistorial_UsuarioId" ON "ProductosPreciosHistorial" ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_PromocionesProductos_ProductoId" ON "PromocionesProductos" ("ProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Proveedor_Nit" ON "Proveedor" ("Nit");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Roles_Nombre" ON "Roles" ("Nombre");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_RolesPermisos_PermisoId" ON "RolesPermisos" ("PermisoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Usuarios_Email" ON "Usuarios" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE INDEX "IX_Usuarios_RolId" ON "Usuarios" ("RolId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    CREATE UNIQUE INDEX "IX_Usuarios_Username" ON "Usuarios" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508184842_CrearEsquemaCompletoSprint1') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260508184842_CrearEsquemaCompletoSprint1', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508233514_CompletarFlujoPosPedidoSlice2') THEN
    ALTER TABLE "Pedido" DROP CONSTRAINT "FK_Pedido_Mesa_MesaId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508233514_CompletarFlujoPosPedidoSlice2') THEN
    ALTER TABLE "Pedido" ALTER COLUMN "MesaId" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508233514_CompletarFlujoPosPedidoSlice2') THEN
    ALTER TABLE "Pedido" ADD "TipoServicio" character varying(20) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508233514_CompletarFlujoPosPedidoSlice2') THEN
    ALTER TABLE "Pedido" ADD CONSTRAINT "FK_Pedido_Mesa_MesaId" FOREIGN KEY ("MesaId") REFERENCES "Mesa" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260508233514_CompletarFlujoPosPedidoSlice2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260508233514_CompletarFlujoPosPedidoSlice2', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509005009_AgregarRecetasProductosSprint1') THEN
    CREATE TABLE "RecetasProductos" (
        "Id" uuid NOT NULL,
        "ProductoId" uuid NOT NULL,
        "Instrucciones" text NOT NULL,
        CONSTRAINT "PK_RecetasProductos" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RecetasProductos_Producto_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES "Producto" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509005009_AgregarRecetasProductosSprint1') THEN
    CREATE TABLE "RecetasIngredientes" (
        "Id" uuid NOT NULL,
        "IngredienteId" uuid NOT NULL,
        "CantidadRequerida" numeric(10,3) NOT NULL,
        "RecetaProductoId" uuid NOT NULL,
        CONSTRAINT "PK_RecetasIngredientes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RecetasIngredientes_Ingrediente_IngredienteId" FOREIGN KEY ("IngredienteId") REFERENCES "Ingrediente" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RecetasIngredientes_RecetasProductos_RecetaProductoId" FOREIGN KEY ("RecetaProductoId") REFERENCES "RecetasProductos" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509005009_AgregarRecetasProductosSprint1') THEN
    CREATE INDEX "IX_RecetasIngredientes_IngredienteId" ON "RecetasIngredientes" ("IngredienteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509005009_AgregarRecetasProductosSprint1') THEN
    CREATE INDEX "IX_RecetasIngredientes_RecetaProductoId" ON "RecetasIngredientes" ("RecetaProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509005009_AgregarRecetasProductosSprint1') THEN
    CREATE INDEX "IX_RecetasProductos_ProductoId" ON "RecetasProductos" ("ProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509005009_AgregarRecetasProductosSprint1') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260509005009_AgregarRecetasProductosSprint1', '8.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509083023_EndurecerRecetaUnicaPorProductoYSeguridad') THEN
    DROP INDEX "IX_RecetasProductos_ProductoId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509083023_EndurecerRecetaUnicaPorProductoYSeguridad') THEN
    CREATE UNIQUE INDEX "IX_RecetasProductos_ProductoId" ON "RecetasProductos" ("ProductoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260509083023_EndurecerRecetaUnicaPorProductoYSeguridad') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260509083023_EndurecerRecetaUnicaPorProductoYSeguridad', '8.0.11');
    END IF;
END $EF$;
COMMIT;

