using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class CrearEsquemaInicialSprint1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriaProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    OrdenDisplay = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaProducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mesa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nit = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Contacto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Producto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TiempoPreparacionMin = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
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
                name: "Pedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MesaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_Mesa_MesaId",
                        column: x => x.MesaId,
                        principalTable: "Mesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingrediente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UnidadMedida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StockActual = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    StockMinimo = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ProveedorDefaultId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
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
                    RolId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermisoId = table.Column<Guid>(type: "uuid", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "DetallePedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: true)
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
                name: "ProductoIngrediente",
                columns: table => new
                {
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CantidadRequerida = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false)
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
                name: "PedidosEstadosLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                name: "ProductosPreciosHistorial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrecioAnterior = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PrecioNuevo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Razon = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaProducto_Nombre",
                table: "CategoriaProducto",
                column: "Nombre",
                unique: true);

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
                name: "IX_Mesa_Numero",
                table: "Mesa",
                column: "Numero",
                unique: true);

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
                name: "IX_Proveedor_Nit",
                table: "Proveedor",
                column: "Nit",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallePedido");

            migrationBuilder.DropTable(
                name: "PedidosEstadosLog");

            migrationBuilder.DropTable(
                name: "ProductoIngrediente");

            migrationBuilder.DropTable(
                name: "ProductosPreciosHistorial");

            migrationBuilder.DropTable(
                name: "RolesPermisos");

            migrationBuilder.DropTable(
                name: "Pedido");

            migrationBuilder.DropTable(
                name: "Ingrediente");

            migrationBuilder.DropTable(
                name: "Producto");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Mesa");

            migrationBuilder.DropTable(
                name: "Proveedor");

            migrationBuilder.DropTable(
                name: "CategoriaProducto");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
