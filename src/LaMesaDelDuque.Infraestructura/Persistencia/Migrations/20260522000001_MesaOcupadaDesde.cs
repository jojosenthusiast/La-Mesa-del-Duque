using Microsoft.EntityFrameworkCore.Migrations;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Migrations;

public partial class MesaOcupadaDesde : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>("OcupadaDesde", "Mesa", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("OcupadaDesde", "Mesa");
    }
}
