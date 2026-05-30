using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public sealed class PersistenciaModelTests
{
    [Fact]
    public void ModeloEf_ContieneCamposDeMermaYCierreAudit4()
    {
        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var contexto = new LaMesaDelDuqueDbContext(opciones);
        var modelo = contexto.Model;

        var merma = modelo.FindEntityType(typeof(MermaDiaria));
        Assert.NotNull(merma);
        Assert.NotNull(merma!.FindProperty("Tipo"));
        Assert.NotNull(merma.FindProperty("Lote"));

        var cierre = modelo.FindEntityType(typeof(CierreDia));
        Assert.NotNull(cierre);
        Assert.NotNull(cierre!.FindProperty("EfectivoReal"));
        Assert.NotNull(cierre.FindProperty("TarjetaReal"));
        Assert.NotNull(cierre.FindProperty("DiferenciaEfectivo"));
        Assert.NotNull(cierre.FindProperty("DiferenciaTarjeta"));
        Assert.NotNull(cierre.FindProperty("EsCerrado"));
        Assert.NotNull(cierre.FindProperty("CerradoEn"));
    }

    [Fact]
    public void ModeloEf_Pedido_TieneCheckConstraintParaDomicilio()
    {
        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var contexto = new LaMesaDelDuqueDbContext(opciones);

        var modeloDiseno = contexto.GetService<IDesignTimeModel>().Model;
        var pedido = modeloDiseno.FindEntityType(typeof(Pedido));
        Assert.NotNull(pedido);

        var constraint = Assert.Single(
            pedido!.GetCheckConstraints(),
            c => c.Name == "CK_Pedido_Domicilio_DatosEntrega");

        Assert.Contains("\"TipoServicio\" <> 'Domicilio'", constraint.Sql);
        Assert.Contains("\"MesaId\" IS NULL", constraint.Sql);
        Assert.Contains("\"NombreClienteEntrega\" IS NOT NULL", constraint.Sql);
        Assert.Contains("length(trim(\"NombreClienteEntrega\")) > 0", constraint.Sql);
        Assert.Contains("\"TelefonoEntrega\" IS NOT NULL", constraint.Sql);
        Assert.Contains("length(trim(\"TelefonoEntrega\")) > 0", constraint.Sql);
        Assert.Contains("\"DireccionEntrega\" IS NOT NULL", constraint.Sql);
        Assert.Contains("length(trim(\"DireccionEntrega\")) > 0", constraint.Sql);
    }

}
