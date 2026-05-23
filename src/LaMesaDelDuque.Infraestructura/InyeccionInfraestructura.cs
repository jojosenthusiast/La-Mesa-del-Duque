using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LaMesaDelDuque.Infraestructura;

public static class InyeccionInfraestructura
{
    public static IServiceCollection AgregarPersistencia(
        this IServiceCollection servicios, IConfiguration configuracion, IHostEnvironment ambiente)
    {
        var connectionString = configuracion.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (ambiente.IsProduction())
            {
                throw new InvalidOperationException("La cadena de conexión DefaultConnection no está configurada.");
            }

            servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
                opciones.UseSqlite("Data Source=la-mesa-del-duque-dev.db"));
        }
        else
        {
            connectionString = ConexionHelper.Normalizar(connectionString);
            servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
                opciones.UseNpgsql(connectionString));
        }

        return AgregarRepositorios(servicios);
    }

    public static IServiceCollection AgregarPersistencia(
        this IServiceCollection servicios, IConfiguration configuracion, bool esDesarrollo = false)
    {
        var connectionString = configuracion.GetConnectionString("DefaultConnection");

        if (esDesarrollo && string.IsNullOrWhiteSpace(connectionString))
        {
            servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
                opciones.UseSqlite("Data Source=la-mesa-del-duque-dev.db"));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión DefaultConnection no está configurada.");
            }

            connectionString = ConexionHelper.Normalizar(connectionString);
            servicios.AddDbContext<LaMesaDelDuqueDbContext>(opciones =>
                opciones.UseNpgsql(connectionString));
        }

        return AgregarRepositorios(servicios);
    }

    private static IServiceCollection AgregarRepositorios(IServiceCollection servicios)
    {

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
        servicios.AddScoped<OrdenCocinaRepositorio>();
        servicios.AddScoped<CuentaRepositorio>();
        servicios.AddScoped<PagoRepositorio>();
        servicios.AddScoped<AlergenoRepositorio>();
        servicios.AddScoped<IAlergenoRepositorio, AlergenoRepositorio>();
        servicios.AddScoped<ProveedorRepositorio>();
        servicios.AddScoped<IProveedorRepositorio, ProveedorRepositorio>();
        servicios.AddScoped<MermaRepositorio>();
        servicios.AddScoped<CierreDiaRepositorio>();

        // Unidad de Trabajo
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return servicios;
    }
}
