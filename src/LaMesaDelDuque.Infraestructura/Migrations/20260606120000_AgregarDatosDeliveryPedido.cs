using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    [Migration("20260606120000_AgregarDatosDeliveryPedido")]
    public partial class AgregarDatosDeliveryPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClienteDeliveryNombre",
                table: "Pedido",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteDeliveryTelefono",
                table: "Pedido",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteDeliveryDireccion",
                table: "Pedido",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteDeliveryReferencia",
                table: "Pedido",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClienteDeliveryNotas",
                table: "Pedido",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ClienteDeliveryNombre", table: "Pedido");
            migrationBuilder.DropColumn(name: "ClienteDeliveryTelefono", table: "Pedido");
            migrationBuilder.DropColumn(name: "ClienteDeliveryDireccion", table: "Pedido");
            migrationBuilder.DropColumn(name: "ClienteDeliveryReferencia", table: "Pedido");
            migrationBuilder.DropColumn(name: "ClienteDeliveryNotas", table: "Pedido");
        }
    }
}
