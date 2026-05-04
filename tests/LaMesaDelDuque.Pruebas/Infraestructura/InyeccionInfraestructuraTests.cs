using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public class InyeccionInfraestructuraTests
{
    [Fact]
    public void AgregarPersistencia_SinConnectionString_DebeLanzarExcepcion()
    {
        var servicios = new ServiceCollection();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            servicios.AgregarPersistencia(configuracion));

        Assert.Contains("ConnectionStrings", ex.Message);
    }

    [Fact]
    public void AgregarPersistencia_ConConnectionStringVacio_DebeLanzarExcepcion()
    {
        var servicios = new ServiceCollection();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ""
            })
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            servicios.AgregarPersistencia(configuracion));

        Assert.Contains("ConnectionStrings", ex.Message);
    }

    [Fact]
    public void AgregarPersistencia_ConConnectionStringValido_DebeRegistrarDbContext()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();

        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
            })
            .Build();

        servicios.AgregarPersistencia(configuracion);
        var proveedor = servicios.BuildServiceProvider();

        var contexto = proveedor.GetService<LaMesaDelDuqueDbContext>();
        Assert.NotNull(contexto);
    }
}
