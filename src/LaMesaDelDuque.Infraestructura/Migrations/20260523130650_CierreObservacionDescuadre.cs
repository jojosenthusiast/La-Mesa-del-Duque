using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class CierreObservacionDescuadre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "CierresDia",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "CierresDia");
        }
    }
}
