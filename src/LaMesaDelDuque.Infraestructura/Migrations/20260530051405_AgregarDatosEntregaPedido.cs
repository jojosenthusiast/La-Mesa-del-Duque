using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosEntregaPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionEntrega",
                table: "Pedido",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreClienteEntrega",
                table: "Pedido",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaEntrega",
                table: "Pedido",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoEntrega",
                table: "Pedido",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Pedido_Domicilio_DatosEntrega",
                table: "Pedido",
                sql: "\"TipoServicio\" <> 'Domicilio' OR (\"MesaId\" IS NULL AND \"NombreClienteEntrega\" IS NOT NULL AND length(trim(\"NombreClienteEntrega\")) > 0 AND \"TelefonoEntrega\" IS NOT NULL AND length(trim(\"TelefonoEntrega\")) > 0 AND \"DireccionEntrega\" IS NOT NULL AND length(trim(\"DireccionEntrega\")) > 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Pedido_Domicilio_DatosEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DireccionEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "NombreClienteEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "ReferenciaEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "TelefonoEntrega",
                table: "Pedido");
        }
    }
}
