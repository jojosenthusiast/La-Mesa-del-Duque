using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

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
}
