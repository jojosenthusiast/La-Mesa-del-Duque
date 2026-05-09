using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace LaMesaDelDuque.Aplicacion;

public static class InyeccionAplicacion
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AddScoped<ICatalogoProductosServicio, CatalogoProductosServicio>();
        servicios.AddScoped<IRecetasProductosServicio, RecetasProductosServicio>();
        servicios.AddScoped<IMesasServicio, MesasServicio>();
        servicios.AddScoped<IPedidosServicio, PedidosServicio>();
        servicios.AddScoped<IUsuariosServicio, UsuariosServicio>();

        return servicios;
    }
}
