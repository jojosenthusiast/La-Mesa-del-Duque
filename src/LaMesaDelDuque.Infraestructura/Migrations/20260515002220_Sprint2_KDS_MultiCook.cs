using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_KDS_MultiCook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alergenos",
                table: "OrdenesCocina",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CocineroId",
                table: "OrdenesCocina",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientesExtra",
                table: "OrdenesCocina",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientesQuitados",
                table: "OrdenesCocina",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "DetallePedido",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alergenos",
                table: "OrdenesCocina");

            migrationBuilder.DropColumn(
                name: "CocineroId",
                table: "OrdenesCocina");

            migrationBuilder.DropColumn(
                name: "IngredientesExtra",
                table: "OrdenesCocina");

            migrationBuilder.DropColumn(
                name: "IngredientesQuitados",
                table: "OrdenesCocina");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "DetallePedido");
        }
    }
}
