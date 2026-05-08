using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public class InyeccionInfraestructuraTests
{
    private const string MensajeConnectionStringObligatoria = "La cadena de conexión 'DefaultConnection' es obligatoria.";

    [Fact]
    public void AgregarPersistencia_SinConnectionString_DebeLanzarExcepcion()
    {
        var servicios = new ServiceCollection();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            servicios.AgregarPersistencia(configuracion));

        Assert.Equal(MensajeConnectionStringObligatoria, ex.Message);
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

        Assert.Equal(MensajeConnectionStringObligatoria, ex.Message);
    }

    [Fact]
    public void AgregarPersistencia_ConConnectionStringSoloEspacios_DebeLanzarExcepcion()
    {
        var servicios = new ServiceCollection();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "   "
            })
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            servicios.AgregarPersistencia(configuracion));

        Assert.Equal(MensajeConnectionStringObligatoria, ex.Message);
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

    [Fact]
    public void AgregarPersistencia_DebeUsarProveedorEsperadoYConnectionStringNombrado()
    {
        const string expectedConnectionString = "Host=localhost;Database=lmd_test;Username=test;Password=test123";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:OtraConnection"] = "Host=localhost;Database=otro;Username=test;Password=test123",
                ["ConnectionStrings:DefaultConnection"] = expectedConnectionString
            })
            .Build();

        var services = new ServiceCollection();

        services.AgregarPersistencia(configuration);
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<DbContextOptions<LaMesaDelDuqueDbContext>>();
        Assert.NotNull(options);

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LaMesaDelDuqueDbContext>();

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
        Assert.Equal(expectedConnectionString, context.Database.GetDbConnection().ConnectionString);
    }
}
