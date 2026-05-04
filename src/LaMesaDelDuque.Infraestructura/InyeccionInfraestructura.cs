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
        var cadenaConexion = configuracion.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                "No se encontró ConnectionStrings:DefaultConnection en la configuración. " +
                "Agréguela en appsettings.json o en variables de entorno.");
        }

        servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
            opciones.UseNpgsql(cadenaConexion));

        // Repositorios
        servicios.AddScoped<CategoriaProductoRepositorio>();
        servicios.AddScoped<ProductoRepositorio>();
        servicios.AddScoped<MesaRepositorio>();
        servicios.AddScoped<PedidoRepositorio>();

        // Unidad de Trabajo
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return servicios;
    }
}
