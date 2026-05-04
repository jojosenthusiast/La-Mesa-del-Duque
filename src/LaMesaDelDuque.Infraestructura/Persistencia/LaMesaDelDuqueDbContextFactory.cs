using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

/// <summary>
/// Fábrica en tiempo de diseño para que EF Core CLI (<c>dotnet ef migrations add</c>)
/// pueda crear migraciones sin necesidad de ejecutar la aplicación web completa.
/// Usa el mismo proveedor Npgsql que producción.
/// </summary>
internal class LaMesaDelDuqueDbContextFactory : IDesignTimeDbContextFactory<LaMesaDelDuqueDbContext>
{
    public LaMesaDelDuqueDbContext CreateDbContext(string[] args)
    {
        // Resolver la ruta base al proyecto Web desde el directorio de trabajo de la CLI
        var basePath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "LaMesaDelDuque.Web"));

        var configuracion = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine(basePath, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuracion.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                "No se encontró ConnectionStrings:DefaultConnection para la fábrica de tiempo de diseño. " +
                "Agréguela en src/LaMesaDelDuque.Web/appsettings.json o establezca la variable de entorno " +
                "ConnectionStrings__DefaultConnection.");
        }

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>();
        opciones.UseNpgsql(cadenaConexion);

        return new LaMesaDelDuqueDbContext(opciones.Options);
    }
}
