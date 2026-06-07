using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMeseroAsignadoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MeseroAsignadoId",
                table: "Pedido",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_MeseroAsignadoId",
                table: "Pedido",
                column: "MeseroAsignadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Usuarios_MeseroAsignadoId",
                table: "Pedido",
                column: "MeseroAsignadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Usuarios_MeseroAsignadoId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_MeseroAsignadoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "MeseroAsignadoId",
                table: "Pedido");
        }
    }
}
