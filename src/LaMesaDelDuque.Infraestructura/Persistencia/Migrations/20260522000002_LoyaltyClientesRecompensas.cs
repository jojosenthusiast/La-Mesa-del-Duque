using Microsoft.EntityFrameworkCore.Migrations;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Migrations;

public partial class LoyaltyClientesRecompensas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Clientes", table => new
        {
            Id = table.Column<Guid>(nullable: false),
            Nombre = table.Column<string>(maxLength: 200, nullable: false),
            Telefono = table.Column<string>(maxLength: 20, nullable: false),
            Notas = table.Column<string>(maxLength: 500, nullable: true),
            Activo = table.Column<bool>(nullable: false),
            PuntosAcumulados = table.Column<int>(nullable: false),
            VisitasTotales = table.Column<int>(nullable: false),
            CreadoEn = table.Column<DateTime>(nullable: false),
            UltimaVisita = table.Column<DateTime>(nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_Clientes", x => x.Id); });

        migrationBuilder.CreateTable("Recompensas", table => new
        {
            Id = table.Column<Guid>(nullable: false),
            Nombre = table.Column<string>(maxLength: 100, nullable: false),
            Descripcion = table.Column<string>(maxLength: 300, nullable: true),
            PuntosRequeridos = table.Column<int>(nullable: false),
            Activo = table.Column<bool>(nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_Recompensas", x => x.Id); });

        // Seed 3 recompensas básicas
        migrationBuilder.InsertData("Recompensas",
            ["Id", "Nombre", "Descripcion", "PuntosRequeridos", "Activo"],
            [Guid.NewGuid(), "Postre gratis", "Un postre a elección", 500, true]);
        migrationBuilder.InsertData("Recompensas",
            ["Id", "Nombre", "Descripcion", "PuntosRequeridos", "Activo"],
            [Guid.NewGuid(), "Bebida gratis", "Una bebida a elección", 300, true]);
        migrationBuilder.InsertData("Recompensas",
            ["Id", "Nombre", "Descripcion", "PuntosRequeridos", "Activo"],
            [Guid.NewGuid(), "10% descuento", "Descuento en toda la cuenta", 800, true]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Recompensas");
        migrationBuilder.DropTable("Clientes");
    }
}
