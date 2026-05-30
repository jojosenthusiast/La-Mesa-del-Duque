using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

public class LaMesaDelDuqueDbContextFactory : IDesignTimeDbContextFactory<LaMesaDelDuqueDbContext>
{
    public LaMesaDelDuqueDbContext CreateDbContext(string[] args)
    {
        var assemblyDir = Path.GetDirectoryName(typeof(LaMesaDelDuqueDbContextFactory).Assembly.Location)!;
        var webDir = Path.GetFullPath(Path.Combine(assemblyDir, "../../../../LaMesaDelDuque.Web"));

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(webDir) ? webDir : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>();

        if (!string.IsNullOrWhiteSpace(cs))
        {
            cs = ConexionHelper.Normalizar(cs);
            optionsBuilder.UseNpgsql(cs);
        }
        else
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=lmd_migrations;Username=postgres;Password=postgres");
        }

        return new LaMesaDelDuqueDbContext(optionsBuilder.Options);
    }
}
