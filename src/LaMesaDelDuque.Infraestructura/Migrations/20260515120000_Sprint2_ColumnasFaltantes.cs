using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_ColumnasFaltantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OrdenesCocina: columnas presentes en el modelo pero ausentes en la BD
            migrationBuilder.AddColumn<string>(
                name: "Curso",
                table: "OrdenesCocina",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductoId",
                table: "OrdenesCocina",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<int>(
                name: "TiempoPreparacionMin",
                table: "OrdenesCocina",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // DetallePedido: ModificacionesJson para modificadores/notas estructuradas
            migrationBuilder.AddColumn<string>(
                name: "ModificacionesJson",
                table: "DetallePedido",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Curso", table: "OrdenesCocina");
            migrationBuilder.DropColumn(name: "ProductoId", table: "OrdenesCocina");
            migrationBuilder.DropColumn(name: "TiempoPreparacionMin", table: "OrdenesCocina");
            migrationBuilder.DropColumn(name: "ModificacionesJson", table: "DetallePedido");
        }
    }
}
