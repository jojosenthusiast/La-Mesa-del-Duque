using Microsoft.EntityFrameworkCore.Migrations;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Migrations;

public partial class AgregarAlergenos : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Alergenos", table => new
        {
            Id = table.Column<Guid>(nullable: false),
            Nombre = table.Column<string>(maxLength: 100, nullable: false),
            Icono = table.Column<string>(maxLength: 50, nullable: true),
            Activo = table.Column<bool>(nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Alergenos", x => x.Id);
        });

        migrationBuilder.CreateTable("ProductosAlergenos", table => new
        {
            Id = table.Column<Guid>(nullable: false),
            ProductoId = table.Column<Guid>(nullable: false),
            AlergenoId = table.Column<Guid>(nullable: false),
            Justificacion = table.Column<string>(maxLength: 200, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ProductosAlergenos", x => x.Id);
            table.ForeignKey("FK_ProductosAlergenos_Productos", x => x.ProductoId, "Productos", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_ProductosAlergenos_Alergenos", x => x.AlergenoId, "Alergenos", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_ProductosAlergenos_ProductoId", "ProductosAlergenos", "ProductoId");
        migrationBuilder.CreateIndex("IX_ProductosAlergenos_AlergenoId", "ProductosAlergenos", "AlergenoId");

        // Seed data: alérgenos comunes en cocina LATAM
        InsertAlergeno(migrationBuilder, "Mariscos", "shellfish");
        InsertAlergeno(migrationBuilder, "Lácteos", "milk");
        InsertAlergeno(migrationBuilder, "Gluten", "wheat");
        InsertAlergeno(migrationBuilder, "Maní", "peanut");
        InsertAlergeno(migrationBuilder, "Soja", "soy");
        InsertAlergeno(migrationBuilder, "Huevo", "egg");
        InsertAlergeno(migrationBuilder, "Sulfitos", "wine");
        InsertAlergeno(migrationBuilder, "Frutos secos", "tree-nut");
        InsertAlergeno(migrationBuilder, "Pescado", "fish");
        InsertAlergeno(migrationBuilder, "Aceite de pescado",null);
    }

    private static void InsertAlergeno(MigrationBuilder migrationBuilder, string nombre, string? icono)
    {
        migrationBuilder.InsertData("Alergenos",
            ["Id", "Nombre", "Icono", "Activo"],
            [Guid.NewGuid(), nombre, icono, true]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ProductosAlergenos");
        migrationBuilder.DropTable("Alergenos");
    }
}
