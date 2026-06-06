using System;
using System.IO;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LaMesaDelDuque.Infraestructura;

public static class InyeccionInfraestructura
{
    public static IServiceCollection AgregarPersistencia(
        this IServiceCollection servicios, IConfiguration configuracion, bool esDesarrollo = false)
    {
        // Solo usar appsettings.json. Ignorar variable de entorno porque
        // persiste en sesiones de PowerShell y causa conexiones rotas a Supabase.
        var connectionString = configuracion.GetConnectionString("DefaultConnection");

        servicios.AddHttpContextAccessor();
        servicios.AddScoped<AuditoriaInterceptor>();

        servicios.AddDbContext<LaMesaDelDuqueDbContext>((sp, opciones) =>
        {
            opciones.AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>());

            if (esDesarrollo && string.IsNullOrWhiteSpace(connectionString))
            {
                var dbPath = Path.Combine(
                    AppContext.BaseDirectory, "..", "..", "..", "lmd-dev.db");
                opciones.UseSqlite($"Data Source={dbPath}");
                return;
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = ConexionHelper.Normalizar(connectionString);
                opciones.UseNpgsql(connectionString);
                return;
            }

            var dbPath2 = Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "lmd-dev.db");
            opciones.UseSqlite($"Data Source={dbPath2}");
        });

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
        servicios.AddScoped<ProveedorRepositorio>();
        servicios.AddScoped<MermaRepositorio>();
        servicios.AddScoped<CierreDiaRepositorio>();
        servicios.AddScoped<AlergenoRepositorio>();
        servicios.AddScoped<IAlergenoRepositorio, AlergenoRepositorio>();
        servicios.AddScoped<ZonaSalonRepositorio>();
        servicios.AddScoped<IMetricaRepositorio, MetricaRepositorio>();
        servicios.AddScoped<TurnoCajaRepositorio>();
        servicios.AddScoped<PromocionRepositorio>();
        servicios.AddScoped<DescuentoRepositorio>();
        servicios.AddScoped<MotivoDescuentoRepositorio>();
        servicios.AddScoped<DevolucionRepositorio>();

        // Unidad de Trabajo
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

        return servicios;
    }
}
