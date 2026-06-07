using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRepartidorPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AsignadoEn",
                table: "Pedido",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EntregadoEn",
                table: "Pedido",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RepartidorId",
                table: "Pedido",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsignadoEn",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EntregadoEn",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "RepartidorId",
                table: "Pedido");
        }
    }
}
