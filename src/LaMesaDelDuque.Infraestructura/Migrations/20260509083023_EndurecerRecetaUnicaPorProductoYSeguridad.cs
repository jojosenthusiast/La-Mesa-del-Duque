using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class EndurecerRecetaUnicaPorProductoYSeguridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecetasProductos_ProductoId",
                table: "RecetasProductos");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasProductos_ProductoId",
                table: "RecetasProductos",
                column: "ProductoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecetasProductos_ProductoId",
                table: "RecetasProductos");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasProductos_ProductoId",
                table: "RecetasProductos",
                column: "ProductoId");
        }
    }
}
