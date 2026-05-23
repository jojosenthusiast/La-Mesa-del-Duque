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
        servicios.AddScoped<ICocinaServicio, CocinaServicio>();
        servicios.AddScoped<IAlertaStockServicio, AlertaStockServicio>();
        servicios.AddScoped<ITicketServicio, TicketServicio>();
        servicios.AddScoped<IAlergenoServicio, AlergenoServicio>();
        servicios.AddScoped<ITableTimerServicio, TableTimerServicio>();
        servicios.AddScoped<IUpsellServicio, UpsellServicio>();
        servicios.AddScoped<IShiftHandoffServicio, ShiftHandoffServicio>();
        servicios.AddScoped<IInventarioServicio, InventarioServicio>();
        servicios.AddScoped<IMargenServicio, MargenServicio>();
        servicios.AddScoped<IMermaServicio, MermaServicio>();
        servicios.AddScoped<ICierreServicio, CierreServicio>();
        servicios.AddScoped<IDespachoServicio, DespachoServicio>();
        servicios.AddScoped<IZonasSalonServicio, ZonasSalonServicio>();

        return servicios;
    }
}
