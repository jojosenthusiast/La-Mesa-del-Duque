using Microsoft.EntityFrameworkCore.Migrations;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Migrations;

public partial class MermaTipoLote : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>("Tipo", "MermaDiaria", maxLength: 30, nullable: false, defaultValue: "Otro");
        migrationBuilder.AddColumn<string>("Lote", "MermaDiaria", maxLength: 50, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("Tipo", "MermaDiaria");
        migrationBuilder.DropColumn("Lote", "MermaDiaria");
    }
}
