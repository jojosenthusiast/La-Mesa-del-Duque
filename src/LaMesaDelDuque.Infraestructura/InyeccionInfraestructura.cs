using System;
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
        var connectionString = Environment.GetEnvironmentVariable("LMD_CONNECTION_STRING")
            ?? configuracion.GetConnectionString("DefaultConnection");

        connectionString = ConexionHelper.Normalizar(connectionString);

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
