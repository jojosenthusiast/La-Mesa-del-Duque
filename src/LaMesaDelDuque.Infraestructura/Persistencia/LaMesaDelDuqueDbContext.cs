using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

public class LaMesaDelDuqueDbContext : DbContext
{
    public LaMesaDelDuqueDbContext(DbContextOptions<LaMesaDelDuqueDbContext> opciones)
        : base(opciones)
    {
    }

    protected override void OnModelCreating(ModelBuilder constructorDeModelos)
    {
        constructorDeModelos.ApplyConfigurationsFromAssembly(typeof(LaMesaDelDuqueDbContext).Assembly);
    }
}
