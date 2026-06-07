using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGuardasStockIngrediente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Ingrediente_StockActual_NoNegativo",
                table: "Ingrediente",
                sql: "\"StockActual\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ingrediente_StockActual_NoNegativo",
                table: "Ingrediente");
        }
    }
}
