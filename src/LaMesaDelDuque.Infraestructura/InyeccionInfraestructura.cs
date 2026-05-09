using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaMesaDelDuque.Infraestructura;

public static class InyeccionInfraestructura
{
    /// <summary>
    /// Registra los servicios de infraestructura (persistencia, repositorios, unidad de trabajo).
    /// Lanza InvalidOperationException si no hay connection string configurada.
    /// </summary>
    public static IServiceCollection AgregarPersistencia(
        this IServiceCollection servicios, IConfiguration configuracion)
    {
        var connectionString = configuracion.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' es obligatoria.");
        }

        servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
            opciones.UseNpgsql(connectionString));

        // Repositorios
        servicios.AddScoped<CategoriaProductoRepositorio>();
        servicios.AddScoped<ProductoRepositorio>();
        servicios.AddScoped<IngredienteRepositorio>();
        servicios.AddScoped<MesaRepositorio>();
        servicios.AddScoped<PedidoRepositorio>();
        servicios.AddScoped<RolRepositorio>();
        servicios.AddScoped<UsuarioRepositorio>();
        servicios.AddScoped<AuditoriaRepositorio>();
        servicios.AddScoped<RecetaProductoRepositorio>();

        // Unidad de Trabajo
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return servicios;
    }
}
