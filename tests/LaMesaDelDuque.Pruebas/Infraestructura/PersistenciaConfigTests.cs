using LaMesaDelDuque.Infraestructura;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LaMesaDelDuque.Pruebas.Infraestructura;

public sealed class PersistenciaConfigTests
{
    [Fact]
    public void AgregarPersistencia_DevelopmentSinConnectionString_UsaSqliteLocal()
    {
        var servicios = new ServiceCollection();

        var configuracion = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ""
            })
            .Build();

        var ambiente = new FakeHostEnvironment { EnvironmentName = Environments.Development };

        servicios.AgregarPersistencia(configuracion, true);

        using var proveedor = servicios.BuildServiceProvider();
        using var scope = proveedor.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<LaMesaDelDuqueDbContext>();

        Assert.True(contexto.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "LaMesaDelDuque.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
