using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

public class LaMesaDelDuqueDbContextFactory : IDesignTimeDbContextFactory<LaMesaDelDuqueDbContext>
{
    public LaMesaDelDuqueDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>();
        optionsBuilder.UseSqlite("Data Source=lmd-migrations.db");
        return new LaMesaDelDuqueDbContext(optionsBuilder.Options);
    }
}
