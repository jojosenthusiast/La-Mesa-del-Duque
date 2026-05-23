using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidadaSprint3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alergenos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Icono = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alergenos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    OrdenDisplay = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    EstacionCocina = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaProducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    PrecioCombo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    FechaInicio = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.Id);
                    table.CheckConstraint("CK_Combo_PrecioCombo", "\"PrecioCombo\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCocina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductoNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Alergenos = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    IngredientesQuitados = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    IngredientesExtra = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    CocineroId = table.Column<int>(type: "INTEGER", nullable: true),
                    Estacion = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraRecibido = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraListo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MesaNumero = table.Column<int>(type: "INTEGER", nullable: true),
                    TipoServicio = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Curso = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TiempoPreparacionMin = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCocina", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    TipoDescuento = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ValorDescuento = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.Id);
                    table.CheckConstraint("CK_Promocion_TipoDescuento", "\"TipoDescuento\" IN ('porcentaje', 'fijo')");
                    table.CheckConstraint("CK_Promocion_ValorDescuento", "\"ValorDescuento\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Nit = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Contacto = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestauranteConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    HorarioApertura = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HorarioCierre = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CantidadMesas = table.Column<int>(type: "INTEGER", nullable: false),
                    DatosTicketJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestauranteConfigs", x => x.Id);
                    table.CheckConstraint("CK_RestauranteConfig_CantidadMesas", "\"CantidadMesas\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonasSalon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonasSalon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    ImagenUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TiempoPreparacionMin = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Producto_CategoriaProducto_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "CategoriaProducto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingrediente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    UnidadMedida = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StockActual = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    ProveedorDefaultId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingrediente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingrediente_Proveedor_ProveedorDefaultId",
                        column: x => x.ProveedorDefaultId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RolesPermisos",
                columns: table => new
                {
                    RolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermisoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NombreCompleto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mesa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    OcupadaDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PosicionX = table.Column<int>(type: "INTEGER", nullable: true),
                    PosicionY = table.Column<int>(type: "INTEGER", nullable: true),
                    ZonaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Forma = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Rotacion = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mesa_ZonasSalon_ZonaId",
                        column: x => x.ZonaId,
                        principalTable: "ZonasSalon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CombosProductos",
                columns: table => new
                {
                    ComboId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombosProductos", x => new { x.ComboId, x.ProductoId });
                    table.CheckConstraint("CK_ComboProducto_Cantidad", "\"Cantidad\" > 0");
                    table.ForeignKey(
                        name: "FK_CombosProductos_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CombosProductos_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductosAlergenos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlergenoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Justificacion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosAlergenos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosAlergenos_Alergenos_AlergenoId",
                        column: x => x.AlergenoId,
                        principalTable: "Alergenos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductosAlergenos_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromocionesProductos",
                columns: table => new
                {
                    PromocionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromocionesProductos", x => new { x.PromocionId, x.ProductoId });
                    table.ForeignKey(
                        name: "FK_PromocionesProductos_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromocionesProductos_Promociones_PromocionId",
                        column: x => x.PromocionId,
                        principalTable: "Promociones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecetasProductos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Instrucciones = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetasProductos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecetasProductos_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductoIngrediente",
                columns: table => new
                {
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CantidadRequerida = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoIngrediente", x => new { x.ProductoId, x.IngredienteId });
                    table.ForeignKey(
                        name: "FK_ProductoIngrediente_Ingrediente_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingrediente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductoIngrediente_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TablaAfectada = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RegistroId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Accion = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DatosAnteriores = table.Column<string>(type: "jsonb", nullable: true),
                    DatosNuevos = table.Column<string>(type: "jsonb", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.CheckConstraint("CK_Auditoria_Accion", "\"Accion\" IN ('INSERT', 'UPDATE', 'DELETE')");
                    table.ForeignKey(
                        name: "FK_Auditorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CierresDia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TotalVentas = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    TotalVentasEfectivo = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    TotalVentasTarjeta = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    TotalPedidos = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPedidosCancelados = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMermaValorizada = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    ResumenJson = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EfectivoReal = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    TarjetaReal = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    DiferenciaEfectivo = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    DiferenciaTarjeta = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    EsCerrado = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CerradoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresDia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CierresDia_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProveedorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "solicitado"),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    ImpactoFallo = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompra", x => x.Id);
                    table.CheckConstraint("CK_OrdenCompra_Estado", "\"Estado\" IN ('solicitado', 'en_camino', 'recibido', 'fallo')");
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompra_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductosPreciosHistorial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrecioAnterior = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    PrecioNuevo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Razon = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosPreciosHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosPreciosHistorial_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductosPreciosHistorial_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoServicio = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MesaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_Mesa_MesaId",
                        column: x => x.MesaId,
                        principalTable: "Mesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecetasIngredientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CantidadRequerida = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    RecetaProductoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetasIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecetasIngredientes_Ingrediente_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingrediente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecetasIngredientes_RecetasProductos_RecetaProductoId",
                        column: x => x.RecetaProductoId,
                        principalTable: "RecetasProductos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MermasDiarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CierreDiaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CantidadDescartada = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    CostoEstimado = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Lote = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MermasDiarias", x => x.Id);
                    table.CheckConstraint("CK_MermaDiaria_CantidadDescartada", "\"CantidadDescartada\" > 0");
                    table.CheckConstraint("CK_MermaDiaria_CostoEstimado", "\"CostoEstimado\" >= 0");
                    table.ForeignKey(
                        name: "FK_MermasDiarias_CierresDia_CierreDiaId",
                        column: x => x.CierreDiaId,
                        principalTable: "CierresDia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MermasDiarias_Ingrediente_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingrediente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MermasDiarias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesCompraDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrdenCompraId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CantidadSolicitada = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "TEXT", precision: 10, scale: 3, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompraDetalle", x => x.Id);
                    table.CheckConstraint("CK_OrdenCompraDetalle_CantidadRecibida", "\"CantidadRecibida\" IS NULL OR \"CantidadRecibida\" >= 0");
                    table.CheckConstraint("CK_OrdenCompraDetalle_CantidadSolicitada", "\"CantidadSolicitada\" > 0");
                    table.CheckConstraint("CK_OrdenCompraDetalle_PrecioUnitario", "\"PrecioUnitario\" >= 0");
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_Ingrediente_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingrediente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalle_OrdenesCompra_OrdenCompraId",
                        column: x => x.OrdenCompraId,
                        principalTable: "OrdenesCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    PropinaMonto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cuentas_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallePedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ModificacionesJson = table.Column<string>(type: "TEXT", nullable: true),
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallePedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallePedido_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallePedido_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Producto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PedidosEstadosLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FechaCambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosEstadosLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosEstadosLog_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidosEstadosLog_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentaDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CuentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CantidadAsignada = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentaDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentaDetalle_Cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CuentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PropinaMonto = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Metodo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_TablaAfectada_RegistroId",
                table: "Auditorias",
                columns: new[] { "TablaAfectada", "RegistroId" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaProducto_Nombre",
                table: "CategoriaProducto",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CierresDia_Fecha",
                table: "CierresDia",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CierresDia_UsuarioId",
                table: "CierresDia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CombosProductos_ProductoId",
                table: "CombosProductos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaDetalle_CuentaId",
                table: "CuentaDetalle",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_PedidoId",
                table: "Cuentas",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_PedidoId_Estado",
                table: "Cuentas",
                columns: new[] { "PedidoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedido_PedidoId",
                table: "DetallePedido",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedido_ProductoId",
                table: "DetallePedido",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingrediente_Nombre",
                table: "Ingrediente",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingrediente_ProveedorDefaultId",
                table: "Ingrediente",
                column: "ProveedorDefaultId");

            migrationBuilder.CreateIndex(
                name: "IX_MermasDiarias_CierreDiaId",
                table: "MermasDiarias",
                column: "CierreDiaId");

            migrationBuilder.CreateIndex(
                name: "IX_MermasDiarias_IngredienteId",
                table: "MermasDiarias",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MermasDiarias_UsuarioId",
                table: "MermasDiarias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Mesa_Numero",
                table: "Mesa",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mesa_ZonaId",
                table: "Mesa",
                column: "ZonaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCocina_Estacion_Estado",
                table: "OrdenesCocina",
                columns: new[] { "Estacion", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCocina_Estado_HoraRecibido",
                table: "OrdenesCocina",
                columns: new[] { "Estado", "HoraRecibido" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_FechaSolicitud",
                table: "OrdenesCompra",
                column: "FechaSolicitud");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_ProveedorId",
                table: "OrdenesCompra",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_UsuarioId",
                table: "OrdenesCompra",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalle_IngredienteId",
                table: "OrdenesCompraDetalle",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalle_OrdenCompraId",
                table: "OrdenesCompraDetalle",
                column: "OrdenCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CuentaId",
                table: "Pagos",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_MesaId",
                table: "Pedido",
                column: "MesaId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosEstadosLog_FechaCambio",
                table: "PedidosEstadosLog",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosEstadosLog_PedidoId",
                table: "PedidosEstadosLog",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosEstadosLog_UsuarioId",
                table: "PedidosEstadosLog",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Nombre",
                table: "Permisos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producto_CategoriaId",
                table: "Producto",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoIngrediente_IngredienteId",
                table: "ProductoIngrediente",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAlergenos_AlergenoId",
                table: "ProductosAlergenos",
                column: "AlergenoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAlergenos_ProductoId",
                table: "ProductosAlergenos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosPreciosHistorial_FechaCambio",
                table: "ProductosPreciosHistorial",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosPreciosHistorial_ProductoId",
                table: "ProductosPreciosHistorial",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosPreciosHistorial_UsuarioId",
                table: "ProductosPreciosHistorial",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PromocionesProductos_ProductoId",
                table: "PromocionesProductos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_Nit",
                table: "Proveedor",
                column: "Nit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecetasIngredientes_IngredienteId",
                table: "RecetasIngredientes",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasIngredientes_RecetaProductoId",
                table: "RecetasIngredientes",
                column: "RecetaProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasProductos_ProductoId",
                table: "RecetasProductos",
                column: "ProductoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_PermisoId",
                table: "RolesPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Username",
                table: "Usuarios",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZonasSalon_Nombre",
                table: "ZonasSalon",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "CombosProductos");

            migrationBuilder.DropTable(
                name: "CuentaDetalle");

            migrationBuilder.DropTable(
                name: "DetallePedido");

            migrationBuilder.DropTable(
                name: "MermasDiarias");

            migrationBuilder.DropTable(
                name: "OrdenesCocina");

            migrationBuilder.DropTable(
                name: "OrdenesCompraDetalle");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "PedidosEstadosLog");

            migrationBuilder.DropTable(
                name: "ProductoIngrediente");

            migrationBuilder.DropTable(
                name: "ProductosAlergenos");

            migrationBuilder.DropTable(
                name: "ProductosPreciosHistorial");

            migrationBuilder.DropTable(
                name: "PromocionesProductos");

            migrationBuilder.DropTable(
                name: "RecetasIngredientes");

            migrationBuilder.DropTable(
                name: "RestauranteConfigs");

            migrationBuilder.DropTable(
                name: "RolesPermisos");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "CierresDia");

            migrationBuilder.DropTable(
                name: "OrdenesCompra");

            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropTable(
                name: "Alergenos");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropTable(
                name: "Ingrediente");

            migrationBuilder.DropTable(
                name: "RecetasProductos");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Pedido");

            migrationBuilder.DropTable(
                name: "Proveedor");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Mesa");

            migrationBuilder.DropTable(
                name: "CategoriaProducto");

            migrationBuilder.DropTable(
                name: "ZonasSalon");
        }
    }
}
