using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class Audit4CamposCierreMermaPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EfectivoReal",
                table: "CierresDia",
                type: "TEXT",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TarjetaReal",
                table: "CierresDia",
                type: "TEXT",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiferenciaEfectivo",
                table: "CierresDia",
                type: "TEXT",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiferenciaTarjeta",
                table: "CierresDia",
                type: "TEXT",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EsCerrado",
                table: "CierresDia",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CerradoEn",
                table: "CierresDia",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OcupadaDesde",
                table: "Mesa",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Pagos",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Cuentas",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

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

            migrationBuilder.CreateIndex(
                name: "IX_CuentaDetalle_CuentaId",
                table: "CuentaDetalle",
                column: "CuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAlergenos_AlergenoId",
                table: "ProductosAlergenos",
                column: "AlergenoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosAlergenos_ProductoId",
                table: "ProductosAlergenos",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentaDetalle");

            migrationBuilder.DropTable(
                name: "ProductosAlergenos");

            migrationBuilder.DropTable(
                name: "Alergenos");

            migrationBuilder.DropColumn(
                name: "EfectivoReal",
                table: "CierresDia");

            migrationBuilder.DropColumn(
                name: "TarjetaReal",
                table: "CierresDia");

            migrationBuilder.DropColumn(
                name: "DiferenciaEfectivo",
                table: "CierresDia");

            migrationBuilder.DropColumn(
                name: "DiferenciaTarjeta",
                table: "CierresDia");

            migrationBuilder.DropColumn(
                name: "EsCerrado",
                table: "CierresDia");

            migrationBuilder.DropColumn(
                name: "CerradoEn",
                table: "CierresDia");

            migrationBuilder.DropColumn(
                name: "OcupadaDesde",
                table: "Mesa");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Cuentas");
        }
    }
}
