using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
        var builder = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>();
        var connectionString = ConexionHelper.Normalizar(
            Environment.GetEnvironmentVariable("LMD_CONNECTION_STRING"));

        builder.UseNpgsql(connectionString);
        return new LaMesaDelDuqueDbContext(builder.Options);
    }
}
