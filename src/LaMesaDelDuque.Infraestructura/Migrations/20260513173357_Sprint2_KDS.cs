using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaMesaDelDuque.Infraestructura.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_KDS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstacionCocina",
                table: "CategoriaProducto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrdenesCocina",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetallePedidoId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductoNombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Notas = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Estacion = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    HoraRecibido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HoraListo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MesaNumero = table.Column<int>(type: "integer", nullable: true),
                    TipoServicio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCocina", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCocina_Estacion_Estado",
                table: "OrdenesCocina",
                columns: new[] { "Estacion", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCocina_Estado_HoraRecibido",
                table: "OrdenesCocina",
                columns: new[] { "Estado", "HoraRecibido" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesCocina");

            migrationBuilder.DropColumn(
                name: "EstacionCocina",
                table: "CategoriaProducto");
        }
    }
}
