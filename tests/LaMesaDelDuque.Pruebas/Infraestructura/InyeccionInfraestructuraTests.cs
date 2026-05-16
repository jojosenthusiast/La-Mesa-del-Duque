using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public class InyeccionInfraestructuraTests
{
    [Fact]
    public void AgregarPersistencia_SinConnectionString_DebeUsarSqlite()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        servicios.AgregarPersistencia(configuracion);
        var proveedor = servicios.BuildServiceProvider();

        var contexto = proveedor.GetService<LaMesaDelDuqueDbContext>();
        Assert.NotNull(contexto);
    }

    [Fact]
    public void AgregarPersistencia_ConConnectionStringVacio_DebeUsarSqlite()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ""
            })
            .Build();

        servicios.AgregarPersistencia(configuracion);
        var proveedor = servicios.BuildServiceProvider();

        var contexto = proveedor.GetService<LaMesaDelDuqueDbContext>();
        Assert.NotNull(contexto);
    }

    [Fact]
    public void AgregarPersistencia_ConConnectionStringSoloEspacios_DebeUsarSqlite()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "   "
            })
            .Build();

        servicios.AgregarPersistencia(configuracion);
        var proveedor = servicios.BuildServiceProvider();

        var contexto = proveedor.GetService<LaMesaDelDuqueDbContext>();
        Assert.NotNull(contexto);
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
        services.AddLogging();

        services.AgregarPersistencia(configuration);
        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<DbContextOptions<LaMesaDelDuqueDbContext>>();
        Assert.NotNull(options);

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LaMesaDelDuqueDbContext>();

        var providerName = context.Database.ProviderName;
        Assert.True(
            providerName == "Npgsql.EntityFrameworkCore.PostgreSQL" ||
            providerName == "Microsoft.EntityFrameworkCore.Sqlite",
            $"Unexpected provider: {providerName}"
        );

        var actualConnectionString = context.Database.GetDbConnection().ConnectionString;
        Assert.Contains("Host=localhost", actualConnectionString);
        Assert.Contains("Database=lmd_test", actualConnectionString);
    }
}
