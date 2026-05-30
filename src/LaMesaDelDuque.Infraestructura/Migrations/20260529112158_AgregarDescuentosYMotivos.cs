using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDescuentosYMotivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevolucionesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PagoOriginalId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontoDevuelto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MetodoDevolucion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MotivoDevolucion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UsuarioSolicitaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioAutorizaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StockReintegrado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesPago_Pagos_PagoOriginalId",
                        column: x => x.PagoOriginalId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MotivosDescuento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosDescuento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DescuentosAplicados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "uuid", nullable: true),
                    MotivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoDescuento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoAplicado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UsuarioSolicitaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioAutorizaId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotaAutorizador = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescuentosAplicados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescuentosAplicados_MotivosDescuento_MotivoId",
                        column: x => x.MotivoId,
                        principalTable: "MotivosDescuento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DescuentosAplicados_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DescuentosAplicados_Estado",
                table: "DescuentosAplicados",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_DescuentosAplicados_MotivoId",
                table: "DescuentosAplicados",
                column: "MotivoId");

            migrationBuilder.CreateIndex(
                name: "IX_DescuentosAplicados_PedidoId",
                table: "DescuentosAplicados",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesPago_FechaHora",
                table: "DevolucionesPago",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesPago_PagoOriginalId",
                table: "DevolucionesPago",
                column: "PagoOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_MotivosDescuento_Nombre",
                table: "MotivosDescuento",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescuentosAplicados");

            migrationBuilder.DropTable(
                name: "DevolucionesPago");

            migrationBuilder.DropTable(
                name: "MotivosDescuento");
        }
    }
}
